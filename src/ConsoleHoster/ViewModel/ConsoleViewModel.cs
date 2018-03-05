//-----------------------------------------------------------------------
// <copyright file="ConsoleViewModel.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ConsoleHoster.Common.Utilities;
using ConsoleHoster.Common.ViewModel;
using ConsoleHoster.Model;
using ConsoleHoster.ViewModel.Enities;
using ConsoleHoster.ViewModel.Entities;

namespace ConsoleHoster.ViewModel
{
	public partial class ConsoleViewModel : ViewModelBase
	{
		public const string DEFAULT_COMMAND = "cmd.exe";
		private const int RECENT_MESSAGES_COUNT = 70;
		private const string MESSAGES_PROPERTY_NAME = "Messages";
		private const double DEFAULT_FONT_SIZE = 12;

		#region Fields
		private ObservableCollection<KeyValuePair<string, Color>> history = new ObservableCollection<KeyValuePair<string, Color>>();
		private ObservableQueue<KeyValuePair<string, Color>> recentHistory = new ObservableQueue<KeyValuePair<string, Color>>(RECENT_MESSAGES_COUNT);
		private string currentCommand;
		private ProcessWrapper underlyingProcess;
		private bool isAlive = false;
		private ConsoleProjectViewModel project = null;
		private int commandHistoryIndex = 0;
		private IList<string> commandHistory = new List<string>();
		private SearchViewModel searchVM = new SearchViewModel();
		private ExplorerViewModel explorerVM;

		private ICommand closeCommand = null;
		private ICommand projectCommand = null;
		private ICommand retrieveRecentCommand = null;
		private ICommand clearHistoryCommand = null;
		private ICommand breakExecutionCommand = null;
		private ICommand runTypedCommandCommand = null;
		private readonly Object historySyncObject = new Object();
		private bool showRecentMessagesOnly = false;
		private bool autoScroll = true;
		private bool autoSyncWithCurrentFolder;
		private string currentFolder = null;
		private bool showExplorerPane = true;
		private bool isReady = false;

		private int zoomLevel = 100;
		private bool suggestionProvided = false;

		private SuggestionContext pathSuggestionContext = null;
		#endregion

		public ConsoleViewModel(Dispatcher argDispatcher, ConsoleProjectViewModel argProject)
			: base(argDispatcher)
		{
			if (argProject == null)
			{
				throw new ArgumentNullException();
			}

			this.Project = argProject;
			if (!this.IsInDesignMode)
			{
				this.StartBackgroundConsole();
			}
		}

		private void StartBackgroundConsole()
		{
			SimpleFileLogger.Instance.LogMessage(String.Format("Console:\t{0} has started. Starting the background process", this.Project.Name));
			ProcessStartInfo tmpInfo = new ProcessStartInfo();
			if (this.Project != null)
			{
				tmpInfo.Arguments = this.Project.Arguments;
				tmpInfo.FileName = this.Project.Executable;
				tmpInfo.WorkingDirectory = this.Project.WorkingDir;
			}
			else
			{
				tmpInfo.FileName = DEFAULT_COMMAND;
			}

			if (String.IsNullOrWhiteSpace(tmpInfo.FileName))
			{
				tmpInfo.FileName = DEFAULT_COMMAND;
			}
			tmpInfo.RedirectStandardError = true;
			tmpInfo.RedirectStandardInput = true;
			tmpInfo.RedirectStandardOutput = true;
			tmpInfo.UseShellExecute = false;
			tmpInfo.CreateNoWindow = true;

			this.underlyingProcess = new ProcessWrapper(tmpInfo);

			try
			{
				this.underlyingProcess.DataReceived += OnProcessDataReceived;
				this.underlyingProcess.ProcessExited += OnCommandProcessExited;
				this.underlyingProcess.StartProcess();
				this.OnStarted();
				this.IsAlive = true;
			}
			catch (Exception ex)
			{
				SimpleFileLogger.Instance.LogMessage(String.Format("Console:\t{0} terminated. Unable to start the background process", this.Project.Name));
				SimpleFileLogger.Instance.LogError(ex);

				if (this.underlyingProcess != null)
				{
					this.underlyingProcess.Dispose();
				}

				this.IsAlive = false;
				this.ErrorMessage = "Unable to start the process: " + ex.Message;
			}
		}

		private void OnCommandProcessExited(ProcessWrapper sender)
		{
			this.underlyingProcess.Dispose();
			this.IsReady = true;
			this.IsAlive = false;
			SimpleFileLogger.Instance.LogMessage(String.Format("Console:\t{0} has exited", this.Project.Name));
		}

		private void OnProcessDataReceived(ProcessWrapper sender, ProcessMessage argMessage)
		{
			SimpleFileLogger.Instance.LogMessage(String.Format("Console:\t{0}\tMessage type: {1}\tMessage: {2}", this.Project.Name, argMessage.IsOutputMessage ? "Data" : "Error", argMessage.Data.Trim()));
			if (argMessage.IsOutputMessage)
			{
				this.AddMessageToHistory(argMessage.Data, this.Project.MessageColor);
				this.CheckFolderChanged(argMessage.Data);
			}
			else
			{
				this.AddMessageToHistory(argMessage.Data, this.Project.ErrorMessageColor);
			}
		}

		private void OnStarted()
		{
			SimpleFileLogger.Instance.LogMessage(String.Format("Console:\t{0} started successfully.", this.Project.Name));
		}

		protected override void InitializeRuntimeMode()
		{
			base.InitializeRuntimeMode();
			this.IsBusy = false;
			this.AutoSyncWithCurrentFolder = true;

			this.explorerVM = new ExplorerViewModel(this.Dispatcher);
			this.ExplorerVM.ItemChosen += OnExplorerVM_ItemChosen;
			this.ExplorerVM.TryOpenExplorer += OnExplorerVM_TryOpenExplorer;
			this.ExplorerVM.FolderChanged += OnExplorerVM_FolderChanged;
			this.ExplorerVM.RunFile += OnExplorerVM_RunFile;
		}

		private void OnExplorerVM_RunFile(ExplorerViewModel argSender, string argItemPath)
		{
			this.RunCommand(argItemPath);
		}

		private void OnExplorerVM_TryOpenExplorer(ExplorerViewModel argSender, string argItemPath)
		{
			try
			{
				Process.Start("explorer.exe", argItemPath);
			}
			catch
			{
				this.RunCommand(String.Format("start \"{0}\"", argItemPath));
			}
		}

		private void OnExplorerVM_ItemChosen(ExplorerViewModel argSender, string argItemPath)
		{
			this.CurrentCommand += argItemPath;
		}

		private void OnExplorerVM_FolderChanged(ExplorerViewModel argSender, string argItemPath)
		{
			char tmpOldDrive = this.CurrentFolder[0];
			char tmpNewDrive = argItemPath[0];

			this.RunCommand("cd " + argItemPath);

			if (tmpOldDrive != tmpNewDrive)
			{
				this.RunCommand(String.Format("{0}:", tmpNewDrive));
			}
		}

		private void OnCurrentCommandChanged(string argOldValue)
		{
			if (this.CurrentCommand != null)
			{
				if (this.CurrentCommand.Contains('\t'))
				{
					try
					{
						bool tmpFirstSuggestion;
						if (tmpFirstSuggestion = (this.PathSuggestionContext == null))
						{
							string tmpFolder;
							string tmpSuggestionBase;
							if (SuggestionRequested(out tmpFolder, out tmpSuggestionBase))
							{
								this.PathSuggestionContext = new SuggestionContext(tmpFolder, tmpSuggestionBase);
							}
						}

						if (this.PathSuggestionContext != null)
						{
							string tmpLastSuggestion = this.PathSuggestionContext.LastSuggestionBase;
							string tmpSuggestion = this.PathSuggestionContext.GetNextSuggestion();
							if (tmpSuggestion != null)
							{
								this.SuggestionProvided = true;
								string tmpTextToReplace = (tmpFirstSuggestion ? this.PathSuggestionContext.SuggestionBase : tmpLastSuggestion) + "\t";
								this.currentCommand = this.CurrentCommand.Replace(tmpTextToReplace, tmpSuggestion);
							}
						}
					}
					catch (Exception ex)
					{
						SimpleFileLogger.Instance.LogError(String.Format("Unable during providing a suggestion for: ||{0}||", this.CurrentCommand), ex);
					}
					this.RemoveTabsFromCurrentCommandSilently();
				}
				else if (this.PathSuggestionContext != null)
				{
					this.PathSuggestionContext = null;
				}
			}
		}

		private void RemoveTabsFromCurrentCommandSilently()
		{
			this.currentCommand = this.CurrentCommand.Replace("\t", String.Empty);
		}

		protected override void InitializeDesignMode()
		{
			base.InitializeDesignMode();

			this.IsBusy = true;
			this.CurrentCommand = "dir";
		}

		private void AddMessageToHistory(string argMessage, Color argColor)
		{
			KeyValuePair<string, Color> message = new KeyValuePair<string, Color>(argMessage, argColor);
			new Action(() =>
			{
				this.History.Add(message);
				this.RecentHistory.Add(message);
			}).SyncronizeCallByLocking(this.historySyncObject);
		}

		private void CheckFolderChanged(string argLastMessage)
		{
			string tmpPossiblePath = argLastMessage.Trim();

			if (Regex.IsMatch(argLastMessage, @"^[a-zA-Z]:(\\|(\\[\S ]+)*)>$"))
			{
				string tmpCurrentFolder = argLastMessage.Trim('>');
				if (tmpCurrentFolder != this.CurrentFolder)
				{
					this.CurrentFolder = tmpCurrentFolder;
					SimpleFileLogger.Instance.LogMessage(String.Format("Console:\t{0}: Current folder chagned to{1}", this.Project.Name, this.CurrentFolder));
					if (this.AutoSyncWithCurrentFolder || !this.IsReady)
					{
						SimpleFileLogger.Instance.LogMessage(String.Format("Console:\t{0}: Navigating to folder {1} in explorer", this.Project.Name, this.CurrentFolder));
						try
						{
							if (this.ShowExplorerPane)
							{
								this.ExplorerVM.NavigateTo(this.CurrentFolder);
							}
						}
						catch (Exception ex)
						{
							SimpleFileLogger.Instance.LogMessage(String.Format("Console:\t{0}: Explorer navigation to folder {1} failed", this.Project.Name, this.CurrentFolder));
							SimpleFileLogger.Instance.LogError(ex);
						}
					}
				}

				this.IsReady = true;
			}
			else
			{
				this.IsReady = false;
			}
		}

		public void ExecuteCommand(CommandDataViewModel argCommandData)
		{
			if (argCommandData == null)
			{
				this.ErrorMessage = "No command to execute";
				return;
			}

			if (argCommandData.IsFinal)
			{
				this.RunCommand(argCommandData.CommandText);
			}
			else
			{
				this.CurrentCommand = argCommandData.CommandText;
			}
		}

		private void LoadCommandFromHistory(bool argRetrieveNext)
		{
			if (argRetrieveNext)
			{
				this.CommandHistoryIndex++;
			}
			else
			{
				this.CommandHistoryIndex--;
			}

			if (this.CommandHistory.Any())
			{
				this.CurrentCommand = this.CommandHistory[this.CommandHistoryIndex];
			}
		}

		public void ClearHistory()
		{
			SimpleFileLogger.Instance.LogMessage("Trying to ClearHistory");
			try
			{
				new Action(() =>
					{
						this.History.Clear();
						this.RecentHistory.Clear();
						if (this.IsReady)
						{
							this.RunCommand(String.Empty);
						}
					}).SyncronizeCallByLocking(this.historySyncObject, 1000, true);
			}
			catch (ApplicationException aex)
			{
				SimpleFileLogger.Instance.LogError(aex);
			}
			catch (Exception ex)
			{
				SimpleFileLogger.Instance.LogError(ex);
			}
		}

		public void BreakExecution()
		{
			try
			{
				if (!this.underlyingProcess.TryBreak())
				{
					this.ErrorMessage = "Unable to break the process";
				}
			}
			catch (Exception ex)
			{
				SimpleFileLogger.Instance.LogError("Exception during breaking execution", ex);
			}
		}

		private bool SuggestionRequested(out string argFolder, out string argSuggestionBase)
		{
			argSuggestionBase = null;
			argFolder = null;
			double total = 0;
			for (int i = 0; i < 500; i++)
			{
				DateTime tmpStart = DateTime.Now;
				Match tmpRegexMatch = Regex.Match(this.CurrentCommand, "(\"" + @"(?:(?:(?:(^\w:|\s\w:)|\.\.?)(?:\/|\\))?(?:(?:[\w.]+(?: [\w.]+)?)(?:\/|\\))*)|(?:(?:(?:(^\w:|\s\w:)|\.\.?)(?:\/|\\))?(?:(?:[\w.]+(?:[\w.]+)?)(?:\/|\\))*))([\w\.]*)\t");
				if (!tmpRegexMatch.Success)
				{
					return false;
				}
				if (tmpRegexMatch.Groups[2].Success || tmpRegexMatch.Groups[3].Success)
				{
					//Drive letter specified
					argFolder = tmpRegexMatch.Groups[1].Value.Trim();
				}
				else
				{
					// Relative Path specified
					argFolder = Path.Combine(this.CurrentFolder, tmpRegexMatch.Groups[1].Value);
				}
				argSuggestionBase = tmpRegexMatch.Groups[4].Value;
				total += DateTime.Now.Subtract(tmpStart).TotalMilliseconds;
			}
			Console.WriteLine("Method Took {0}", total / 500.0);
			return true;
		}

		#region Properties
		public ObservableCollection<KeyValuePair<string, Color>> Messages
		{
			get
			{
				return this.ShowRecentMessagesOnly ? this.RecentHistory : this.History;
			}
		}

		public ObservableCollection<KeyValuePair<string, Color>> History
		{
			get
			{
				return this.history;
			}
			private set
			{
				if (value != this.history)
				{
					this.history = value;
					this.NotifyPropertyChanged("History");
					this.NotifyPropertyChanged(MESSAGES_PROPERTY_NAME);
				}
			}
		}

		public ObservableQueue<KeyValuePair<string, Color>> RecentHistory
		{
			get
			{
				return this.recentHistory;
			}
			set
			{
				if (value != this.recentHistory)
				{
					this.recentHistory = value;
					this.NotifyPropertyChanged("RecentHistory");
					this.NotifyPropertyChanged(MESSAGES_PROPERTY_NAME);
				}
			}
		}

		public bool ShowRecentMessagesOnly
		{
			get
			{
				return this.showRecentMessagesOnly;
			}
			set
			{
				if (value != this.showRecentMessagesOnly)
				{
					this.showRecentMessagesOnly = value;
					this.NotifyPropertyChanged("ShowRecentMessagesOnly");
					this.NotifyPropertyChanged(MESSAGES_PROPERTY_NAME);
				}
			}
		}

		public bool IsAlive
		{
			get
			{
				return this.isAlive;
			}
			set
			{
				if (value != this.isAlive)
				{
					this.isAlive = value;
					this.NotifyPropertyChanged("IsAlive");
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

		public ICommand CloseCommand
		{
			get
			{
				if (this.closeCommand == null)
				{
					this.closeCommand = new RelayCommand(o => this.Close());
				}
				return this.closeCommand;
			}
		}

		public ICommand ProjectCommand
		{
			get
			{
				if (this.projectCommand == null)
				{
					this.projectCommand = new RelayCommand(tmpCommand => this.ExecuteCommand(tmpCommand as CommandDataViewModel));
				}
				return this.projectCommand;
			}
		}

		public ICommand RetrieveRecentCommand
		{
			get
			{
				if (this.retrieveRecentCommand == null)
				{
					this.retrieveRecentCommand = new RelayCommand(retrieveNext => this.LoadCommandFromHistory((bool)retrieveNext));
				}
				return this.retrieveRecentCommand;
			}
		}

		public ICommand BreakExecutionCommand
		{
			get
			{
				if (this.breakExecutionCommand == null)
				{
					this.breakExecutionCommand = new RelayCommand(o => this.BreakExecution());
				}
				return this.breakExecutionCommand;
			}
		}

		public ICommand ClearHistoryCommand
		{
			get
			{
				if (this.clearHistoryCommand == null)
				{
					this.clearHistoryCommand = new RelayCommand(o => this.ClearHistory());
				}
				return this.clearHistoryCommand;
			}
		}

		public ICommand RunTypedCommandCommand
		{
			get
			{
				if (this.runTypedCommandCommand == null)
				{
					this.runTypedCommandCommand = new RelayCommand(o =>
						{
							this.RunCommand(this.CurrentCommand);
							this.CurrentCommand = String.Empty;
						});
				}
				return this.runTypedCommandCommand;
			}
		}

		private IList<string> CommandHistory
		{
			get
			{
				return this.commandHistory;
			}
		}

		private int CommandHistoryIndex
		{
			get
			{
				return this.commandHistoryIndex;
			}
			set
			{
				if (value < 0)
				{
					this.commandHistoryIndex = 0;
				}
				else if (value >= this.CommandHistory.Count)
				{
					this.commandHistoryIndex = this.CommandHistory.Count - 1;
				}
				else
				{
					this.commandHistoryIndex = value;
				}
			}
		}

		public SearchViewModel SearchVM
		{
			get
			{
				return this.searchVM;
			}
		}

		public ExplorerViewModel ExplorerVM
		{
			get
			{
				return explorerVM;
			}
		}

		public bool AutoScroll
		{
			get
			{
				return this.autoScroll;
			}
			set
			{
				if (value != this.autoScroll)
				{
					this.autoScroll = value;
					this.NotifyPropertyChanged("AutoScroll");
				}
			}
		}

		public bool AutoSyncWithCurrentFolder
		{
			get
			{
				return this.autoSyncWithCurrentFolder;
			}
			set
			{
				if (value != this.autoSyncWithCurrentFolder)
				{
					this.autoSyncWithCurrentFolder = value;
					if (this.ExplorerVM != null)
					{
						this.ExplorerVM.AutoCollapseOldItems = value;
					}
					this.NotifyPropertyChanged("AutoSyncWithCurrentFolder");
				}
			}
		}

		public int MinZoomLevel
		{
			get
			{
				return 10;
			}
		}

		public int MaxZoomLevel
		{
			get
			{
				return 190;
			}
		}

		public int ZoomLevel
		{
			get
			{
				return this.zoomLevel;
			}
			set
			{
				if (value != this.zoomLevel)
				{
					this.zoomLevel = Math.Min(Math.Max(this.MinZoomLevel, value), this.MaxZoomLevel);
					this.NotifyPropertyChanged("ZoomLevel");
					this.NotifyPropertyChanged("FontSize");
				}
			}
		}

		public double FontSize
		{
			get
			{
				return this.ZoomLevel * DEFAULT_FONT_SIZE / 100;
			}
		}

		public bool IsReady
		{
			get
			{
				return this.isReady;
			}
			set
			{
				if (value != this.isReady)
				{
					this.isReady = value;
					this.NotifyPropertyChanged("IsReady");
					this.NotifyPropertyChanged("WaitIndicatorVisibility");
				}
			}
		}

		public Visibility WaitIndicatorVisibility
		{
			get
			{
				return (!this.IsAlive || this.IsReady) ? Visibility.Collapsed : Visibility.Visible;
			}
		}

		public bool SuggestionProvided
		{
			get
			{
				return this.suggestionProvided;
			}
			internal set
			{
				this.suggestionProvided = value;
			}
		}

		public bool ShowExplorerPane
		{
			get
			{
				return this.showExplorerPane;
			}
			set
			{
				if (value != this.showExplorerPane)
				{
					this.showExplorerPane = value;
					if (this.showExplorerPane && this.AutoSyncWithCurrentFolder)
					{
						this.ExplorerVM.NavigateTo(this.CurrentFolder);
					}
					this.NotifyPropertyChanged("ShowExplorerPane");
				}
			}
		}

		public bool IsMultilineCommand
		{
			get
			{
				return this.CurrentCommand != null && this.CurrentCommand.Contains("\n");
			}
		}

		private SuggestionContext PathSuggestionContext
		{
			get
			{
				return this.pathSuggestionContext;
			}
			set
			{
				this.pathSuggestionContext = value;
			}
		}
		#endregion
	}
}