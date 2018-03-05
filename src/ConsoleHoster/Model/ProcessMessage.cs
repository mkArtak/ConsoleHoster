//-----------------------------------------------------------------------
// <copyright file="ProcessMessage.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;

namespace ConsoleHoster.Model
{
    public struct ProcessMessage
    {
        private readonly string data;
        private readonly DateTime receivedDate;
        private readonly bool isOutputMessage;

        public ProcessMessage(string argData, DateTime argReceivedTime, bool argIsOutputMessage)
        {
            this.data = argData;
            this.receivedDate = argReceivedTime;
            this.isOutputMessage = argIsOutputMessage;
        }

        public string Data
        {
            get
            {
                return this.data;
            }
        }

        public DateTime ReceivedDate
        {
            get
            {
                return this.receivedDate;
            }
        }

        public bool IsOutputMessage
        {
            get
            {
                return this.isOutputMessage;
            }
        }
    }
}