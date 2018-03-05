//-----------------------------------------------------------------------
// <copyright file="CommandDataViewModel.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>28/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System.Windows.Media;
using ConsoleHoster.Common.ViewModel;
using ConsoleHoster.Model.Entities;

namespace ConsoleHoster.ViewModel.Enities
{
	public class CommandDataViewModel : ViewModelEntityBase<CommandData>
	{
		public CommandDataViewModel(CommandData argModel)
			: base(argModel)
		{

		}

		public CommandDataViewModel()
			: this(new CommandData())
		{

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
				}
			}
		}

		public string CommandText
		{
			get
			{
				return this.Model.CommandText;
			}
			set
			{
				if (value != this.Model.CommandText)
				{
					this.Model.CommandText = value;
					this.NotifyPropertyChanged("CommandText");
				}
			}
		}

		public bool IsFinal
		{
			get
			{
				return this.Model.IsFinal;
			}
			set
			{
				if (value != this.Model.IsFinal)
				{
					this.Model.IsFinal = value;
					this.NotifyPropertyChanged("IsFinal");
				}
			}
		}

		public string GroupName
		{
			get
			{
				return this.Model.GroupName;
			}
			set
			{
				if (value != this.Model.GroupName)
				{
					this.Model.GroupName = value;
					this.NotifyPropertyChanged("GroupName");
				}
			}
		}

		public Color CommandColor
		{
			get
			{
				return this.Model.CommandColor;
			}
			set
			{
				if (value != this.Model.CommandColor)
				{
					this.Model.CommandColor = value;
					this.NotifyPropertyChanged("CommandColor");
					this.NotifyPropertyChanged("ColorBrush");
				}
			}
		}

		public Brush ColorBrush
		{
			get
			{
				return new SolidColorBrush(this.CommandColor);
			}
		}
		#endregion
	}
}
