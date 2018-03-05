//-----------------------------------------------------------------------
// <copyright file="ConsoleView.xaml.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using ConsoleHoster.View.Utilities;
using ConsoleHoster.ViewModel;
using System.Linq;
using ConsoleHoster.Model;

namespace ConsoleHoster.View
{
	/// <summary>
	/// Interaction logic for ConsoleView.xaml
	/// </summary>
	public partial class ConsoleView : UserControl
	{
		public ConsoleView()
		{
			InitializeComponent();
		}

		private void txbHistory_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (this.ViewModel != null && this.ViewModel.AutoScroll)
			{
				(sender as TextBoxBase).ScrollToEnd();
			}
		}

		private void txtCommand_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			switch (e.Key)
			{
				case System.Windows.Input.Key.Down:
					if (!this.ViewModel.IsMultilineCommand)
					{
						this.RetrieveHistoryCommand(true);
						e.Handled = true;
					}
					break;

				case System.Windows.Input.Key.Up:
					if (!this.ViewModel.IsMultilineCommand)
					{
						this.RetrieveHistoryCommand(false);
						e.Handled = true;
					}
					break;

				case Key.Enter:
					if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
					{
						int tmpCaretIndex = this.txtCommand.CaretIndex;
						this.ViewModel.CurrentCommand = this.ViewModel.CurrentCommand.Insert(tmpCaretIndex, Environment.NewLine);
						this.txtCommand.CaretIndex = tmpCaretIndex + 2;
					}
					else
					{
						this.ViewModel.RunTypedCommandCommand.Execute(null);
						e.Handled = true;
					}
					break;

				case Key.Tab:
					string tmpCurrentCommand = this.ViewModel.CurrentCommand ?? String.Empty;
					this.ViewModel.CurrentCommand = tmpCurrentCommand.Insert(this.txtCommand.CaretIndex, "\t");
					e.Handled = true;
					break;
			}
		}

		private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case System.Windows.Input.Key.F:
					if (Keyboard.IsKeyDown(Key.LeftCtrl))
					{
						this.ViewModel.SearchVM.SearchEnabled = true;
						e.Handled = true;
					}
					break;

				case Key.Escape:
					this.ViewModel.CurrentCommand = String.Empty;
					e.Handled = true;
					break;
			}
		}

		private void RetrieveHistoryCommand(bool argRetrieveNext)
		{
			this.ViewModel.RetrieveRecentCommand.Execute(argRetrieveNext);
			this.txtCommand.SelectionStart = this.txtCommand.Text.Length;
		}

		private void UserControl_DataContextChanged_1(object sender, DependencyPropertyChangedEventArgs e)
		{
			ConsoleViewModel tmpOldVM = e.OldValue as ConsoleViewModel;
			if (tmpOldVM != null)
			{
				tmpOldVM.SearchVM.FindNext -= OnSearchVM_FindNext;
			}

			ConsoleViewModel tmpNewVM = e.NewValue as ConsoleViewModel;
			if (tmpNewVM != null)
			{
				tmpNewVM.SearchVM.FindNext += OnSearchVM_FindNext;
			}

			this.txtCommand.Focus();
		}

		private void UserControl_SizeChanged_1(object sender, SizeChangedEventArgs e)
		{
			try
			{
				this.mctHistory.ScrollToEnd();
			}
			catch
			{
			}
		}

		private void OnSearchVM_FindNext(string argTextToFind)
		{
			this.mctHistory.FindNext(argTextToFind);
		}

		private void mctHistory_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			this.Zoom(e);
		}

		private void txtCommand_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			this.Zoom(e);
		}

		private void Zoom(MouseWheelEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl))
			{
				this.ViewModel.ZoomLevel += e.Delta > 0 ? 5 : -5;
				e.Handled = true;
			}
		}

		private void txtCommand_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (this.ViewModel != null && this.ViewModel.SuggestionProvided)
			{
				this.txtCommand.SelectionStart = this.txtCommand.Text.Length;
				this.ViewModel.SuggestionProvided = false;
			}
		}

		private ConsoleViewModel ViewModel
		{
			get
			{
				return this.DataContext as ConsoleViewModel;
			}
		}
	}
}