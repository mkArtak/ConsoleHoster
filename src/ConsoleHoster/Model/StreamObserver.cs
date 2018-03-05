//-----------------------------------------------------------------------
// <copyright file="StreamObserver.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.IO;
using System.Text;
using System.Threading;
using ConsoleHoster.Common.Utilities;
using ConsoleHoster.Common.Utilities.Threading;

namespace ConsoleHoster.Model
{
	public delegate void StreamObserverEventHandler(StreamObserver argSender, string argData);

	public class StreamObserver : IDisposable
	{
		public event StreamObserverEventHandler MessageReceived;

		private readonly StreamReader stream;
		private readonly StringBuilder messageBuffer = new StringBuilder();
		private readonly SharedLock messageSyncer;

		private Thread streamReadingThread;

		private bool isDisposed = false;
		private readonly Object disposalLock = new Object();
		private string lastMessage = null;
		private DateTime lastMessageReceivedAt;

		~StreamObserver()
		{
			this.Dispose(false);
		}

		public StreamObserver(StreamReader argStream, Object argMessageSyncer = null)
		{
			if (argStream == null)
			{
				throw new ArgumentNullException("argStream");
			}

			this.stream = argStream;
			this.messageSyncer = new SharedLock(argMessageSyncer);
		}

		public void BeginObservation()
		{
			if (this.streamReadingThread != null)
			{
				throw new Exception("Already observing the stream");
			}

			this.streamReadingThread = new Thread(new ThreadStart(this.ObserveStreamInBackground));
			this.streamReadingThread.IsBackground = true;
			this.streamReadingThread.Priority = ThreadPriority.BelowNormal;
			this.streamReadingThread.Start();
		}

		private void ObserveStreamInBackground()
		{
			try
			{
				char? tmpStreamChar = null;

				do
				{
					int tmpStatus = this.Stream.Peek();
					if (tmpStatus == -1)
					{
						if (this.messageBuffer.Length > 0)
						{
							this.InformMessage();
						}

						this.messageSyncer.ReleaseLock();
						tmpStreamChar = (char)this.Stream.Read();
					}

					if (this.messageBuffer.Length == 0)
					{
						this.messageSyncer.AquireLock();
					}

					char tmpCurrentChar = tmpStreamChar ?? (char)this.Stream.Read();
					if (tmpStreamChar.HasValue)
					{
						tmpStreamChar = null;
					}

					this.messageBuffer.Append(tmpCurrentChar);
					if (tmpCurrentChar == '\n')
					{
						this.InformMessage();
					}
				} while (true);
			}
			catch (ThreadAbortException)
			{
				this.messageSyncer.ReleaseLock();
				SimpleFileLogger.Instance.LogMessage("Stream Observer thread aborted");
			}
			catch (Exception ex)
			{
				this.messageSyncer.ReleaseLock();
				SimpleFileLogger.Instance.LogError("Error while observing stream", ex);
			}
		}

		private void InformMessage()
		{
			string tmpMessage = this.messageBuffer.ToString();
			this.messageBuffer.Clear();
			if (lastMessage == null || lastMessage.Trim() != tmpMessage.Trim())
			{
				this.lastMessage = tmpMessage;
				this.lastMessageReceivedAt = DateTime.Now;
				this.OnMessageReceived(tmpMessage);
			}
		}

		private void OnMessageReceived(string argMessage)
		{
			StreamObserverEventHandler temp = this.MessageReceived;
			if (temp != null)
			{
				temp(this, argMessage);
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
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
							try
							{
								this.Stream.Close();
								this.streamReadingThread.Abort();
							}
							catch
							{
							}
							finally
							{
								this.streamReadingThread = null;
							}
						}

						this.isDisposed = true;
					}
				}).SyncronizeCallByLocking(this.disposalLock);
			}
		}

		protected StreamReader Stream
		{
			get
			{
				return this.stream;
			}
		}
	}
}