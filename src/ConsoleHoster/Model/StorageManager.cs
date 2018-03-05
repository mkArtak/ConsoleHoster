//-----------------------------------------------------------------------
// <copyright file="StorageManager.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ConsoleHoster.Common.Utilities;
using ConsoleHoster.Model.Entities;

namespace ConsoleHoster.Model
{
	public static class StorageManager
	{
		public const string PROJECTS_FILE_NAME = "./Projects.xml";
		private const string TAG_GLOBAL_SETTINGS = "GlobalSettings";
		private const string TAG_FONT = "FontFamily";
		private const string TAG_PROJECT = "Project";
		private const string TAG_COMMAND = "Command";
		private const string TAG_GLOBAL_COMMANDS = "GenericCommands";
		private const string TAG_PLUGINS = "Plugins";

		public static IEnumerable<CommandData> LoadGlobalCommands()
		{
			IList<CommandData> tmpResult;
			if (File.Exists(PROJECTS_FILE_NAME))
			{
				try
				{
					tmpResult = new List<CommandData>();
					XDocument tmpProjectsDocument = XDocument.Load(PROJECTS_FILE_NAME);
					XElement tmpCommandsRootXml = tmpProjectsDocument.Descendants(TAG_GLOBAL_COMMANDS).SingleOrDefault();
					if (tmpCommandsRootXml != null)
					{
						foreach (XElement tmpCommandXml in tmpCommandsRootXml.Descendants(TAG_COMMAND))
						{
							tmpResult.Add(CommandData.LoadFromXElement(tmpCommandXml));
						}
					}
				}
				catch (Exception ex)
				{
					throw new Exception("Failed to load commands", ex);
				}
			}
			else
			{
				tmpResult = null;
			}
			return tmpResult;
		}

		public static IEnumerable<ConsoleProject> LoadProjects()
		{
			IList<ConsoleProject> tmpResult;
			if (File.Exists(PROJECTS_FILE_NAME))
			{
				try
				{
					tmpResult = new List<ConsoleProject>();
					XDocument tmpProjectsDocument = XDocument.Load(PROJECTS_FILE_NAME);
					foreach (XElement tmpProjectXml in tmpProjectsDocument.Descendants(TAG_PROJECT))
					{
						ConsoleProject tmpProject = ConsoleProject.LoadFromXElement(tmpProjectXml);
						tmpProject.Commands = new ObservableCollection<CommandData>();

						foreach (XElement tmpCommandXml in tmpProjectXml.Descendants(TAG_COMMAND))
						{
							CommandData tmpChildCommand = CommandData.LoadFromXElement(tmpCommandXml);
							tmpProject.Commands.Add(tmpChildCommand);
						}

						tmpResult.Add(tmpProject);
					}
				}
				catch (Exception ex)
				{
					throw new Exception("Failed to load projects", ex);
				}
			}
			else
			{
				tmpResult = null;
			}
			return tmpResult;
		}

		internal static IEnumerable<PluginDetails> LoadPlugins()
		{
			IList<PluginDetails> tmpResult;
			if (File.Exists(PROJECTS_FILE_NAME))
			{
				try
				{
					tmpResult = new List<PluginDetails>();
					XDocument tmpProjectsDocument = XDocument.Load(PROJECTS_FILE_NAME);
					foreach (XElement tmpPluginXml in tmpProjectsDocument.Descendants(TAG_PLUGINS))
					{
						PluginDetails tmpPlugin = PluginDetails.LoadFromXElement(tmpPluginXml);
						tmpResult.Add(tmpPlugin);
					}
				}
				catch (Exception ex)
				{
					throw new Exception("Failed to load plugins", ex);
				}
			}
			else
			{
				tmpResult = null;
			}
			return tmpResult;
		}

		public static void SaveProject(ConsoleProject argProject)
		{
			XDocument tmpConfigDoc = XDocument.Load(StorageManager.PROJECTS_FILE_NAME);
			AddProject(tmpConfigDoc, argProject);
			tmpConfigDoc.Save(StorageManager.PROJECTS_FILE_NAME);
		}

		public static void UpdateProject(string projectName, ConsoleProject argProject)
		{
			XDocument tmpConfigDoc = XDocument.Load(StorageManager.PROJECTS_FILE_NAME);
			RemoveProject(tmpConfigDoc, projectName);
			AddProject(tmpConfigDoc, argProject);
			tmpConfigDoc.Save(StorageManager.PROJECTS_FILE_NAME);
		}

		public static void DeleteProject(string argProjectName)
		{
			XDocument tmpConfigDoc = XDocument.Load(StorageManager.PROJECTS_FILE_NAME);
			RemoveProject(tmpConfigDoc, argProjectName);
			tmpConfigDoc.Save(StorageManager.PROJECTS_FILE_NAME);
		}

		private static void AddProject(XDocument tmpConfigDoc, ConsoleProject argProject)
		{
			XElement tmpProjectsNode;
			if ((tmpProjectsNode = tmpConfigDoc.Descendants("Projects").SingleOrDefault()) == null)
			{
				tmpProjectsNode = new XElement("Projects");
				tmpConfigDoc.Add(tmpProjectsNode);
			}

			XElement tmpProjectElement = argProject.ToXElement();
			tmpProjectsNode.Add(tmpProjectElement);

			if (argProject.Commands != null && argProject.Commands.Any())
			{
				XElement tmpCommandsElement = tmpProjectElement.Descendants("Commands").SingleOrDefault();
				if (tmpCommandsElement == null)
				{
					tmpCommandsElement = new XElement("Commands");
					tmpProjectElement.Add(tmpCommandsElement);
				}

				foreach (CommandData tmpCommand in argProject.Commands)
				{
					tmpCommandsElement.Add(tmpCommand.ToXElement());
				}
			}
		}

		private static void RemoveProject(XDocument argDocument, string argProjectName)
		{
			XElement tmpProjectsNode;
			if ((tmpProjectsNode = argDocument.Descendants("Projects").SingleOrDefault()) == null)
			{
				throw new ApplicationException("No such project");
			}

			XElement tmpCurrentProject = tmpProjectsNode.Descendants("Project").Where(item => item.Attribute("name").Value.Equals(argProjectName, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
			if (tmpCurrentProject == null)
			{
				throw new ApplicationException("No such project");
			}
			tmpCurrentProject.Remove();
		}

		internal static void LoadGlobalSettings()
		{
			SimpleFileLogger.Instance.LogMessage("Loading global settings");

			if (!File.Exists(PROJECTS_FILE_NAME))
			{
				throw new FileNotFoundException("Unable to find global settings file");
			}

			try
			{
				XDocument tmpProjectsDocument = GetSettingsXmlDocument();
				XElement tmpGlobalSettingsXml = tmpProjectsDocument.Descendants(TAG_GLOBAL_SETTINGS).SingleOrDefault();
				if (tmpGlobalSettingsXml != null)
				{
					XElement tmpFontXml = tmpGlobalSettingsXml.Descendants(TAG_FONT).LastOrDefault();
					if (tmpFontXml != null)
					{
						GlobalSettings.Instance.SetFontFamily(tmpFontXml.Value);
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to load global settings", ex);
			}
		}

		internal static void SaveGlobalSettings()
		{
			XDocument tmpProjectDocument;
			if (!File.Exists(PROJECTS_FILE_NAME))
			{
				tmpProjectDocument = XDocument.Parse(@"<?xml version='1.0' encoding='utf-8' ?>
<HosterConfig>
  <Projects></Projects>
  <GenericCommands>
    <Command Name='dir' CommandText='dir' isFinal='true' />
    <Command Name='Root Level' CommandText='cd \' isFinal='true' />
    <Command Name='Level Up' CommandText='cd ..' isFinal='true' />
  </GenericCommands>
  <GlobalSettings>
  </GlobalSettings>
</HosterConfig>");
				tmpProjectDocument.Save(PROJECTS_FILE_NAME);
			}
			else
			{
				tmpProjectDocument = GetSettingsXmlDocument();
			}
			XElement tmpHosterConfig = tmpProjectDocument.Descendants("HosterConfig").SingleOrDefault();
			if (tmpHosterConfig == null)
			{
				tmpHosterConfig = new XElement("HosterConfig");
				tmpProjectDocument.AddFirst(tmpHosterConfig);
			}
			XElement tmpGlobalSettings = tmpHosterConfig.Descendants("GlobalSettings").SingleOrDefault();
			if (tmpGlobalSettings == null)
			{
				tmpGlobalSettings = new XElement("GlobalSettings");
				tmpHosterConfig.Add(tmpGlobalSettings);
			}

			XElement tmpFontFamily = tmpGlobalSettings.Descendants("FontFamily").SingleOrDefault();
			if (tmpFontFamily == null)
			{
				tmpFontFamily = new XElement("FontFamily");
				tmpGlobalSettings.Add(tmpFontFamily);
			}

			tmpFontFamily.Value = GlobalSettings.Instance.FontFamily.Source;
			SaveSettingsDocument(tmpProjectDocument);
		}

		private static void SaveSettingsDocument(XDocument argXmlDocument)
		{
			argXmlDocument.Save(PROJECTS_FILE_NAME);
		}

		private static XDocument GetSettingsXmlDocument()
		{
			return XDocument.Load(PROJECTS_FILE_NAME);
		}
	}
}