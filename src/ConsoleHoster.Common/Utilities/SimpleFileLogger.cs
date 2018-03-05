//-----------------------------------------------------------------------
// <copyright file="SimpleFileLogger.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.IO;
using System.Text;

namespace ConsoleHoster.Common.Utilities
{
	public sealed class SimpleFileLogger : IDisposable
	{
		private static readonly SimpleFileLogger instance = new SimpleFileLogger();
		private Object syncObject = new Object();

		private bool disposed = false;
		private bool isInitialized = false;
		private bool isLogEnabled = false;
		private string logDirectory = null;

		private StreamWriter logFile;

		public void Initialize(string directory)
		{
			if (this.LogEnabled && !this.IsInitialized)
			{
				lock (this.syncObject)
				{
					if (!this.IsInitialized)
					{
						if (!Directory.Exists(directory))
						{
							try
							{
								Directory.CreateDirectory(directory);
							}
							catch (Exception)
							{
								throw new Exception("Unable to create the directory specified");
							}
						}
						this.logDirectory = directory;
						string fileName = String.Format("Log_{0}.log", DateTime.UtcNow.ToString("yyyy-MM-dd_hh-mm"));
						string logPath = Path.Combine(this.logDirectory, fileName);
						try
						{
							this.logFile = new StreamWriter(logPath, true);
							this.LogFile.AutoFlush = true;
						}
						catch (Exception ex)
						{
							// unable to create a file
							throw new Exception("Unable to initialize the logger", ex);
						}

						this.IsInitialized = true;
					}
				}
			}
		}

		private SimpleFileLogger()
		{

		}

		~SimpleFileLogger()
		{
			this.Dispose(false);
		}

		public void LogError(Exception error)
		{
			this.LogError(String.Empty, error);
		}

		public void LogError(string errorMessage, Exception error)
		{
			StringBuilder messageBuilder = new StringBuilder("Exception type: " + error.ToString());
			Exception ex = error;
			while (ex != null)
			{
				messageBuilder.AppendFormat(" :: {0}", ex.Message);
				ex = ex.InnerException;
			}

			LogMessage(String.Format("{0}: {1} \n{2}", errorMessage, messageBuilder.ToString(), error.StackTrace));
		}

		public void LogMessage(string argMessage)
		{
			if (this.LogEnabled)
			{
				this.ValidateState();
				lock (this.syncObject)
				{
					DateTime now = DateTime.Now;
					this.LogFile.WriteLine("{0} {1}:\t{2}", now.ToLongDateString(), now.ToLongTimeString(), argMessage);
				}
			}
		}

		public void LogMessage(string argFormat, params string[] args)
		{
			this.LogMessage(String.Format(argFormat, args));
		}

		private void ValidateState()
		{
			if (!this.IsInitialized)
			{
				lock (this.syncObject)
				{
					if (!this.IsInitialized)
					{
						throw new Exception("The Instance must be initialized before usage");
					}
				}
			}
		}

		public void Dispose()
		{
			this.Dispose(true);

			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					lock (this.syncObject)
					{
						if (!this.disposed)
						{
							if (this.LogFile != null)
							{
								this.LogFile.Dispose();
								this.LogFile = null;
							}
						}
					}
				}

				this.IsInitialized = false;
				this.disposed = true;
			}
		}

		public static SimpleFileLogger Instance
		{
			get
			{
				return instance;
			}
		}

		private bool IsInitialized
		{
			get
			{
				return this.isInitialized;
			}
			set
			{
				this.isInitialized = value;
			}
		}

		public bool LogEnabled
		{
			get
			{
				return this.isLogEnabled;
			}
			set
			{
				this.isLogEnabled = value;
			}
		}

		private string LogDirectory
		{
			get
			{
				return this.logDirectory;
			}
			set
			{
				this.logDirectory = value;
			}
		}

		private StreamWriter LogFile
		{
			get
			{
				return this.logFile;
			}
			set
			{
				this.logFile = value;
			}
		}
	}
}