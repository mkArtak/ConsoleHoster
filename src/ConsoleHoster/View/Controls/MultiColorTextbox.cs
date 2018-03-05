//-----------------------------------------------------------------------
// <copyright file="MultiColorTextbox.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ConsoleHoster.View.Utilities;
using ConsoleHoster.Common.Utilities;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Diagnostics;

namespace ConsoleHoster.View.Controls
{
	public class MultiColorTextbox : RichTextBox
	{
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(ObservableCollection<KeyValuePair<string, Color>>), typeof(MultiColorTextbox), new PropertyMetadata(null, new PropertyChangedCallback(OnTextPropertyChanged)));

		private FlowDocumentHelper documentHelper = null;
		private string textToFind = null;
		private int searchOccurances = 0;
		private bool callbackExpected = false;

		private static void OnTextPropertyChanged(DependencyObject argSender, DependencyPropertyChangedEventArgs argEA)
		{
			MultiColorTextbox tmpSender = argSender as MultiColorTextbox;

			tmpSender.Dispatcher.Invoke(new Action(() =>
			{
				ObservableCollection<KeyValuePair<string, Color>> tmpOldValue = argEA.OldValue as ObservableCollection<KeyValuePair<string, Color>>;
				ObservableCollection<KeyValuePair<string, Color>> tmpNewValue = argEA.NewValue as ObservableCollection<KeyValuePair<string, Color>>;
				if (tmpOldValue != tmpNewValue)
				{
					if (tmpOldValue != null)
					{
						tmpSender.UnRegisterTextChangeHandler(tmpOldValue);
					}

					tmpSender.AddTextItems(tmpNewValue, true);

					if (tmpNewValue != null)
					{
						tmpSender.RegisterTextChangeHandler(tmpNewValue);
					}
				}
			}));
		}

		public MultiColorTextbox()
			: base()
		{
		}

		private void OnTextCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (Dispatcher.Thread != Thread.CurrentThread)
			{
				Dispatcher.Invoke(new Action(() => this.OnTextCollectionChanged(sender, e)));
				return;
			}

			if (this.callbackExpected)
			{
				this.callbackExpected = false;
				return;
			}

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					IList<KeyValuePair<string, Color>> tmpList = e.NewItems.OfType<KeyValuePair<string, Color>>().ToList();
					this.AddTextItems(tmpList, false);
					break;

				case NotifyCollectionChangedAction.Move:
					break;

				case NotifyCollectionChangedAction.Remove:
					this.RemoveTextItems(e.OldItems.OfType<KeyValuePair<string, Color>>().ToList());
					break;

				case NotifyCollectionChangedAction.Replace:
					break;

				case NotifyCollectionChangedAction.Reset:
					this.callbackExpected = true;
					this.Text.Clear();
					this.SingleParagraph.Inlines.Clear();
					break;
			}
		}

		private void AddTextItems(IList<KeyValuePair<string, Color>> items, bool argResetBeforAdd)
		{
			this.AddTextInternal(items, argResetBeforAdd);
		}

		private void RemoveTextItems(IList<KeyValuePair<string, Color>> argItemsToRemove)
		{
			this.RemoveItemInternal(argItemsToRemove);
		}

		private void AddTextInternal(IList<KeyValuePair<string, Color>> items, bool argResetBeforAdd)
		{
			if (argResetBeforAdd)
			{
				this.SingleParagraph.Inlines.Clear();
			}

			if (items != null)
			{
				int tmpCount = items.Count();
				for (int i = 0; i < tmpCount; i++)
				{
					KeyValuePair<string, Color> tmpItem = items[i];
					Paragraph tmpParagraph = this.SingleParagraph;
					string tmpPattern = @"((ftp|https?)://|www.)[-a-zA-Z0-9@:%_\\\+.~#?&//=]+";
					if (Regex.IsMatch(tmpItem.Key, tmpPattern))
					{
						MatchCollection tmpMatches = Regex.Matches(tmpItem.Key, tmpPattern);

						int tmpLastPosition = 0;
						foreach (Match tmpMatch in tmpMatches)
						{
							if (tmpMatch.Index > tmpLastPosition)
							{
								Run tmpPart = new Run(tmpItem.Key.Substring(tmpLastPosition, tmpMatch.Index - tmpLastPosition));
								tmpPart.Foreground = new SolidColorBrush(tmpItem.Value);
								tmpParagraph.Inlines.Add(tmpPart);
							}

							Hyperlink tmpHyperlink = new Hyperlink(new Run(tmpMatch.Value));
							tmpHyperlink.NavigateUri = new Uri(tmpMatch.Value, UriKind.RelativeOrAbsolute);
							tmpHyperlink.Foreground = new SolidColorBrush(tmpItem.Value);
							tmpHyperlink.Cursor = Cursors.Hand;
							tmpHyperlink.ToolTip = "Use [Ctrl]+Click to navigate";
							tmpHyperlink.PreviewMouseLeftButtonUp += this.OnHyperlink_PreviewMouseLeftButtonUp;
							tmpParagraph.Inlines.Add(tmpHyperlink);

							tmpLastPosition = tmpMatch.Index + tmpMatch.Length;
						}

						if (tmpLastPosition < tmpItem.Key.Length)
						{
							Run tmpPart = new Run(tmpItem.Key.Substring(tmpLastPosition));
							tmpPart.Foreground = new SolidColorBrush(tmpItem.Value);
							tmpParagraph.Inlines.Add(tmpPart);
						}
					}
					else
					{
						tmpParagraph.Inlines.Add(new Run(tmpItem.Key)
						{
							Foreground = new System.Windows.Media.SolidColorBrush(tmpItem.Value)
						});
					}
				}
			}
		}

		private void OnHyperlink_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
			{
				Hyperlink tmpLink = sender as Hyperlink;
				try
				{
					Process.Start(tmpLink.NavigateUri.ToString());
				}
				catch (Exception ex)
				{
					SimpleFileLogger.Instance.LogError(String.Format("Failed to navigate to url: {0}", tmpLink.NavigateUri), ex);
				}
			}
		}

		private void RemoveItemInternal(IList<KeyValuePair<string, Color>> argItemsToRemove)
		{
			int tmpCount = argItemsToRemove.Count();
			for (int i = 0; i < tmpCount; i++)
			{
				KeyValuePair<string, Color> tmpItem = argItemsToRemove[i];
				Run tmpInline = this.SingleParagraph.Inlines.OfType<Run>().Where(item => item.Text == tmpItem.Key && (item.Foreground as SolidColorBrush).Color == tmpItem.Value).FirstOrDefault();
				if (tmpInline != null)
				{
					Inline tmpLB = this.SingleParagraph.Inlines.SkipWhile(item => item != tmpInline).Skip(1).Take(1).SingleOrDefault();
					this.SingleParagraph.Inlines.Remove(tmpInline);
					if (tmpLB != null && tmpLB is LineBreak)
					{
						this.SingleParagraph.Inlines.Remove(tmpLB);
					}
				}
			}
		}

		private void UnRegisterTextChangeHandler(ObservableCollection<KeyValuePair<string, Color>> argCollection)
		{
			argCollection.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.OnTextCollectionChanged);
		}

		private void RegisterTextChangeHandler(ObservableCollection<KeyValuePair<string, Color>> argCollection)
		{
			argCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnTextCollectionChanged);
		}

		public void FindNext(string argTextToFind)
		{
			if ((!String.IsNullOrEmpty(argTextToFind) && argTextToFind != this.TextToFind))
			{
				this.DocumentHelper.CurrentPosition = this.Document.ContentStart;
			}
			this.TextToFind = argTextToFind;

			if (!String.IsNullOrEmpty(this.TextToFind))
			{
				TextRange tmpRange = this.DocumentHelper.FindNext(argTextToFind, FindOptions.None);
				if (tmpRange != null)
				{
					this.Focus();
					this.Selection.Select(tmpRange.Start, tmpRange.End);
				}
				else
				{
					// Reached the end of the document, start from the top
					this.DocumentHelper.CurrentPosition = this.Document.ContentStart;
				}
			}
		}

		private int CountSearchOccurances()
		{
			int tmpResult = 0;
			this.DocumentHelper.CurrentPosition = this.Document.ContentStart;

			if (!String.IsNullOrEmpty(this.TextToFind))
			{
				TextRange tmpRange;
				while ((tmpRange = this.DocumentHelper.FindNext(this.TextToFind, FindOptions.None)) != null)
				{
					tmpResult++;
				}
			}

			return tmpResult;
		}

		public ObservableCollection<KeyValuePair<string, Color>> Text
		{
			get
			{
				return this.GetValue(TextProperty) as ObservableCollection<KeyValuePair<string, Color>>;
			}
			set
			{
				this.SetValue(TextProperty, value);
			}
		}

		private Paragraph SingleParagraph
		{
			get
			{
				return this.Document.Blocks.FirstBlock as Paragraph;
			}
		}

		public string TextToFind
		{
			get
			{
				return this.textToFind;
			}
			set
			{
				if (value != this.textToFind)
				{
					this.textToFind = value;
					this.DocumentHelper.CurrentPosition = this.Document.ContentStart;
					this.SearchOccurances = this.CountSearchOccurances();
				}
			}
		}

		private FlowDocumentHelper DocumentHelper
		{
			get
			{
				if (this.documentHelper == null)
				{
					this.documentHelper = new FlowDocumentHelper(this.Document);
				}
				return this.documentHelper;
			}
		}

		public int SearchOccurances
		{
			get
			{
				return this.searchOccurances;
			}

			private set
			{
				this.searchOccurances = value;
			}
		}
	}
}