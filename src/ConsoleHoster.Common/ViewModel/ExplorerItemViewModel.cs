//-----------------------------------------------------------------------
// <copyright file="ExplorerItemViewModel.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using ConsoleHoster.Common.Model;
using ConsoleHoster.Common.Utilities;

namespace ConsoleHoster.Common.ViewModel
{
	public class ExplorerItemViewModel : ViewModelBase
	{
		private readonly ExplorerItem explorerItem;
		private readonly ExplorerItemViewModel parent;

		private ObservableCollection<ExplorerItemViewModel> children = null;
		private ExplorerItemState itemState = ExplorerItemState.Collapsed;
		private bool isSelected = false;
		private string navigateToFolder = null;

		private Action<ExplorerItemViewModel> OnSelectedCallback;

		public ExplorerItemViewModel(ExplorerItem argItem, Action<ExplorerItemViewModel> argSelectedCallback)
		{
			if (argItem == null)
			{
				throw new ArgumentNullException("argItem");
			}
			this.explorerItem = argItem;
			if (this.ExplorerItem.ItemType != ExplorerItemType.File)
			{
				this.SetDummyChildOnly();
			}
			this.OnSelectedCallback = argSelectedCallback;
		}

		public ExplorerItemViewModel(ExplorerItem argItem, Action<ExplorerItemViewModel> argSelectedCallback, ExplorerItemViewModel argParent)
			: this(argItem, argSelectedCallback)
		{
			this.parent = argParent;
		}

		private void SetDummyChildOnly()
		{
			this.Children = new ObservableCollection<ExplorerItemViewModel>(new List<ExplorerItemViewModel> { null });
		}

		private void BeginLoadChildItems()
		{
			if (this.ItemState == ExplorerItemState.Collapsed)
			{
				this.ItemState = ExplorerItemState.Expanding;

				ThreadPool.QueueUserWorkItem(new WaitCallback(o =>
				{
					this.LoadChildItems();
					this.ItemState = ExplorerItemState.Expanded;
				}));
			}
		}

		private void LoadChildItems()
		{
			try
			{
				IList<ExplorerItem> tmpChildren = new List<ExplorerItem>();
				foreach (string tmpPath in Directory.GetDirectories(this.ExplorerItem.Path))
				{
					ExplorerItem tmpItem = new ExplorerItem(ExplorerItemType.Folder);
					tmpItem.Name = tmpPath.Split(new char[] { '\\' }).Last();
					tmpItem.Path = tmpPath;

					tmpChildren.Add(tmpItem);
				}

				foreach (string tmpFilePath in Directory.GetFiles(this.ExplorerItem.Path))
				{
					ExplorerItem tmpItem = new ExplorerItem(ExplorerItemType.File);
					tmpItem.Name = tmpFilePath.Split(new char[] { '\\' }).Last();
					tmpItem.Path = tmpFilePath;

					tmpChildren.Add(tmpItem);
				}

				this.Children = new ObservableCollection<ExplorerItemViewModel>(tmpChildren.Select(item => new ExplorerItemViewModel(item, this.OnSelectedCallback, this)));
				this.OnFolderExpanded();
			}
			catch (Exception ex)
			{
				SimpleFileLogger.Instance.LogMessage(String.Format("Explorer item {0} was unable to load children", this.ExplorerItem.Path));
				SimpleFileLogger.Instance.LogError(ex);
			}
		}

		private void OnFolderExpanded()
		{
			if (this.NavigateToFolder != null && this.NavigateToFolder != this.ExplorerItem.Path)
			{
				string[] tmpPathLeftSections = this.NavigateToFolder.Remove(0, this.ExplorerItem.Path.Length).Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
				if (tmpPathLeftSections.Any())
				{
					string tmpChildFolder = tmpPathLeftSections[0];
					ExplorerItemViewModel tmpChildVM = this.Children.Where(item => item.ExplorerItem.Name == tmpChildFolder).FirstOrDefault();
					if (tmpChildVM != null)
					{
						tmpChildVM.NavigateTo(this.NavigateToFolder);
					}
				}
				this.NavigateToFolder = null;
			}
		}

		public void NavigateTo(string argFolder)
		{
			ThreadPool.QueueUserWorkItem(o =>
				{
					if (this.ExplorerItem.Path != argFolder)
					{
						this.NavigateToFolder = argFolder;
					}
					else
					{
						this.IsSelected = true;
						this.OnSelectedCallback(this);
					}

					if (!this.IsExpanded)
					{
						this.IsExpanded = true;
					}
					else
					{
						this.OnFolderExpanded();
					}
				});
		}

		public bool IsParentOf(ExplorerItemViewModel argItem)
		{
			ExplorerItemViewModel tmpParent = argItem.Parent;
			while (tmpParent != null && tmpParent != this)
			{
				tmpParent = tmpParent.Parent;
			}

			return tmpParent == this;
		}

		public ObservableCollection<ExplorerItemViewModel> Children
		{
			get
			{
				return this.children;
			}
			set
			{
				this.children = value;
				this.NotifyPropertyChanged("Children");
			}
		}

		public ExplorerItem ExplorerItem
		{
			get
			{
				return this.explorerItem;
			}
		}

		public ExplorerItemState ItemState
		{
			get
			{
				return this.itemState;
			}
			set
			{
				if (value != this.itemState)
				{
					this.itemState = value;
					this.NotifyPropertyChanged("ItemState");
				}
			}
		}

		public bool IsExpanded
		{
			get
			{
				return this.ItemState != ExplorerItemState.Collapsed;
			}
			set
			{
				if (value != this.IsExpanded)
				{
					if (value)
					{
						this.BeginLoadChildItems();
					}
					else
					{
						this.SetDummyChildOnly();
						this.ItemState = ExplorerItemState.Collapsed;
					}
					this.NotifyPropertyChanged("IsExpanded");
				}
			}
		}

		public bool IsSelected
		{
			get
			{
				return this.isSelected;
			}
			set
			{
				if (value != this.isSelected)
				{
					this.isSelected = value;
					this.NotifyPropertyChanged("IsSelected");
				}
			}
		}

		public bool IsFile
		{
			get
			{
				return this.ExplorerItem.ItemType == ExplorerItemType.File;
			}
		}

		public ExplorerItemViewModel Parent
		{
			get
			{
				return parent;
			}
		}

		private string NavigateToFolder
		{
			get
			{
				return this.navigateToFolder;
			}
			set
			{
				this.navigateToFolder = value;
			}
		}

		public enum ExplorerItemState
		{
			Collapsed,
			Expanding,
			Expanded
		}
	}
}