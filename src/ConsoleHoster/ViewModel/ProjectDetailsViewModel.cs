//-----------------------------------------------------------------------
// <copyright file="ProjectDetailsViewModel.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using ConsoleHoster.Common.Utilities;
using ConsoleHoster.Common.ViewModel;
using ConsoleHoster.ViewModel.Enities;
using ConsoleHoster.ViewModel.Entities;

namespace ConsoleHoster.ViewModel
{
	public class ProjectDetailsViewModel : ViewModelBase
	{
		private readonly ConsoleHostViewModel hostVM;
		private ConsoleProjectViewModel project;
		private bool isNewProject = true;
		private CommandDataViewModel selectedCommand;
		private ICommand newCommandDataCommand;
		private ICommand removeCommandDataCommand;

		public ProjectDetailsViewModel(ConsoleHostViewModel argHostVM)
			: this(argHostVM, new ConsoleProjectViewModel(), true)
		{
		}

		public ProjectDetailsViewModel(ConsoleHostViewModel argHostVM, ConsoleProjectViewModel argProject, bool argIsNew)
		{
			if (argHostVM == null)
			{
				throw new ArgumentNullException("argHostVM", "Host VM cannot be null");
			}

			this.hostVM = argHostVM;
			this.IsNewProject = argIsNew;
			this.Project = argProject;
		}

		private bool ValidateProject(bool argValidateName)
		{
			bool tmpResult = true;

			if (argValidateName && this.HostVM.AvailableProjects.Any(item => item != this.Project && item.Name.Equals(this.Project.Name, StringComparison.InvariantCultureIgnoreCase)))
			{
				this.ErrorMessage = "A project with the same name already exists";
				tmpResult = false;
			}

			if (tmpResult && this.Project.Commands != null && this.Project.Commands.GroupBy(item => item.Name, StringComparer.InvariantCultureIgnoreCase).Any(item => item.Count() > 1))
			{
				this.ErrorMessage = "Please ensure that the command names are unique per group";
				tmpResult = false;
			}

			return tmpResult;
		}

		#region Properties
		public ConsoleHostViewModel HostVM
		{
			get
			{
				return this.hostVM;
			}
		}

		public bool IsNewProject
		{
			get
			{
				return this.isNewProject;
			}
			set
			{
				if (value != this.isNewProject)
				{
					this.isNewProject = value;
					this.NotifyPropertyChanged("IsNewProject");
				}
			}
		}

		public ConsoleProjectViewModel Project
		{
			get
			{
				return this.project;
			}
			set
			{
				if (value != this.project)
				{
					this.project = value;
					this.NotifyPropertyChanged("Project");
				}
			}
		}

		public string ProjectColor
		{
			get
			{
				return ColorUtilities.GetColorNameOrCode(this.Project.ProjectColor);
			}
			set
			{
				this.Project.ProjectColor = ColorUtilities.GetColorFromString(value, this.Project.ProjectColor);
			}
		}

		public string BackgroundColor
		{
			get
			{
				return this.Project.BackgroundColor.ToString();
			}
			set
			{
				this.Project.BackgroundColor = ColorUtilities.GetColorFromString(value, Colors.Black);
			}
		}

		public string MessageColor
		{
			get
			{
				return this.Project.MessageColor.ToString();
			}
			set
			{
				this.Project.MessageColor = ColorUtilities.GetColorFromString(value, Colors.White);
			}
		}

		public string ErrorMessageColor
		{
			get
			{
				return this.Project.ErrorMessageColor.ToString();
			}
			set
			{
				this.Project.ErrorMessageColor = ColorUtilities.GetColorFromString(value, Colors.Red);
			}
		}

		public ICommand NewCommandDataCommand
		{
			get
			{
				if (this.newCommandDataCommand == null)
				{
					this.newCommandDataCommand = new RelayCommand(p =>
						{
							this.RunActionWithErrorHandling(() =>
								{
									CommandDataViewModel tmpNewCommand = new CommandDataViewModel
									{
										Name = "New Command"
									};
									if (this.Project.Commands == null)
									{
										this.Project.Commands = new ObservableCollection<CommandDataViewModel>();
									}
									this.Project.Commands.Add(tmpNewCommand);
									this.SelectedCommand = tmpNewCommand;
								});
						});
				}
				return this.newCommandDataCommand;
			}
		}

		public ICommand RemoveCommandDataCommand
		{
			get
			{
				if (this.removeCommandDataCommand == null)
				{
					this.removeCommandDataCommand = new RelayCommand(p =>
					{
						if (this.SelectedCommand != null)
						{
							this.Project.Commands.Remove(this.SelectedCommand);
							this.SelectedCommand = null;
						}
					});
				}
				return this.removeCommandDataCommand;
			}
		}

		public CommandDataViewModel SelectedCommand
		{
			get
			{
				return this.selectedCommand;
			}
			set
			{
				if (value != this.selectedCommand)
				{
					this.selectedCommand = value;
					this.NotifyPropertyChanged("SelectedCommand");
				}
			}
		}

		public bool IsValid
		{
			get
			{
				return this.ValidateProject(this.IsNewProject);
			}
		}
		#endregion
	}
}