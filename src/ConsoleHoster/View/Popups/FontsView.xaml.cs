//-----------------------------------------------------------------------
// <copyright file="FontsView.xaml.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System.ComponentModel;
using System.Windows;
using ConsoleHoster.ViewModel;

namespace ConsoleHoster.View.Popups
{
	/// <summary>
	/// Interaction logic for FontsView.xaml
	/// </summary>
	public partial class FontsView : Window
	{
		public FontsView()
		{
			InitializeComponent();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		internal FontsViewModel ViewModel
		{
			get
			{
				return this.DataContext as FontsViewModel;
			}
		}
	}
}