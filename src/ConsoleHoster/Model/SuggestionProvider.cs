//-----------------------------------------------------------------------
// <copyright file="SuggestionProvider.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleHoster.Model
{
    public class SuggestionProvider
    {
        private string folder;
        private string suggestionBase;
        private int currentSuggestionIndex;
        private readonly IList<string> suggestions = new List<string>();

        public SuggestionProvider()
        {
        }

        public void PrepareSuggestion(string argBase, string argFolder)
        {
            if (!Directory.Exists(argFolder))
            {
                throw new ArgumentException("No such directory");
            }

            this.Folder = argFolder;
            this.SuggestionBase = argBase;
            this.CurrentSuggestionIndex = 0;

            this.Suggestions.Clear();
            string[] tmpItems = Directory.GetFileSystemEntries(this.Folder, this.SuggestionBase + "*", SearchOption.TopDirectoryOnly);
            foreach (string tmpItem in tmpItems)
            {

                int tmpIndex = tmpItem.LastIndexOf('/');
                if (tmpIndex == -1)
                {
                    tmpIndex = tmpItem.LastIndexOf('\\');
                }
                this.Suggestions.Add(tmpItem.Substring(tmpIndex + 1));
            }
        }

        public string GetSuggestion()
        {
            string tmpResult;
            if (this.Suggestions.Any())
            {
                tmpResult = this.Suggestions[this.CurrentSuggestionIndex];
                if (++this.CurrentSuggestionIndex >= this.Suggestions.Count())
                {
                    this.CurrentSuggestionIndex = 0;
                }
            }
            else
            {
                tmpResult = null;
            }
            return tmpResult;
        }

        #region Properties
        private IList<string> Suggestions
        {
            get
            {
                return suggestions;
            }
        }

        public string Folder
        {
            get
            {
                return this.folder;
            }
            private set
            {
                this.folder = value;
            }
        }

        public int CurrentSuggestionIndex
        {
            get
            {
                return this.currentSuggestionIndex;
            }
            private set
            {
                this.currentSuggestionIndex = value;
            }
        }

        public string SuggestionBase
        {
            get
            {
                return this.suggestionBase;
            }
            private set
            {
                this.suggestionBase = value;
            }
        }
        #endregion
    }
}