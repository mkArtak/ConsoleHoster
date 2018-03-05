//-----------------------------------------------------------------------
// <copyright file="VersionUpgradeApplier.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Linq;
using System.Xml.Linq;
using ConsoleHoster.Common.Utilities;

namespace ConsoleHoster.Model
{
	internal static class VersionUpgradeApplier
	{
		public static void ApplyVersionUpgrade(int versionToUpgradeTo)
		{
			if (versionToUpgradeTo >= 108)
			{
				UpgradeFromVersion1_07To1_08();
			}
		}

		public static void ApplyFullVersionUpgrade()
		{
			ApplyVersionUpgrade(Int32.MaxValue);
		}

		private static void UpgradeFromVersion1_07To1_08()
		{
			try
			{
				XDocument tmpDocument = XDocument.Load(StorageManager.PROJECTS_FILE_NAME);
				if (!tmpDocument.Descendants("HosterConfig").Any())
				{
					XElement tmpHosterConfig = new XElement("HosterConfig");
					XElement tmpProjects = tmpDocument.Descendants("Projects").SingleOrDefault();
					if (tmpProjects != null)
					{
						tmpProjects.Remove();
						tmpHosterConfig.Add(tmpProjects);
					}

					tmpDocument.Add(tmpHosterConfig);
					tmpDocument.Save(StorageManager.PROJECTS_FILE_NAME);
				}
			}
			catch (Exception ex)
			{
				SimpleFileLogger.Instance.LogError("Unable to upgrade configuration from version 1.07 to version 1.08", ex);
				throw new ApplicationException("Unable to upgrade configuration", ex);
			}
		}
	}
}