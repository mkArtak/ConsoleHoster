//-----------------------------------------------------------------------
// <copyright file="DriveExplorerView.xaml.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
//-----------------------------------------------------------------------
using ConsoleHoster.Model.Entities;
using ConsoleHoster.ViewModel;
using ConsoleHoster.ViewModel.Entities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ConsoleHoster.View.Controls
{
	/// <summary>
	/// Interaction logic for DriveExplorerView.xaml
	/// </summary>
	public partial class DriveExplorerView : UserControl
	{
		public DriveExplorerView()
		{
			InitializeComponent();
		}

		private void OnExplorerItemText_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 1)
			{
				ExplorerItem tmpItem = GetViewModelForItem(sender as TextBlock);
				if (Keyboard.IsKeyDown(Key.LeftCtrl))
				{
					this.ViewModel.OnItemChosen(tmpItem);
					e.Handled = true;
					return;
				}
				else if (Keyboard.IsKeyDown(Key.LeftShift))
				{
					this.ViewModel.OnGoToItem(tmpItem);
					e.Handled = true;
					return;
				}
			}
		}

		private void OnExplorerItemText_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl))
			{
				ExplorerItem tmpItem = GetViewModelForItem(sender as TextBlock);
				if (tmpItem != null)
				{
					this.ViewModel.OnOpenExplorer(tmpItem);
				}
			}
		}

		private void OnExplorerItemText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				ExplorerItem tmpItem = GetViewModelForItem(sender as TextBlock);
				if (tmpItem != null && tmpItem.ItemType == ExplorerItemType.File)
				{
					this.ViewModel.RunItem(tmpItem);
					e.Handled = true;
				}
			}
		}

		private void OnContextMenu_OpenExplorer(object sender, RoutedEventArgs e)
		{
			ExplorerItemViewModel tmpItem = (sender as MenuItem).DataContext as ExplorerItemViewModel;
			if (tmpItem != null)
			{
				this.ViewModel.OnOpenExplorer(tmpItem.ExplorerItem);
				e.Handled = true;
			}
		}

		private void OnContextMenu_UsePath(object sender, RoutedEventArgs e)
		{
			ExplorerItemViewModel tmpItem = (sender as MenuItem).DataContext as ExplorerItemViewModel;
			if (tmpItem != null)
			{
				this.ViewModel.OnItemChosen(tmpItem.ExplorerItem);
				e.Handled = true;
			}
		}

		private void OnContextMenu_RedirectTo(object sender, RoutedEventArgs e)
		{
			ExplorerItemViewModel tmpItem = (sender as MenuItem).DataContext as ExplorerItemViewModel;
			if (tmpItem != null)
			{
				this.ViewModel.OnGoToItem(tmpItem.ExplorerItem);
				e.Handled = true;
			}
		}

		private void OnContextMenu_Run(object sender, RoutedEventArgs e)
		{
			ExplorerItemViewModel tmpItem = (sender as MenuItem).DataContext as ExplorerItemViewModel;
			if (tmpItem != null)
			{
				this.ViewModel.RunItem(tmpItem.ExplorerItem);
				e.Handled = true;
			}
		}

		private static ExplorerItem GetViewModelForItem(TextBlock sender)
		{
			ExplorerItemViewModel tmpVM = sender.DataContext as ExplorerItemViewModel;
			return tmpVM == null ? null : tmpVM.ExplorerItem;
		}

		private IExplorerViewModel ViewModel
		{
			get
			{
				return this.DataContext as IExplorerViewModel;
			}
		}

		public string SelectedImagePath
		{
			get;
			set;
		}
	}
}