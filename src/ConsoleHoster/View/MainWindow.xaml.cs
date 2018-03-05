//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using ConsoleHoster.Common.Utilities;
using ConsoleHoster.Model.NativeWrappers;
using ConsoleHoster.View.Popups;
using ConsoleHoster.ViewModel;
using ConsoleHoster.ViewModel.Enities;
using ConsoleHoster.ViewModel.Entities;

namespace ConsoleHoster.View
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private AboutView aboutView = null;
		private FontsView fontsView = null;
		private ProjectView projectView = null;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void ImportProject_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog tmpDialog = new OpenFileDialog();
			tmpDialog.Multiselect = false;
			tmpDialog.Filter = "Shortcuts|*.lnk";
			DialogResult tmpDialogResult = tmpDialog.ShowDialog();
			if (tmpDialogResult == System.Windows.Forms.DialogResult.OK)
			{
				string tmpLink = tmpDialog.FileName;
				try
				{
					ConsoleProjectViewModel tmpProject;
					using (WindowsShortcut tmpShellLink = new WindowsShortcut(tmpLink))
					{
						tmpProject = new ConsoleProjectViewModel();
						tmpProject.Arguments = tmpShellLink.Arguments;
						tmpProject.Executable = tmpShellLink.Target;
						tmpProject.Name = System.IO.Path.GetFileNameWithoutExtension(tmpShellLink.ShortCutFile);
						tmpProject.WorkingDir = tmpShellLink.WorkingDirectory;
					}

					this.CreateNewProject(tmpProject);
				}
				catch (Exception ex)
				{
					SimpleFileLogger.Instance.LogError("Unable to import the project", ex);
					System.Windows.MessageBox.Show("Unable to import the project. You can find more details about this in the application log.");
				}
			}
		}

		private void NewProject_Click(object sender, RoutedEventArgs e)
		{
			this.CreateNewProject(new ConsoleProjectViewModel
			{
				Name = "New Project",
				Commands = new ObservableCollection<CommandDataViewModel>()
			});
		}

		private void EditProject_Click(object sender, RoutedEventArgs e)
		{
			this.EditProject(this.ViewModel.ActiveConsole.Project);
		}

		private void MenuHelpAbout_Click(object sender, RoutedEventArgs e)
		{
			this.aboutView = new AboutView();
			this.aboutView.ShowDialog();
			this.aboutView = null;
		}

		private void MenuUsersGuide_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				string tmpUserGuidFile = "./Resources/User Guide.docx";
				if (File.Exists(tmpUserGuidFile))
				{
					Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tmpUserGuidFile));
				}
				else
				{
					Process.Start("http://consolehoster.codeplex.com/");
				}
			}
			catch (Exception ex)
			{
				SimpleFileLogger.Instance.LogError(ex);
			}
		}

		private void DeleteProject_Click(object sender, RoutedEventArgs e)
		{
			MessageBoxResult tmpUserChoice = System.Windows.MessageBox.Show("Are you sure you want to delete current project ?", "Delete Project", MessageBoxButton.YesNo, MessageBoxImage.Warning);
			if (tmpUserChoice == MessageBoxResult.Yes)
			{
				this.ViewModel.RemoveProject(this.ViewModel.ActiveConsole.Name);
			}
		}

		private void FontMenu_Click(object sender, RoutedEventArgs e)
		{
			this.fontsView = new FontsView();
			bool? tmpResult = this.fontsView.ShowDialog();
			if (tmpResult.HasValue && tmpResult.Value)
			{
				this.ViewModel.SetGlobalFont(this.fontsView.ViewModel.SelectedFont);
			}
			this.fontsView = null;
		}

		private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this.ViewModel.Dispose();
		}

		private void Window_Activated_1(object sender, EventArgs e)
		{
			this.ActivateWindowIfNotNull(this.fontsView);
			this.ActivateWindowIfNotNull(this.projectView);
			this.ActivateWindowIfNotNull(this.aboutView);
		}

		private void MainWindow_PreviewKeyDown_1(object sender, System.Windows.Input.KeyEventArgs e)
		{
			switch (e.Key)
			{
				case System.Windows.Input.Key.Tab:
					if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
					{
						if (this.ViewModel.ActiveConsole != null)
						{
							this.ViewModel.SwitchToNextConsole();
						}
						e.Handled = true;
					}
					break;
			}
		}

		private void DuplicateProject_Click(object sender, RoutedEventArgs e)
		{
			ConsoleProjectViewModel tmpProjectVM = this.ViewModel.ActiveConsole.Project.GetCopy();
			SimpleFileLogger.Instance.LogMessage("Duplicating project {0}", tmpProjectVM.Name);
			tmpProjectVM.Name = String.Format("Duplicate {0}", tmpProjectVM.Name);
			this.CreateNewProject(tmpProjectVM);
		}

		private void MainWindow_Loaded_1(object sender, RoutedEventArgs e)
		{
			this.ViewModel.InitializeDispatcher(this.Dispatcher);
			this.ViewModel.LoadStartupData();
		}

		private void ActivateWindowIfNotNull(Window argWindow)
		{
			if (argWindow != null)
			{
				argWindow.Activate();
			}
		}

		private void CreateNewProject(ConsoleProjectViewModel argProject)
		{
			ProjectDetailsViewModel tmpVM = new ProjectDetailsViewModel(this.ViewModel, argProject, true);
			this.projectView = new ProjectView(tmpVM);
			bool? tmpResult = this.projectView.ShowDialog();
			if (tmpResult.HasValue && tmpResult.Value)
			{
				this.ViewModel.AddNewProject(this.projectView.ViewModel.Project);
			}
			this.projectView = null;
		}

		private void EditProject(ConsoleProjectViewModel argProject)
		{
			this.projectView = new ProjectView(new ProjectDetailsViewModel(this.ViewModel, argProject.GetCopy(), false));
			bool? tmpResult = this.projectView.ShowDialog();
			if (tmpResult.HasValue && tmpResult.Value)
			{
				this.ViewModel.PersistProjectDetails(this.projectView.ViewModel.Project);
				this.ViewModel.UpdateActiveConsoleProject(this.projectView.ViewModel.Project);
			}
		}

		private ConsoleHostViewModel ViewModel
		{
			get
			{
				return this.DataContext as ConsoleHostViewModel;
			}
		}
	}
}