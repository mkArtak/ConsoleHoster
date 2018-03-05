//-----------------------------------------------------------------------
// <copyright file="HelpView.xaml.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace ConsoleHoster.View.Popups
{
	/// <summary>
	/// Interaction logic for HelpView.xaml
	/// </summary>
	public partial class AboutView : Window
	{
		public AboutView()
		{
			InitializeComponent();
		}

		private void Hyperlink_RequestNavigate_1(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			e.Handled = true;
			try
			{
				Process.Start("http://consolehoster.codeplex.com/");
			}
			catch
			{
			}
		}
	}
}