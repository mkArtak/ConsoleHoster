//-----------------------------------------------------------------------
// <copyright file="SuggestionContext.cs" author="Artak Mkrtchyan">
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

namespace ConsoleHoster.Model
{
	internal class SuggestionContext
	{
		private readonly SuggestionProvider nameSuggestionProvider = new SuggestionProvider();

		private string folder;
		private string suggestionBase;
		private string lastSuggestionBase;

		public SuggestionContext(string argFolder, string argSuggestionBase)
		{
			this.PrepareAllSuggestions(argFolder, argSuggestionBase);
		}

		private void PrepareAllSuggestions(string argFolder, string argSuggestionBase)
		{
			this.folder = argFolder;
			this.SuggestionBase = argSuggestionBase;
			this.lastSuggestionBase = argSuggestionBase;

			this.NameSuggestionProvider.PrepareSuggestion(this.SuggestionBase, this.folder);
		}

		public string GetNextSuggestion()
		{
			this.LastSuggestionBase = this.NameSuggestionProvider.GetSuggestion();
			return this.LastSuggestionBase;
		}

		public string LastSuggestionBase
		{
			get
			{
				return this.lastSuggestionBase;
			}
			private set
			{
				this.lastSuggestionBase = value;
			}
		}

		private SuggestionProvider NameSuggestionProvider
		{
			get
			{
				return this.nameSuggestionProvider;
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
	}
}