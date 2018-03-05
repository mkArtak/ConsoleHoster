//-----------------------------------------------------------------------
// <copyright file="CommandData.cs" author="ArtakLaptop\Artak.Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Xml.Linq;

namespace ConsoleHoster.Model
{
	public class CommandData
	{
		private const string ATTRIBUTE_NAME = "name";
		private const string ATTRIBUTE_GROUP = "group";
		private const string ATTRIBUTE_COMMAND_TEXT = "commandtext";
		private const string ATTRIBUTE_IS_FINAL = "isfinal";

		private string commandText;
		private string name;
		private bool isFinal = false;
		private string groupName = String.Empty;

		internal static CommandData LoadFromXElement(XElement argXml)
		{
			CommandData tmpData = new CommandData();
			foreach (XAttribute tmpCommandAttribute in argXml.Attributes())
			{
				switch (tmpCommandAttribute.Name.LocalName.ToLower())
				{
					case ATTRIBUTE_COMMAND_TEXT:
						tmpData.CommandText = tmpCommandAttribute.Value;
						break;

					case ATTRIBUTE_NAME:
						tmpData.Name = tmpCommandAttribute.Value;
						break;

					case ATTRIBUTE_GROUP:
						tmpData.GroupName = tmpCommandAttribute.Value.Trim();
						break;

					case ATTRIBUTE_IS_FINAL:
						bool tmpIsFinal;
						if (!Boolean.TryParse(tmpCommandAttribute.Value, out tmpIsFinal))
						{
							tmpIsFinal = false;
						}
						tmpData.IsFinal = tmpIsFinal;
						break;
				}
			}

			if (!String.IsNullOrWhiteSpace(argXml.Value))
			{
				tmpData.CommandText = argXml.Value.Trim();
			}
			return tmpData;
		}

		public CommandData()
		{
		}

		private CommandData(CommandData argCopyFrom)
		{
			this.CommandText = argCopyFrom.CommandText;
			this.GroupName = argCopyFrom.GroupName;
			this.IsFinal = argCopyFrom.IsFinal;
			this.Name = argCopyFrom.Name;
		}

		internal XElement ToXElement()
		{
			XElement tmpResult = new XElement("Command");

			tmpResult.SetAttributeValue(ATTRIBUTE_NAME, this.Name);
			tmpResult.Value = this.CommandText;
			if (!String.IsNullOrWhiteSpace(this.GroupName))
			{
				tmpResult.SetAttributeValue(ATTRIBUTE_GROUP, this.GroupName);
			}
			tmpResult.SetAttributeValue(ATTRIBUTE_IS_FINAL, this.IsFinal.ToString());

			return tmpResult;
		}

		internal CommandData GetCopy()
		{
			return new CommandData(this);
		}

		#region Properties
		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				this.name = value;
			}
		}

		public string CommandText
		{
			get
			{
				return this.commandText;
			}
			set
			{
				this.commandText = value;
			}
		}

		public bool IsFinal
		{
			get
			{
				return this.isFinal;
			}
			set
			{
				this.isFinal = value;
			}
		}

		public string GroupName
		{
			get
			{
				return this.groupName;
			}
			set
			{
				this.groupName = value;
			}
		}
		#endregion
	}
}