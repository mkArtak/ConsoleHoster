//-----------------------------------------------------------------------
// <copyright file="GlobalSettings.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Windows.Media;

namespace ConsoleHoster.Model
{
	public class GlobalSettings : INotifyPropertyChanged
	{
		private const string TAG_FONT = "fontfamily";

		public event PropertyChangedEventHandler PropertyChanged;

		private static readonly GlobalSettings instance = new GlobalSettings();
		private System.Windows.Media.FontFamily fontFamily = new FontFamily("Consolas");

		private GlobalSettings()
		{

		}

		public void SetFontFamily(string argFontFamily)
		{
			if (!String.IsNullOrWhiteSpace(argFontFamily))
			{
				this.FontFamily = new FontFamily(argFontFamily);
			}
		}

		private void NotifyPropertyChanged(string argProperty)
		{
			PropertyChangedEventHandler tmpEH = this.PropertyChanged;
			if (tmpEH != null)
			{
				tmpEH(this, new PropertyChangedEventArgs(argProperty));
			}
		}

		public static GlobalSettings Instance
		{
			get
			{
				return instance;
			}
		}

		public FontFamily FontFamily
		{
			get
			{
				return this.fontFamily;
			}
			private set
			{
				this.fontFamily = value;
				this.NotifyPropertyChanged("FontFamily");
			}
		}
	}
}