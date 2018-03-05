//-----------------------------------------------------------------------
// <copyright file="SharedLock.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>11/08/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------

using System;
using System.Runtime.ConstrainedExecution;
using System.Threading;

namespace ConsoleHoster.Common.Utilities.Threading
{
	public class SharedLock : CriticalFinalizerObject, IDisposable
	{
		private readonly Object sharedLock;

		private bool lockAquired = false;

		~SharedLock()
		{
			this.Dispose(false);
		}

		public SharedLock(Object argObjectToLock)
		{
			this.sharedLock = argObjectToLock;
		}

		public void AquireLock()
		{
			if (!this.lockAquired && this.sharedLock != null)
			{
				this.lockAquired = true;
				Monitor.Enter(this.sharedLock);
			}
		}

		public void ReleaseLock()
		{
			if (this.lockAquired)
			{
				try
				{
					Monitor.Exit(this.sharedLock);
				}
				catch (Exception ex)
				{
					SimpleFileLogger.Instance.LogError(ex);
				}
				this.lockAquired = false;
			}
		}

		public void Dispose()
		{
			this.Dispose(true);

			GC.SuppressFinalize(this);
		}

		private void Dispose(bool argDisposing)
		{
			this.ReleaseLock();
		}
	}
}
