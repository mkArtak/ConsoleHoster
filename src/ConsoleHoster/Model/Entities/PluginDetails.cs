//-----------------------------------------------------------------------
// <copyright file="PluginDetails.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>17/08/2012</date>
//-----------------------------------------------------------------------
using ConsoleHoster.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ConsoleHoster.Model.Entities
{
	internal class PluginDetails
	{
		private const string ATTRIBUTE_NAME = "name";
		private const string ATTRIBUTE_DETAILS = "details";
		private const string ATTRIBUTE_ASSEMBLY_FULL_NAME = "assemblyfullname";
		private const string ATTRIBUTE_PLUGIN_TYPE_NAME = "plugintypename";
		private const string ATTRIBUTE_AUTO_LOAD = "autoload";

		private string name;
		private string details;
		private string assemblyFullName;
		private string pluginTypeName;
		private bool autoLoad = false;

		internal static PluginDetails LoadFromXElement(XElement argXml)
		{
			PluginDetails tmpResult = new PluginDetails();
			foreach (XAttribute tmpAttribute in argXml.Attributes())
			{
				switch (tmpAttribute.Name.LocalName.ToLower())
				{
					case ATTRIBUTE_ASSEMBLY_FULL_NAME:
						tmpResult.AssemblyFullName = tmpAttribute.Value.Trim();
						break;

					case ATTRIBUTE_AUTO_LOAD:
						bool tmpAutoLoad;
						if (!Boolean.TryParse(tmpAttribute.Value, out tmpAutoLoad))
						{
							tmpAutoLoad = false;
						}
						tmpResult.AutoLoad = tmpAutoLoad;
						break;

					case ATTRIBUTE_DETAILS:
						tmpResult.Details = tmpAttribute.Value.Trim();
						break;

					case ATTRIBUTE_NAME:
						tmpResult.Name = tmpAttribute.Value.Trim();
						break;

					case ATTRIBUTE_PLUGIN_TYPE_NAME:
						tmpResult.PluginTypeName = tmpAttribute.Value.Trim();
						break;
				}
			}
			tmpResult.ValidateDetails();

			return tmpResult;
		}

		public PluginDetails()
		{

		}

		private void ValidateDetails()
		{
			this.ValidateMandatoryParameter(this.AssemblyFullName, "AssemblyFullame");
			this.ValidateMandatoryParameter(this.Details, "Details");
			this.ValidateMandatoryParameter(this.Name, "Name");
			this.ValidateMandatoryParameter(this.PluginTypeName, "PluginTypeName");
		}

		private void ValidateMandatoryParameter(string argValue, string argParameterName)
		{
			if (String.IsNullOrWhiteSpace(argValue))
			{
				throw new ApplicationException(String.Format("Missing mandatory parameter {0}", argParameterName));
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
				this.name = value;
			}
		}

		public string Details
		{
			get
			{
				return this.details;
			}
			set
			{
				this.details = value;
			}
		}

		public string AssemblyFullName
		{
			get
			{
				return this.assemblyFullName;
			}
			set
			{
				this.assemblyFullName = value;
			}
		}

		public string PluginTypeName
		{
			get
			{
				return this.pluginTypeName;
			}
			set
			{
				this.pluginTypeName = value;
			}
		}

		public bool AutoLoad
		{
			get
			{
				return this.autoLoad;
			}
			set
			{
				this.autoLoad = value;
			}
		}
	}
}
