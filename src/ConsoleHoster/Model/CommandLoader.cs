//-----------------------------------------------------------------------
// <copyright file="CommandLoader.cs" author="ArtakLaptop\Artak.Mkrtchyan">
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
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleHoster.Model
{
    public static class CommandLoader
    {
        private const string PROJECTS_FILE_NAME = "./Projects.xml";

        private const string TAG_NAME = "Command";

        public static IEnumerable<CommandData> LoadCommands()
        {
            IList<CommandData> tmpResult;
            if (File.Exists(PROJECTS_FILE_NAME))
            {
                try
                {
                    tmpResult = new List<CommandData>();
                    XDocument tmpProjectsDocument = XDocument.Load(PROJECTS_FILE_NAME);
                    XElement tmpCommandsRootXml = tmpProjectsDocument.Descendants("GenericCommands").SingleOrDefault();
                    if (tmpCommandsRootXml != null)
                    {
                        foreach (XElement tmpCommandXml in tmpCommandsRootXml.Descendants(TAG_NAME))
                        {
                            tmpResult.Add(CommandData.LoadFromXElement(tmpCommandXml));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to load commands");
                }
            }
            else
            {
                tmpResult = null;
            }
            return tmpResult;
        }
    }
}