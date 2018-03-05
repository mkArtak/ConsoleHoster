//-----------------------------------------------------------------------
// <copyright file="PluginManager.cs" author="Artak Mkrtchyan">
//     
// </copyright>
// <author>Artak Mkrtchyan</author>

// <date>15/07/2012</date>
//-----------------------------------------------------------------------

using ConsoleHoster.Common.Plugins;
using ConsoleHoster.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;

namespace ConsoleHoster.Model.Plugins
{
	internal class PluginManager
	{
		private const string PluginFolder = "./Plugins";

		private IEnumerable<PluginDetails> availablePlugins;
		private readonly IDictionary<AppDomain, IPlugin> loadedPlugins = new Dictionary<AppDomain, IPlugin>();

		public PluginManager()
		{

		}

		public void Initalize()
		{
			this.availablePlugins = StorageManager.LoadPlugins();
		}

		public IPlugin LoadPlugin(string argPluginName)
		{
			IPlugin tmpResult = this.loadedPlugins.Where(item => item.Value.Name.Equals(argPluginName, StringComparison.InvariantCultureIgnoreCase)).Select(item => item.Value).SingleOrDefault();

			if (tmpResult == null)
			{
				PluginDetails tmpDetails = this.GetPluginDetails(argPluginName, true);
				tmpResult = this.CreatePluginInAppDomain(tmpDetails);
				tmpResult.Load();
			}

			return tmpResult;
		}

		private PluginDetails GetPluginDetails(string argPluginName, bool argThrowIfNotFound)
		{
			PluginDetails tmpResult = availablePlugins.FirstOrDefault(item => item.Name == argPluginName);
			if (argThrowIfNotFound && tmpResult == null)
			{
				throw new ArgumentException(String.Format("Unable to find plugin with the specified name: {0}", argPluginName));
			}

			return tmpResult;
		}

		private IPlugin CreatePluginInAppDomain(PluginDetails argDetails)
		{
			IPlugin tmpResult = null;
			AppDomain tmpPluginDomain = AppDomain.CreateDomain(String.Format("Plugin{0}", argDetails.Name));
			tmpResult = tmpPluginDomain.CreateInstanceAndUnwrap(argDetails.AssemblyFullName, argDetails.PluginTypeName) as IPlugin;
			this.loadedPlugins.Add(new KeyValuePair<AppDomain, IPlugin>(tmpPluginDomain, tmpResult));

			return tmpResult;
		}

		internal IEnumerable<PluginDetails> AvailablePlugins
		{
			get
			{
				return this.availablePlugins;
			}
		}
	}
}
