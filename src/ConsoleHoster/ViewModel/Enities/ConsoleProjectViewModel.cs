//-----------------------------------------------------------------------
// <copyright file="ConsoleProjectViewModel.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using ConsoleHoster.Common.ViewModel;
using ConsoleHoster.Model.Entities;
using ConsoleHoster.ViewModel.Enities;

namespace ConsoleHoster.ViewModel.Entities
{
	public class ConsoleProjectViewModel : ViewModelEntityBase<ConsoleProject>
	{
		private ObservableCollection<CommandDataViewModel> commands = null;

		public ConsoleProjectViewModel(ConsoleProject argModel)
			: base(argModel)
		{
			if (argModel.Commands != null)
			{
				this.commands = new ObservableCollection<CommandDataViewModel>(argModel.Commands.Select(item => new CommandDataViewModel(item)).OrderBy(item => item.Name));
			}
		}

		public ConsoleProjectViewModel()
			: this(new ConsoleProject())
		{

		}

		internal ConsoleProjectViewModel GetCopy()
		{
			return new ConsoleProjectViewModel(this.Model.GetCopy());
		}

		protected override void ApplyChangesToModel(ConsoleProject argModel)
		{
			argModel.Commands = this.Commands == null ? null : new ObservableCollection<CommandData>(this.Commands.Select(item => item.Model).OrderBy(item => item.Name));
		}

		#region Properties
		public string Name
		{
			get
			{
				return this.Model.Name;
			}
			set
			{
				if (value != this.Model.Name)
				{
					this.Model.Name = value;
					this.NotifyPropertyChanged("Name");
					this.NotifyPropertyChanged("UIFriendlyName");
				}
			}
		}

		public string UIFriendlyName
		{
			get
			{
				return this.Name.Replace("_", "__");
			}
		}

		public string Executable
		{
			get
			{
				return this.Model.Executable;
			}
			set
			{
				if (value != this.Model.Executable)
				{
					this.Model.Executable = value;
					this.NotifyPropertyChanged("Executable");
				}
			}
		}

		public string Arguments
		{
			get
			{
				return this.Model.Arguments;
			}
			set
			{
				if (value != this.Model.Arguments)
				{
					this.Model.Arguments = value;
					this.NotifyPropertyChanged("Arguments");
				}
			}
		}

		public string WorkingDir
		{
			get
			{
				return this.Model.WorkingDir;
			}
			set
			{
				if (value != this.Model.WorkingDir)
				{
					this.Model.WorkingDir = value;
					this.NotifyPropertyChanged("WorkingDir");
				}
			}
		}

		public bool AutoLoad
		{
			get
			{
				return this.Model.AutoLoad;
			}
			set
			{
				if (value != this.Model.AutoLoad)
				{
					this.Model.AutoLoad = value;
					this.NotifyPropertyChanged("AutoLoad");
				}
			}
		}

		public ObservableCollection<CommandDataViewModel> Commands
		{
			get
			{
				return this.commands;
			}
			set
			{
				if (value != this.commands)
				{
					this.commands = value;
					this.NotifyPropertyChanged("Commands");
				}
			}
		}

		public Color ProjectColor
		{
			get
			{
				return this.Model.ProjectColor;
			}
			set
			{
				this.Model.ProjectColor = value;
				this.NotifyPropertyChanged("ProjectColor");
				this.NotifyPropertyChanged("ProjectColorBrush");
			}
		}

		public Brush ProjectColorBrush
		{
			get
			{
				return new SolidColorBrush(this.ProjectColor);
			}
		}

		public Color ErrorMessageColor
		{
			get
			{
				return this.Model.ErrorMessageColor;
			}
			set
			{
				if (value != this.Model.ErrorMessageColor)
				{
					this.Model.ErrorMessageColor = value;
					this.NotifyPropertyChanged("ErrorMessageColor");
				}
			}
		}

		public Color MessageColor
		{
			get
			{
				return this.Model.MessageColor;
			}
			set
			{
				if (value != this.Model.MessageColor)
				{
					this.Model.MessageColor = value;
					this.NotifyPropertyChanged("MessageColor");
				}
			}
		}

		public Color BackgroundColor
		{
			get
			{
				return this.Model.BackgroundColor;
			}
			set
			{
				if (value != this.Model.BackgroundColor)
				{
					this.Model.BackgroundColor = value;
					this.NotifyPropertyChanged("BackgroundColor");
				}
			}
		}

		public Color CaretColor
		{
			get
			{
				return this.Model.CaretColor;
			}
			set
			{
				if (value != this.Model.CaretColor)
				{
					this.Model.CaretColor = value;
					this.NotifyPropertyChanged("CaretColor");
				}
			}
		}

		public bool IsEditable
		{
			get
			{
				return this.Model.IsEditable;
			}
			set
			{
				if (value != this.Model.IsEditable)
				{
					this.Model.IsEditable = value;
					this.NotifyPropertyChanged("IsEditable");
				}
			}
		}
		#endregion
	}
}
