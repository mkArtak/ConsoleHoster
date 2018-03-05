//-----------------------------------------------------------------------
// <copyright file="ExplorerItem.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace ConsoleHoster.Common.Model
{
	public class ExplorerItem
	{
		private IList<ExplorerItem> childItems;
		private readonly ExplorerItemType itemType;
		private string name;
		private string path;
		private string alias;

		public ExplorerItem(ExplorerItemType argItemType)
		{
			this.itemType = argItemType;
		}

		public ExplorerItemType ItemType
		{
			get
			{
				return this.itemType;
			}
		}

		public string Alias
		{
			get
			{
				return this.alias ?? this.Name;
			}
			set
			{
				this.alias = value;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				if (String.IsNullOrWhiteSpace(value))
				{
					throw new ArgumentException("Value must be provided");
				}

				this.name = value;
			}
		}

		public string Path
		{
			get
			{
				return this.path;
			}
			set
			{
				this.path = value;
			}
		}
	}
}