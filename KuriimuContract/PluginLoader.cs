using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace KuriimuContract
{
	public static class PluginLoader<T>
	{
		public static IEnumerable<T> LoadPlugins(string path, string filter = "*.dll")
		{
			string[] dllFileNames = null;

			if (Directory.Exists(path))
			{
				dllFileNames = Directory.GetFiles(path, filter);

				ICollection<Assembly> assemblies = new List<Assembly>(dllFileNames.Length);
				foreach (string dllFile in dllFileNames)
				{
					AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
					Assembly assembly = Assembly.Load(an);
					assemblies.Add(assembly);
				}

				Type pluginType = typeof(T);
				ICollection<Type> pluginTypes = new List<Type>();
				foreach (Assembly assembly in assemblies)
				{
					if (assembly != null)
					{
						Type[] types = assembly.GetTypes();

						foreach (Type type in types)
						{
							if (type.IsInterface || type.IsAbstract)
							{
								continue;
							}
							else
							{
								if (type.GetInterface(pluginType.FullName) != null)
								{
									pluginTypes.Add(type);
									Console.WriteLine("Loaded " + assembly.FullName);
								}
							}
						}
					}
				}

				ICollection<T> plugins = new List<T>(pluginTypes.Count);
				foreach (Type type in pluginTypes)
				{
					T plugin = (T)Activator.CreateInstance(type);
					plugins.Add(plugin);
				}

				return plugins;
			}

			return null;
		}
	}
}