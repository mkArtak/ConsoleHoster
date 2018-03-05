//-----------------------------------------------------------------------
// <copyright file="ActionExtensions.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleHoster.Common.Utilities
{
	public static class ActionExtensions
	{
		public static void SyncronizeCallByLocking(this Action argAction, object argLock, int argTimeoutMilliseconds = 1000, bool argUseRetries = false)
		{
			int tmpTriesLeft = 3;
			do
			{
				if (!Monitor.TryEnter(argLock, argTimeoutMilliseconds))
				{
					SimpleFileLogger.Instance.LogMessage(String.Format("Unable to retrieve lock during {0}ms. Retrying. Retry No: {1}", argTimeoutMilliseconds.ToString(), 3 - tmpTriesLeft));
					if (!argUseRetries || --tmpTriesLeft == 0)
					{
						throw new ApplicationException("Unable to lock the object");
					}
				}
				else
				{
					break;
				}
			} while (true);

			try
			{
				argAction();
			}
			catch (Exception ex)
			{
				SimpleFileLogger.Instance.LogError(ex);
				throw;
			}
			finally
			{
				Monitor.Exit(argLock);
			}
		}

		public static void SyncronizeCallByLockingSafely(this Action argAction, object argLock, int argTimeoutMilliseconds = 1000, bool argUseRetries = false)
		{
			try
			{
				SyncronizeCallByLocking(argAction, argLock, argTimeoutMilliseconds, argUseRetries);
			}
			catch (Exception ex)
			{
				SimpleFileLogger.Instance.LogError(ex);
			}
		}
	}
}