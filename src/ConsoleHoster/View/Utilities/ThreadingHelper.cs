//-----------------------------------------------------------------------
// <copyright file="ThreadingHelper.cs" author="Artak Mkrtchyan">
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
using System.Windows;
using ConsoleHoster.Common.Utilities;

namespace ConsoleHoster.View.Utilities
{
    public static class ThreadingHelper
    {
        private static readonly IList<Guid> syncList = new List<Guid>();

        public static void DoOnUIThreadSyncronousely(this DependencyObject argActionOwner, Action argAction)
        {
            if (argActionOwner.Dispatcher.Thread != Thread.CurrentThread)
            {
                argActionOwner.Dispatcher.Invoke(new Action(() => argActionOwner.DoOnUIThreadSyncronousely(argAction)));
                return;
            }

            argAction();
        }
    }
}