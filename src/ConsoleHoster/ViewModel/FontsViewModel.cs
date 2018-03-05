//-----------------------------------------------------------------------
// <copyright file="FontsViewModel.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using ConsoleHoster.Common.ViewModel;
using ConsoleHoster.Model;

namespace ConsoleHoster.ViewModel
{
	internal class FontsViewModel : ViewModelBase
	{
		private ObservableCollection<string> fontFamilies;

		private string selectedFont;

		public FontsViewModel()
			: base()
		{
			this.SelectedFont = GlobalSettings.Instance.FontFamily.Source;
		}

		protected override void InitializeRuntimeMode()
		{
			base.InitializeRuntimeMode();

			Task threadLoadTask = new Task(() =>
			{
				this.RunActionSwitchingToBusyState(() =>
				{
					this.FontFamilies = new ObservableCollection<string>(Fonts.SystemFontFamilies.Select(item => item.Source).OrderBy(item => item));
				}, true);
			});

			threadLoadTask.Start();
		}

		public ObservableCollection<string> FontFamilies
		{
			get
			{
				return this.fontFamilies;
			}
			private set
			{
				this.fontFamilies = value;
				this.NotifyPropertyChanged("FontFamilies");
			}
		}

		public string SelectedFont
		{
			get
			{
				return this.selectedFont;
			}
			set
			{
				if (value != this.selectedFont)
				{
					this.selectedFont = value;
					this.NotifyPropertyChanged("SelectedFont");
				}
			}
		}
	}
}