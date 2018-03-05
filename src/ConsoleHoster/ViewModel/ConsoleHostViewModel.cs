//-----------------------------------------------------------------------
// <copyright file="ConsoleHostViewModel.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
//-----------------------------------------------------------------------
using ConsoleHoster.Common.Plugins;
using ConsoleHoster.Common.Utilities;
using ConsoleHoster.Common.ViewModel;
using ConsoleHoster.Model;
using ConsoleHoster.Model.Entities;
using ConsoleHoster.Model.Plugins;
using ConsoleHoster.ViewModel.Enities;
using ConsoleHoster.ViewModel.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace ConsoleHoster.ViewModel
{
	public class ConsoleHostViewModel : ViewModelBase, IDisposable
	{
		private ObservableCollection<IConsoleVM> projects = new ObservableCollection<IConsoleVM>();
		private ICommand quickConsoleCommand;
		private ICommand loadProjectCommand;
		private ICommand customConsoleCommand;
		private ConsoleViewModel activeConsole = null;
		private ObservableCollection<ConsoleProjectViewModel> availableProjects = new ObservableCollection<ConsoleProjectViewModel>();
		private IList<CommandDataViewModel> globalCommands = new List<CommandDataViewModel>();
		private bool initialized = false;
		private readonly PluginManager pluginManager = new PluginManager();
		private readonly IList<IPlugin> plugins = new List<IPlugin>();

		public ConsoleHostViewModel()
			: base()
		{
			try
			{
				SimpleFileLogger.Instance.LogEnabled = true;
				SimpleFileLogger.Instance.Initialize(Directory.GetCurrentDirectory() + "\\Log\\");
			}
			catch
			{

			}
		}

		public void Dispose()
		{
			while (this.Projects.Count > 0)
			{
				this.Projects[0].Close();
			}
		}

		protected override void InitializeDesignMode()
		{
			base.InitializeDesignMode();

			ConsoleProjectViewModel tmpProject = new ConsoleProjectViewModel
			{
				Name = "Project 1"
			};
			this.AddProject(tmpProject);

			CommandDataViewModel tmpCommand = new CommandDataViewModel()
			{
				Name = "Level Up",
				CommandText = "cd..",
				IsFinal = true
			};
			this.AvailableCommands["Global commands"] = new List<CommandDataViewModel>();
			this.AvailableCommands["Global commands"].Add(tmpCommand);

			this.ErrorMessage = "Design mode error";
		}

		public void LoadStartupData()
		{
			if (!this.initialized && !this.IsInDesignMode)
			{
				this.initialized = true;
				this.IsBusy = true;
				SimpleFileLogger.Instance.LogMessage("Trying to load startup data");
				VersionUpgradeApplier.ApplyFullVersionUpgrade();
				Task tmpLoadingTask = Task.Factory.StartNew(() =>
					{
						this.LoadProjects();
						this.LoadCommands();
						this.LoadPlugins();
						SimpleFileLogger.Instance.LogMessage("Finished loading startup data");
						this.IsBusy = false;
					});
			}
		}

		protected override void NotifyPropertyChanged(string argPropertyName)
		{
			base.NotifyPropertyChanged(argPropertyName);

			if (argPropertyName != "ErrorMessage" && this.ErrorMessage != null)
			{
				this.ErrorMessage = null;
			}
		}

		private void LoadCommands()
		{
			try
			{
				IEnumerable<CommandData> tmpGlobalCommandModels = StorageManager.LoadGlobalCommands();
				this.GlobalCommands = tmpGlobalCommandModels == null ? null : tmpGlobalCommandModels.Select(item => new CommandDataViewModel(item)).ToList();
			}
			catch (Exception ex)
			{
				this.ErrorMessage = "Unable to load commands";
			}
		}

		private void LoadProjects()
		{
			this.RunActionWithErrorHandling(() =>
			{
				SimpleFileLogger.Instance.LogMessage("Loading projects");
				StorageManager.LoadGlobalSettings();

				IEnumerable<ConsoleProjectViewModel> tmpProjects = StorageManager.LoadProjects().Select(item => new ConsoleProjectViewModel(item));
				foreach (ConsoleProjectViewModel tmpProject in tmpProjects)
				{
					if (tmpProject.AutoLoad)
					{
						this.StartProject(tmpProject);
					}
					this.AddProject(tmpProject);
				}
				SimpleFileLogger.Instance.LogMessage("Successfully loaded the projects");
			},
			"Unable to load projects");
		}

		private void LoadPlugins()
		{
			this.RunActionWithErrorHandling(() =>
				{
					SimpleFileLogger.Instance.LogMessage("Loading plugins");
					this.PluginManager.Initalize();
					IEnumerable<PluginDetails> tmpAvailablePlugins = this.PluginManager.AvailablePlugins;
					foreach (PluginDetails tmpDetails in tmpAvailablePlugins)
					{
						if (tmpDetails.AutoLoad)
						{
							IPlugin tmpPlugin = this.PluginManager.LoadPlugin(tmpDetails.Name);
							this.Plugins.Add(tmpPlugin);
						}
					}
				});
		}

		private void AddProject(ConsoleProjectViewModel argProject)
		{
			ObservableCollection<ConsoleProjectViewModel> tmpProjects = new ObservableCollection<ConsoleProjectViewModel>(this.AvailableProjects);
			tmpProjects.Add(argProject);
			this.AvailableProjects = new ObservableCollection<ConsoleProjectViewModel>(tmpProjects.OrderBy(item => item.Name));
		}

		private void StartProject(ConsoleProjectViewModel argProject)
		{
			try
			{
				SimpleFileLogger.Instance.LogMessage("Starting project {0}", argProject.Name);
				ConsoleViewModel tmpVM = new ConsoleViewModel(this.Dispatcher, argProject);
				tmpVM.Closing += OnProject_Closing;

				this.RunActionOnUIThread(() =>
					{
						this.Projects.Add(tmpVM);
					});
				this.ActiveConsole = tmpVM;

				SimpleFileLogger.Instance.LogMessage("Successfullyl started project {0}", argProject.Name);
			}
			catch (Exception ex)
			{
				string tmpError = "Unable to stat the process.";
				if (ex is UnauthorizedAccessException)
				{
					tmpError += " You have no enough permissions for this operation.";
				}
				SimpleFileLogger.Instance.LogError(tmpError, ex);
				this.ErrorMessage = tmpError;
			}
		}

		private void OnProject_Closing(IConsoleVM argSender)
		{
			try
			{
				this.Projects.Remove(argSender);
				argSender.Closing -= OnProject_Closing;
				if (this.Projects.Any())
				{
					this.ActiveConsole = this.Projects.First() as ConsoleViewModel;
				}
				else
				{
					this.ActiveConsole = null;
				}
				argSender.Dispose();

				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
			}
			catch (Exception ex)
			{
				SimpleFileLogger.Instance.LogError("Exception during closing the project", ex);
			}
		}

		private void RunCommand(CommandDataViewModel argCommand)
		{
			if (this.ActiveConsole != null)
			{
				this.ActiveConsole.ExecuteCommand(argCommand);
			}
		}

		private void CreateQuickConsole()
		{
			this.StartProject(new ConsoleProjectViewModel
			{
				Name = "Quick Project",
				BackgroundColor = Colors.Black,
				MessageColor = Colors.White,
				CaretColor = Colors.White,
				ErrorMessageColor = Colors.Red,
				IsEditable = false
			});
		}

		private void OnActiveConsoleChanging()
		{
			if (this.ActiveConsole != null)
			{
				foreach (IPlugin tmpPlugin in this.Plugins)
				{
					tmpPlugin.Deactivate(this.ActiveConsole);
				}
				this.ActiveConsole.ShowRecentMessagesOnly = true;
			}
		}

		private void OnActiveConsoleChanged()
		{
			foreach (IPlugin tmpPlugin in this.Plugins)
			{
				tmpPlugin.Activate(this.ActiveConsole);
			}

			this.OnAvailableCommandsChanged();
		}

		internal void AddNewProject(ConsoleProjectViewModel argProject)
		{
			this.RunActionWithErrorHandling(() =>
				{
					if (argProject == null)
					{
						throw new ArgumentNullException("argProject");
					}

					SimpleFileLogger.Instance.LogMessage("Adding new project {0}", argProject.Name);
					StorageManager.SaveProject(argProject.Model);
					this.AddProject(argProject);
				});
		}

		internal void RemoveProject(string argProjectName)
		{
			this.RunActionWithErrorHandling(() =>
			{
				ConsoleProjectViewModel tmpCurrentProject = this.AvailableProjects.Where(item => item.Name.Equals(argProjectName, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
				if (tmpCurrentProject == null)
				{
					throw new ApplicationException("No such project");
				}
				this.AvailableProjects.Remove(tmpCurrentProject);
				StorageManager.DeleteProject(tmpCurrentProject.Name);
			});
		}

		internal void PersistProjectDetails(ConsoleProjectViewModel argProject)
		{
			this.RunActionWithErrorHandling(() =>
				{
					StorageManager.UpdateProject(argProject.Name, argProject.Model);
				});
		}

		internal void UpdateActiveConsoleProject(ConsoleProjectViewModel argProject)
		{
			this.RunActionWithErrorHandling(() =>
				{
					ConsoleProjectViewModel tmpOldProject = this.AvailableProjects.Where(item => item.Name == argProject.Name).Single();
					this.AvailableProjects[this.AvailableProjects.IndexOf(tmpOldProject)] = argProject;
					this.ActiveConsole.Project = argProject;
					this.OnAvailableCommandsChanged();
				});
		}

		internal void SetGlobalFont(string argFontFamily)
		{
			this.RunActionWithErrorHandling(() =>
				{
					GlobalSettings.Instance.SetFontFamily(argFontFamily);
					StorageManager.SaveGlobalSettings();
				});
		}

		internal void SwitchToNextConsole()
		{
			int tmpNextProjectIndex = this.Projects.IndexOf(this.ActiveConsole) + 1;
			if (tmpNextProjectIndex == this.Projects.Count)
			{
				tmpNextProjectIndex = 0;
			}
			this.ActiveConsole = this.Projects[tmpNextProjectIndex] as ConsoleViewModel;
		}

		private void OnAvailableCommandsChanged()
		{
			this.NotifyPropertyChanged("AvailableCommands");
		}

		#region Properties
		public ObservableCollection<IConsoleVM> Projects
		{
			get
			{
				return this.projects;
			}
			set
			{
				if (value != this.projects)
				{
					this.projects = value;
					this.NotifyPropertyChanged("Projects");
				}
			}
		}

		public ConsoleViewModel ActiveConsole
		{
			get
			{
				return this.activeConsole;
			}
			set
			{
				if (value != this.activeConsole)
				{
					this.OnActiveConsoleChanging();
					this.activeConsole = value;
					this.OnActiveConsoleChanged();
					this.NotifyPropertyChanged("ActiveConsole");
				}
			}
		}

		public ICommand QuickConsoleCommand
		{
			get
			{
				if (this.quickConsoleCommand == null)
				{
					this.quickConsoleCommand = new RelayCommand(param => this.CreateQuickConsole());
				}
				return this.quickConsoleCommand;
			}
		}

		public ICommand LoadProjectCommand
		{
			get
			{
				if (this.loadProjectCommand == null)
				{
					this.loadProjectCommand = new RelayCommand(param => this.StartProject(param as ConsoleProjectViewModel));
				}

				return this.loadProjectCommand;
			}
		}

		public ICommand CustomConsoleCommand
		{
			get
			{
				if (this.customConsoleCommand == null)
				{
					this.customConsoleCommand = new RelayCommand(tmpCommandData => this.RunCommand(tmpCommandData as CommandDataViewModel));
				}
				return this.customConsoleCommand;
			}
		}

		public ObservableCollection<ConsoleProjectViewModel> AvailableProjects
		{
			get
			{
				return this.availableProjects;
			}
			private set
			{
				if (value != this.availableProjects)
				{
					this.availableProjects = value;
					this.NotifyPropertyChanged("AvailableProjects");
				}
			}
		}

		private IList<CommandDataViewModel> GlobalCommands
		{
			get
			{
				return this.globalCommands;
			}
			set
			{
				if (value != this.globalCommands)
				{
					this.globalCommands = value;
					this.OnAvailableCommandsChanged();
				}
			}
		}

		public IDictionary<string, IList<CommandDataViewModel>> AvailableCommands
		{
			get
			{
				IEnumerable<CommandDataViewModel> tmpAllCommands = this.GlobalCommands;
				if (this.ActiveConsole != null && this.ActiveConsole.Project.Commands != null)
				{
					tmpAllCommands = tmpAllCommands.Concat(this.ActiveConsole.Project.Commands);
				}

				IDictionary<string, SortedList<string, CommandDataViewModel>> tmpResult = new Dictionary<string, SortedList<string, CommandDataViewModel>>();
				foreach (CommandDataViewModel tmpCommand in tmpAllCommands)
				{
					string tmpGroup = String.IsNullOrWhiteSpace(tmpCommand.GroupName) ? "Global Commands" : tmpCommand.GroupName;
					if (!tmpResult.ContainsKey(tmpGroup))
					{
						tmpResult[tmpGroup] = new SortedList<string, CommandDataViewModel>();
					}
					tmpResult[tmpGroup].Add(tmpCommand.Name, tmpCommand);
				}

				return tmpResult.OrderBy(item => item.Key).ToDictionary(item => item.Key, item => item.Value.Values);
			}
		}

		private PluginManager PluginManager
		{
			get
			{
				return this.pluginManager;
			}
		}

		public IList<IPlugin> Plugins
		{
			get
			{
				return this.plugins;
			}
		}
		#endregion
	}
}