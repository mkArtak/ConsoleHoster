//-----------------------------------------------------------------------
// <copyright file="ConsoleProject.cs" author="ArtakLaptop\Artak.Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Xml.Linq;
using ConsoleHoster.Common.Utilities;

namespace ConsoleHoster.Model
{
	public class ConsoleProject : INotifyPropertyChanged
	{
		private const string ATTRIBUTE_ARGUMENTS = "arguments";
		private const string ATTRIBUTE_NAME = "name";
		private const string ATTRIBUTE_EXECUTABLE = "executable";
		private const string ATTRIBUTE_WORKING_DIR = "workingdirectory";
		private const string ATTRIBUTE_AUTO_LOAD = "autoload";

		private const string ATTRIBUTE_MESSAGE_COLOR = "messagecolor";
		private const string ATTRIBUTE_ERROR_COLOR = "errorcolor";
		private const string ATTRIBUTE_CARET_COLOR = "caretcolor";
		private const string ATTRIBUTE_BACKGROUND_COLOR = "backgroundcolor";

		public event PropertyChangedEventHandler PropertyChanged;

		private string name;
		private string arguments;
		private string executable;
		private string workingDir;
		private bool autoLoad = false;
		private ObservableCollection<CommandData> commands = null;
		private Color errorMessageColor = Colors.Red;
		private Color messageColor = Colors.White;
		private Color backgroundColor = Colors.Black;
		private Color caretColor = Colors.White;
		private bool isEditable = true;

		internal static ConsoleProject LoadFromXElement(XElement argXml)
		{
			ConsoleProject tmpProject = new ConsoleProject();
			foreach (XAttribute tmpAttribute in argXml.Attributes())
			{
				switch (tmpAttribute.Name.LocalName.ToLower())
				{
					case ATTRIBUTE_ARGUMENTS:
						tmpProject.Arguments = tmpAttribute.Value;
						break;

					case ATTRIBUTE_EXECUTABLE:
						tmpProject.Executable = tmpAttribute.Value;
						break;

					case ATTRIBUTE_NAME:
						tmpProject.Name = tmpAttribute.Value;
						break;

					case ATTRIBUTE_WORKING_DIR:
						tmpProject.WorkingDir = tmpAttribute.Value;
						break;

					case ATTRIBUTE_AUTO_LOAD:
						bool tmpLoad;
						if (!Boolean.TryParse(tmpAttribute.Value, out tmpLoad))
						{
							tmpLoad = false;
						}
						tmpProject.AutoLoad = tmpLoad;
						break;

					case ATTRIBUTE_BACKGROUND_COLOR:
						tmpProject.BackgroundColor = ColorUtilities.GetColorFromString(tmpAttribute.Value, Colors.Black);
						break;

					case ATTRIBUTE_MESSAGE_COLOR:
						tmpProject.MessageColor = ColorUtilities.GetColorFromString(tmpAttribute.Value, Colors.White);
						break;

					case ATTRIBUTE_ERROR_COLOR:
						tmpProject.ErrorMessageColor = ColorUtilities.GetColorFromString(tmpAttribute.Value, Colors.Red);
						break;

					case ATTRIBUTE_CARET_COLOR:
						tmpProject.CaretColor = ColorUtilities.GetColorFromString(tmpAttribute.Value, Colors.White);
						break;
				}
			}
			return tmpProject;
		}

		private static string GetColorName(Color argColor)
		{
			return argColor.ToString();
		}

		public ConsoleProject()
		{

		}

		private ConsoleProject(ConsoleProject argCopyFrom)
		{
			this.name = argCopyFrom.Name;
			this.arguments = argCopyFrom.Arguments;
			this.autoLoad = argCopyFrom.AutoLoad;
			this.backgroundColor = argCopyFrom.BackgroundColor;
			this.caretColor = argCopyFrom.CaretColor;
			if (argCopyFrom.Commands != null)
			{
				this.commands = new ObservableCollection<CommandData>(argCopyFrom.Commands.Select(item => item.GetCopy()));
			}
			this.errorMessageColor = argCopyFrom.ErrorMessageColor;
			this.executable = argCopyFrom.Executable;
			this.messageColor = argCopyFrom.MessageColor;
			this.workingDir = argCopyFrom.WorkingDir;
		}

		private void NotifyPropertyChange(string argProperty)
		{
			PropertyChangedEventHandler temp = this.PropertyChanged;
			if (temp != null)
			{
				temp(this, new PropertyChangedEventArgs(argProperty));
			}
		}

		internal XElement ToXElement()
		{
			XElement tmpResult = new XElement("Project");

			tmpResult.SetAttributeValue(ATTRIBUTE_NAME, this.Name);
			tmpResult.SetAttributeValue(ATTRIBUTE_EXECUTABLE, this.Executable);
			if (!String.IsNullOrWhiteSpace(this.Arguments))
			{
				tmpResult.SetAttributeValue(ATTRIBUTE_ARGUMENTS, this.Arguments.Trim());
			}
			if (!String.IsNullOrWhiteSpace(this.WorkingDir))
			{
				tmpResult.SetAttributeValue(ATTRIBUTE_WORKING_DIR, this.WorkingDir);
			}
			tmpResult.SetAttributeValue(ATTRIBUTE_AUTO_LOAD, this.AutoLoad);

			tmpResult.SetAttributeValue(ATTRIBUTE_BACKGROUND_COLOR, GetColorName(this.BackgroundColor));
			tmpResult.SetAttributeValue(ATTRIBUTE_CARET_COLOR, GetColorName(this.CaretColor));
			tmpResult.SetAttributeValue(ATTRIBUTE_ERROR_COLOR, GetColorName(this.ErrorMessageColor));
			tmpResult.SetAttributeValue(ATTRIBUTE_MESSAGE_COLOR, GetColorName(this.MessageColor));

			return tmpResult;
		}

		internal ConsoleProject GetCopy()
		{
			return new ConsoleProject(this);
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
				if (value != this.name)
				{
					this.name = value;
					this.NotifyPropertyChange("Name");
				}
			}
		}

		public string Executable
		{
			get
			{
				return this.executable;
			}
			set
			{
				if (String.IsNullOrWhiteSpace(value))
				{
					throw new Exception("Executable is mandatory field");
				}

				if (value != this.executable)
				{
					this.executable = value;
					this.NotifyPropertyChange("Executable");
				}
			}
		}

		public string Arguments
		{
			get
			{
				return this.arguments;
			}
			set
			{
				if (value != this.arguments)
				{
					this.arguments = value;
					this.NotifyPropertyChange("Arguments");
				}
			}
		}

		public string WorkingDir
		{
			get
			{
				return this.workingDir;
			}
			set
			{
				this.workingDir = value;
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

		public ObservableCollection<CommandData> Commands
		{
			get
			{
				return this.commands;
			}
			set
			{
				this.commands = value;
			}
		}

		public Color ErrorMessageColor
		{
			get
			{
				return this.errorMessageColor;
			}
			set
			{
				if (value != this.errorMessageColor)
				{
					this.errorMessageColor = value;
					this.NotifyPropertyChange("ErrorMessageColor");
				}
			}
		}

		public Color MessageColor
		{
			get
			{
				return this.messageColor;
			}
			set
			{
				if (value != this.messageColor)
				{
					this.messageColor = value;
					this.NotifyPropertyChange("MessageColor");
				}
			}
		}

		public Color BackgroundColor
		{
			get
			{
				return this.backgroundColor;
			}
			set
			{
				if (value != this.backgroundColor)
				{
					this.backgroundColor = value;
					this.NotifyPropertyChange("BackgroundColor");
				}
			}
		}

		public Color CaretColor
		{
			get
			{
				return this.caretColor;
			}
			set
			{
				this.caretColor = value;
			}
		}

		public bool IsEditable
		{
			get
			{
				return this.isEditable;
			}
			set
			{
				this.isEditable = value;
			}
		}
		#endregion
	}
}