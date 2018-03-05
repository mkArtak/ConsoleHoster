//-----------------------------------------------------------------------
// <copyright file="ExplorerItemViewModel.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using ConsoleHoster.Common.Utilities;
using ConsoleHoster.Common.ViewModel;
using ConsoleHoster.Model.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ConsoleHoster.ViewModel.Entities
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
		private Task itemsLoadingTask;
		private volatile bool stopLoading = false;

		public ExplorerItemViewModel(ExplorerItem argItem, Action<ExplorerItemViewModel> argSelectedCallback, Dispatcher argDispatcher)
			: base(argDispatcher)
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

		public ExplorerItemViewModel(ExplorerItem argItem, Action<ExplorerItemViewModel> argSelectedCallback, ExplorerItemViewModel argParent, Dispatcher argDispatcher)
			: this(argItem, argSelectedCallback, argDispatcher)
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
				this.RunActionSwitchingToBusyState(() =>
					{
						this.ItemState = ExplorerItemState.Expanding;

						this.StopBackgroundLoadIfAny();

						Console.WriteLine("Starting async loading items");
						this.Children = new ObservableCollection<ExplorerItemViewModel>();
						this.itemsLoadingTask = new Task(() =>
							{
								this.LoadChildItems();
							});
						this.itemsLoadingTask.Start();
					}, true);
			}
		}

		private void StopBackgroundLoadIfAny()
		{
			if (this.itemsLoadingTask != null)
			{
				this.stopLoading = true;
				this.itemsLoadingTask.Wait();
				this.stopLoading = false;
			}
		}

		private void LoadChildItems()
		{
			try
			{
				foreach (string tmpPath in Directory.GetDirectories(this.ExplorerItem.Path))
				{
					if (this.stopLoading)
					{
						return;
					}
					ExplorerItem tmpItem = new ExplorerItem(ExplorerItemType.Folder);
					tmpItem.Name = tmpPath.Split(new char[] { '\\' }).Last();
					tmpItem.Path = tmpPath;

					this.AddLoadedChildItem(tmpItem);
				}

				foreach (string tmpFilePath in Directory.GetFiles(this.ExplorerItem.Path))
				{
					if (this.stopLoading)
					{
						return;
					}

					ExplorerItem tmpItem = new ExplorerItem(ExplorerItemType.File);
					tmpItem.Name = tmpFilePath.Split(new char[] { '\\' }).Last();
					tmpItem.Path = tmpFilePath;

					this.AddLoadedChildItem(tmpItem);
				}
				this.ItemState = ExplorerItemState.Expanded;

				this.OnFolderExpanded();
			}
			catch (Exception ex)
			{
				SimpleFileLogger.Instance.LogMessage(String.Format("Explorer item {0} was unable to load children", this.ExplorerItem.Path));
				SimpleFileLogger.Instance.LogError(ex);
			}
		}

		private void AddLoadedChildItem(ExplorerItem argItem)
		{
			ExplorerItemViewModel tmpVM = new ExplorerItemViewModel(argItem, this.OnSelectedCallback, this, this.Dispatcher);
			this.RunActionOnUIThread(() =>
				{
					this.Children.Add(tmpVM);
				});

			// Sleeping here to ensure that there is some time between next call to be transferred to the UI thread, so the UI thread to stay responsive
			Thread.Sleep(5);
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
						this.StopBackgroundLoadIfAny();
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