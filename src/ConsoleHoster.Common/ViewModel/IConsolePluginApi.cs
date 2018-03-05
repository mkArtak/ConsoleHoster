//-----------------------------------------------------------------------
// <copyright file="IConsolePluginApi.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>18/08/2012</date>
//-----------------------------------------------------------------------

namespace ConsoleHoster.Common.ViewModel
{
	/// <summary>
	/// Represents the API of the ConsoleProjectVM to be exposed to the plugins
	/// </summary>
	public interface IConsolePluginApi
	{
		/// <summary>
		/// Runs the specified contract on the console
		/// </summary>
		/// <param name="argCommandToRun">The command to execute</param>
		void RunCommand(string argCommandToRun);

		/// <summary>
		/// The name of the console
		/// </summary>
		string Name
		{
			get;
		}

		/// <summary>
		/// Gets the current folder for the console
		/// </summary>
		string CurrentFolder
		{
			get;
		}
	}
}
