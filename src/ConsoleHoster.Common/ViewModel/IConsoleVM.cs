//-----------------------------------------------------------------------
// <copyright file="IConsoleVM.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>11/08/2012</date>
//-----------------------------------------------------------------------
using System;

namespace ConsoleHoster.Common.ViewModel
{
	public delegate void ConsoleViewModelEventHandler(IConsoleVM argSender);

	/// <summary>
	/// Represents a contract for working with a project console
	/// </summary>
	public interface IConsoleVM : IConsolePluginApi, IDisposable
	{
		/// <summary>
		/// The event will notify when the project console will be closing
		/// </summary>
		event ConsoleViewModelEventHandler Closing;

		/// <summary>
		/// Closes the console
		/// </summary>
		void Close();

		/// <summary>
		/// Gets or sets the command which is going to execute
		/// </summary>
		string CurrentCommand
		{
			get;
			set;
		}
	}
}