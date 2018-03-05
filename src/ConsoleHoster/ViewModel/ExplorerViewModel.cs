//-----------------------------------------------------------------------
// <copyright file="ExplorerViewModel.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
//-----------------------------------------------------------------------
using ConsoleHoster.Common.ViewModel;
using ConsoleHoster.Model.Entities;
using ConsoleHoster.ViewModel.Entities;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace ConsoleHoster.ViewModel
{
	public delegate void ExplorerItemEventHandler(ExplorerViewModel argSender, string argItemPath);

	public class ExplorerViewModel : ViewModelBase, IExplorerViewModel
	{
		public event ExplorerItemEventHandler ItemChosen;
		public event ExplorerItemEventHandler TryOpenExplorer;
		public event ExplorerItemEventHandler FolderChanged;
		public event ExplorerItemEventHandler RunFile;

		private readonly ObservableCollection<ExplorerItemViewModel> drives = new ObservableCollection<ExplorerItemViewModel>();
		private bool autoCollapseOldItems = false;
		private ExplorerItemViewModel selectedItem;

		public ExplorerViewModel(Dispatcher argDispatcher)
			: base(argDispatcher)
		{

		}

		protected override void InitializeRuntimeMode()
		{
			base.InitializeRuntimeMode();

			Task threadLoadTask = new Task(() =>
			{
				this.BusyStatus = "Loading fonts...";
				this.RunActionSwitchingToBusyState(() =>
				{
					this.RetrieveDrives();
				}, true);
			});

			threadLoadTask.Start();
		}

		private void RetrieveDrives()
		{
			foreach (DriveInfo tmpDriveInfo in DriveInfo.GetDrives())
			{
				ExplorerItem tmpItem = new ExplorerItem(ExplorerItemType.Drive);
				tmpItem.Name = tmpDriveInfo.Name;
				tmpItem.Alias = String.Format("{0} ({1})", tmpDriveInfo.IsReady ? tmpDriveInfo.VolumeLabel : String.Empty, tmpDriveInfo.Name);
				tmpItem.Path = tmpDriveInfo.Name;
				this.Drives.Add(new ExplorerItemViewModel(tmpItem, item => this.OnSelectedItemChanged(this.selectedItem, item), this.Dispatcher));
			}
		}

		public void OnItemChosen(ExplorerItem argItem)
		{
			ExplorerItemEventHandler temp = this.ItemChosen;
			if (temp != null)
			{
				temp(this, argItem.Path);
			}
		}

		public void OnOpenExplorer(ExplorerItem argItem)
		{
			ExplorerItemEventHandler temp = this.TryOpenExplorer;
			if (temp != null)
			{
				temp(this, argItem.Path);
			}
		}

		public void NavigateTo(string argFolder)
		{
			string[] tmpPathParts = argFolder.Split('\\');
			string tmpDriveToNavigateTo = tmpPathParts[0] + "\\";
			ExplorerItemViewModel tmpCurrentItem = this.drives.Where(item => String.Equals(item.ExplorerItem.Name, tmpDriveToNavigateTo, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
			tmpCurrentItem.NavigateTo(argFolder);
		}

		public void OnGoToItem(ExplorerItem argItem)
		{
			if (argItem.ItemType == ExplorerItemType.Folder)
			{
				this.OnFolderChanged(argItem.Path);
			}
		}

		public void RunItem(ExplorerItem argItem)
		{
			if (argItem.ItemType == ExplorerItemType.File)
			{
				this.OnRunItem(argItem.Path);
			}
		}

		private void OnRunItem(string argFile)
		{
			ExplorerItemEventHandler temp = this.RunFile;
			if (temp != null)
			{
				temp(this, argFile);
			}
		}

		private void OnFolderChanged(string argFolder)
		{
			ExplorerItemEventHandler temp = this.FolderChanged;
			if (temp != null)
			{
				temp(this, argFolder);
			}
		}

		private void OnSelectedItemChanged(ExplorerItemViewModel argOldItem, ExplorerItemViewModel argNewItem)
		{
			this.selectedItem = argNewItem;

			if (this.AutoCollapseOldItems && argNewItem != null)
			{
				if (argOldItem != null && !argOldItem.IsParentOf(argNewItem))
				{
					ExplorerItemViewModel tmpItemToCollapse = argOldItem;
					while (tmpItemToCollapse != null && !tmpItemToCollapse.IsParentOf(argNewItem))
					{
						tmpItemToCollapse.IsExpanded = false;
						tmpItemToCollapse = tmpItemToCollapse.Parent;
					}
				}
			}
		}

		public ICommand OpenExplorerCommand
		{
			get
			{
				return null;
			}
		}

		public ObservableCollection<ExplorerItemViewModel> Drives
		{
			get
			{
				return this.drives;
			}
		}

		public bool AutoCollapseOldItems
		{
			get
			{
				return this.autoCollapseOldItems;
			}
			set
			{
				this.autoCollapseOldItems = value;
			}
		}
	}
}