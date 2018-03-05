//-----------------------------------------------------------------------
// <copyright file="ObservableQueue.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ConsoleHoster.Common.Utilities;

namespace ConsoleHoster.Model
{
    public sealed class ObservableQueue<T> : ObservableCollection<T>
    {
        private int queueSize;
        private readonly Object syncRoot = new Object();

        public ObservableQueue(int size)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            this.queueSize = size;
        }

        protected override void InsertItem(int index, T item)
        {
            new Action(() =>
                {
                    int tmpInsertionIndex = index;
                    if (this.queueSize != 0 && this.Count >= this.queueSize)
                    {
                        this.RemoveAt(0);
                        tmpInsertionIndex = index - 1;
                    }

                    base.InsertItem(tmpInsertionIndex, item);
                }).SyncronizeCallByLocking(this.syncRoot);
        }
    }
}