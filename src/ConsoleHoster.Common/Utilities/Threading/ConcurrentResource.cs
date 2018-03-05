//-----------------------------------------------------------------------
// <copyright file="ConcurrentResource.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>30/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleHoster.Common.Utilities.Threading
{
	public class ConcurrentResource<T>
	{
		private readonly Object SyncRoot = new Object();
		public readonly T Resource;

		public ConcurrentResource(T argResource)
		{
			if (argResource == null)
			{
				throw new ArgumentNullException("argResource");
			}

			this.Resource = argResource;
		}

		public void LockForAction(Action argAction)
		{
			lock (this.SyncRoot)
			{
				argAction();
			}
		}

		public void LockForActionWithPulse(Action argAction)
		{
			lock (this.SyncRoot)
			{
				if (argAction != null)
				{
					argAction();
				}
				this.Pulse();
			}
		}

		public void Pulse()
		{
			Monitor.PulseAll(this.SyncRoot);
		}

		public void Wait()
		{
			Monitor.Wait(this.SyncRoot);
		}

		public void ReleaseAnyWaitingThreads()
		{
			this.LockForActionWithPulse(null);
		}
	}
}
