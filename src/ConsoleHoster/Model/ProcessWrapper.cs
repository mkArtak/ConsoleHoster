//-----------------------------------------------------------------------
// <copyright file="ProcessWrapper.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
//-----------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using ConsoleHoster.Common.Utilities;
using ConsoleHoster.Model.NativeWrappers;

namespace ConsoleHoster.Model
{
	public delegate void ProcessWrapperEventHandler(ProcessWrapper argSender);
	public delegate void ProcessWrapperMessageEventHandler(ProcessWrapper argSender, ProcessMessage argMessage);

	public class ProcessWrapper : IDisposable
	{
		public event ProcessWrapperEventHandler ProcessExited;
		public event ProcessWrapperMessageEventHandler DataReceived;

		private Process process;
		private readonly ProcessStartInfo startInfo;

		private bool isDisposed = false;
		private readonly Object disposingSyncer = new Object();

		private StreamObserver outputStreamObserver;
		private StreamObserver errorStreamObserver;

		public ProcessWrapper(ProcessStartInfo argInfo)
		{
			if (argInfo == null)
			{
				throw new ArgumentNullException("info");
			}

			this.startInfo = new ProcessStartInfo();
			this.startInfo.RedirectStandardError = true;
			this.startInfo.RedirectStandardInput = true;
			this.startInfo.RedirectStandardOutput = true;
			this.startInfo.UseShellExecute = false;
			this.startInfo.CreateNoWindow = true;
			this.startInfo.Arguments = argInfo.Arguments;
			this.startInfo.FileName = argInfo.FileName;
			this.startInfo.WorkingDirectory = argInfo.WorkingDirectory;
		}

		~ProcessWrapper()
		{
			this.Dispose(false);
		}

		public void StartProcess()
		{
			if (this.process != null)
			{
				throw new Exception("Process already started");
			}

			try
			{
				this.process = this.CreateUnderlyingProcess(true);
			}
			catch (Exception ex)
			{
				SimpleFileLogger.Instance.LogError("Unable to start the underying process", ex);
				if (this.process != null)
				{
					this.process.Exited -= OnProcess_Exited;
					this.process.Dispose();
					this.process = null;
				}

				throw new Exception("Unable to start the process");
			}

			if (this.process != null)
			{
				this.process.PriorityClass = ProcessPriorityClass.BelowNormal;
				Object streamSyncer = new Object();
				this.outputStreamObserver = this.BeginReadStream(this.process.StandardOutput, streamSyncer, "Unable to read output stream");
				this.errorStreamObserver = this.BeginReadStream(this.process.StandardError, streamSyncer, "Unable to read error stream");
			}
		}

		private Process CreateUnderlyingProcess(bool argSupportProcessGroups)
		{
			Process tmpProcess;
			if (false)
			{
				//ConsoleHoster.Model.NativeWrappers.WindowsApi.STARTUPINFO tmpInfo = new NativeWrappers.WindowsApi.STARTUPINFO();

				//ConsoleHoster.Model.NativeWrappers.WindowsApi.CreateProcess(this.startInfo.FileName, this.startInfo.Arguments, null, null, false, CREATE_NEW_PROCESS_GROUP | STARTF_USESTDHANDLES, null, this.startInfo.WorkingDirectory, 
				//tmpProcess = new WindowsApi.ProcessHelper().StartProcessWithFlag(this.startInfo, CREATE_NEW_PROCESS_GROUP | STARTF_USESTDHANDLES);
			}
			else
			{
				tmpProcess = new Process();
				tmpProcess.StartInfo = this.startInfo;
				tmpProcess.Exited += OnProcess_Exited;
				tmpProcess.EnableRaisingEvents = true;

				tmpProcess.Start();
			}
			return tmpProcess;
		}

		private StreamObserver BeginReadStream(StreamReader argStream, Object messageSyncer, string argErrorMessage)
		{
			StreamObserver tmpResult;
			try
			{
				tmpResult = new StreamObserver(argStream, messageSyncer);
				tmpResult.MessageReceived += OnStreamObserver_MessageReceived;
				tmpResult.BeginObservation();
			}
			catch (Exception ex)
			{
				SimpleFileLogger.Instance.LogError(ex);
				throw new Exception(argErrorMessage, ex);
			}

			return tmpResult;
		}

		private void OnStreamObserver_MessageReceived(StreamObserver argSender, string argData)
		{
			this.OnMessageReceived(new ProcessMessage(argData, DateTime.UtcNow, argSender == this.outputStreamObserver));
		}

		private void OnMessageReceived(ProcessMessage argMessage)
		{
			ProcessWrapperMessageEventHandler temp = this.DataReceived;
			if (temp != null)
			{
				temp(this, argMessage);
			}
		}

		private void OnProcess_Exited(object sender, EventArgs e)
		{
			ProcessWrapperEventHandler temp = this.ProcessExited;
			if (temp != null)
			{
				temp(this);
			}
		}

		public void SendData(string argCommand)
		{
			this.process.StandardInput.WriteLine(argCommand);
		}

		public void Dispose()
		{
			this.Dispose(true);

			GC.SuppressFinalize(this);
			GC.Collect();
		}

		private void Dispose(bool argDisposing)
		{
			if (!this.isDisposed)
			{
				new Action(() =>
				{
					if (!this.isDisposed)
					{
						if (argDisposing)
						{
							if (this.outputStreamObserver != null)
							{
								this.outputStreamObserver.MessageReceived -= OnStreamObserver_MessageReceived;
								this.outputStreamObserver.Dispose();
							}

							if (this.errorStreamObserver != null)
							{
								this.errorStreamObserver.MessageReceived -= OnStreamObserver_MessageReceived;
								this.errorStreamObserver.Dispose();
							}

							if (this.process != null)
							{
								this.process.Exited -= OnProcess_Exited;
								try
								{
									this.process.Kill();
									this.process.WaitForExit();
								}
								catch
								{
								}
								finally
								{
									if (this.process != null)
									{
										this.process.Close();
										this.process.Dispose();
										this.process = null;
									}
								}
							}
						}

						this.isDisposed = true;
					}
				}).SyncronizeCallByLocking(this.disposingSyncer);
			}
		}

		public bool TryBreak()
		{
			bool tmpResult = false;
			try
			{
				tmpResult = ConsoleApi.GenerateConsoleCtrlEvent(0, (uint)this.process.Handle.ToInt32());

				if (!tmpResult)
				{
					int tmpError = Marshal.GetLastWin32Error();
				}
			}
			catch (Exception ex)
			{
				SimpleFileLogger.Instance.LogError("Unable to break the process", ex);
			}

			return tmpResult;
		}
	}

	public enum ProcessMessageTypes
	{
		Output,
		Error
	}
}