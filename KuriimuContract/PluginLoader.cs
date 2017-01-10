using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace KuriimuContract
{
	public static class PluginLoader<T>
	{
		public static IEnumerable<T> LoadPlugins(string path, string filter = "*.dll")
		{
			if (!Directory.Exists(path))
			{
				return null;
			}

			var instances = (from dllFile in Directory.GetFiles(path, filter)
							 let an = AssemblyName.GetAssemblyName(dllFile)
							 let assembly = Assembly.Load(an)
							 where assembly != null
							 from type in assembly.GetTypes()
							 where !type.IsInterface
							 where !type.IsAbstract
							 where type.GetInterface(typeof(T).FullName) != null
							 select new { assembly.FullName, Instance = (T)Activator.CreateInstance(type) })
							 .ToList();

			foreach (var item in instances)
			{
				Console.WriteLine("Loaded " + item.FullName);
			}

			return instances.Select(inst => inst.Instance);
		}
	}
}