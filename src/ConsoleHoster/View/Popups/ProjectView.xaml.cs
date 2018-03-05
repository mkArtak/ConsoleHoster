//-----------------------------------------------------------------------
// <copyright file="ProjectView.xaml.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Windows;
using ConsoleHoster.ViewModel;

namespace ConsoleHoster.View.Popups
{
	/// <summary>
	/// Interaction logic for ProjectView.xaml
	/// </summary>
	public partial class ProjectView : Window
	{
		public ProjectView()
		{
			InitializeComponent();
		}

		public ProjectView(ProjectDetailsViewModel argVM)
			: this()
		{
			this.ViewModel = argVM;
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			if (this.ViewModel.IsValid)
			{
				this.DialogResult = true;
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		public ProjectDetailsViewModel ViewModel
		{
			get
			{
				return this.DataContext as ProjectDetailsViewModel;
			}
			private set
			{
				this.DataContext = value;
			}
		}
	}
}