//-----------------------------------------------------------------------
// <copyright file="IPlugin.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleHoster.Common.ViewModel;

namespace ConsoleHoster.Common.Plugins
{
	/// <summary>
	/// Defines a contract which the plugins should implement
	/// </summary>
	public interface IPlugin
	{
		/// <summary>
		/// Loads the implementing plugin
		/// </summary>
		void Load();

		/// <summary>
		/// Unloads the implementing plugin
		/// </summary>
		void Unload();

		/// <summary>
		/// Activates the implementing plugin, which was already loaded
		/// </summary>
		void Activate(IConsolePluginApi argConsole);

		/// <summary>
		/// Deactivates the implementing plugin, which was already activated
		/// </summary>
		void Deactivate(IConsolePluginApi argConsole);

		/// <summary>
		/// Notification method used to inform the plugin about curent folder change.
		/// </summary>
		/// <param name="argFolder">The new folder path</param>
		void OnFolderChanged(IConsolePluginApi argConsole);

		/// <summary>
		/// Gets the unique name identifier of the Plugin
		/// </summary>
		string Name
		{
			get;
		}
	}
}
