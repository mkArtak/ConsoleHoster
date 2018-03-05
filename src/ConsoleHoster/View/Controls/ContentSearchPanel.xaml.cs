//-----------------------------------------------------------------------
// <copyright file="ContentSearchPanel.xaml.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System.Windows.Controls;
using System.Windows.Input;
using ConsoleHoster.ViewModel;

namespace ConsoleHoster.View.Controls
{
	/// <summary>
	/// Interaction logic for ContentSearchPanel.xaml
	/// </summary>
	public partial class ContentSearchPanel : UserControl
	{
		public ContentSearchPanel()
		{
			InitializeComponent();
		}

		private void txbSearch_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				this.ViewModel.FindNextCommand.Execute(null);
			}
		}

		private SearchViewModel ViewModel
		{
			get
			{
				return this.DataContext as SearchViewModel;
			}
		}
	}
}