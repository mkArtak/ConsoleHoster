//-----------------------------------------------------------------------
// <copyright file="ConsoleViewModel.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>12/08/2012</date>
//-----------------------------------------------------------------------
using System;
using System.Linq;
using ConsoleHoster.Common.Utilities;
using ConsoleHoster.Common.ViewModel;

namespace ConsoleHoster.ViewModel
{
	/// <summary>
	/// ConsoleViewModel class's IConsoleVM implementation
	/// </summary>
	public partial class ConsoleViewModel : IConsoleVM
	{
		public event ConsoleViewModelEventHandler Closing;

		#region Public methods
		public void RunCommand(string argCommand)
		{
			if (argCommand != null)
			{
				try
				{
					SimpleFileLogger.Instance.LogMessage(String.Format("Running command: {0}", argCommand));
					this.underlyingProcess.SendData(argCommand);

					if (!String.IsNullOrWhiteSpace(argCommand) && (!this.CommandHistory.Any() || !String.Equals(this.CommandHistory.Last(), argCommand, StringComparison.InvariantCultureIgnoreCase)))
					{
						this.CommandHistory.Add(argCommand);
						this.commandHistoryIndex = this.CommandHistory.Count;
					}
				}
				catch (Exception ex)
				{
					string tmpErrorMessage = "Unable to run command: " + argCommand;
					SimpleFileLogger.Instance.LogError(tmpErrorMessage, ex);
					this.ErrorMessage = tmpErrorMessage;
				}
			}
		}

		public void Close()
		{
			this.OnClosing();
		}

		public void Dispose()
		{
			if (this.underlyingProcess != null)
			{
				this.underlyingProcess.Dispose();
			}
		}
		#endregion

		private void OnClosing()
		{
			ConsoleViewModelEventHandler temp = this.Closing;
			if (temp != null)
			{
				temp(this);
			}
		}

		#region Properties
		public string Name
		{
			get
			{
				return this.Project.Name;
			}
		}

		public string CurrentFolder
		{
			get
			{
				return this.currentFolder;
			}
			private set
			{
				if (value != this.currentFolder)
				{
					this.currentFolder = value;
					this.NotifyPropertyChanged("CurrentFolder");
				}
			}
		}

		public string CurrentCommand
		{
			get
			{
				return this.currentCommand;
			}
			set
			{
				if (value != this.currentCommand)
				{
					string tmpOldValue = this.currentCommand;
					this.currentCommand = value;
					this.OnCurrentCommandChanged(tmpOldValue);
					this.NotifyPropertyChanged("CurrentCommand");
				}
			}
		}
		#endregion
	}
}
