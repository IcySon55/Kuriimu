using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Kuriimu.Contract
{
    public static class PluginLoader<T>
    {
        public static IEnumerable<T> LoadPlugins(string pluginPath, string filter = "*.dll")
        {
            string path = Path.Combine(Application.StartupPath, pluginPath);
            string[] dllFileNames = null;
            ICollection<T> plugins = new List<T>();

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
                        try
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
                                    }
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                }

                foreach (Type type in pluginTypes)
                {
                    T plugin = (T)Activator.CreateInstance(type);
                    plugins.Add(plugin);
                }
            }

            return plugins;
        }
    }
}