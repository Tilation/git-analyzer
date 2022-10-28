using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GitAnalyzer.Modules
{
    public static class ModuleLoader
    {
        public static readonly string ModulesFolder = "modules";

        public static BaseModule[] GetInternalModules()
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(x => x.BaseType == typeof(BaseModule))
                .Select(x => (BaseModule)Activator.CreateInstance(x))
                .OrderBy(x => x.ModuleName).ToArray();
        }

        public static BaseModule[] GetExternalModules()
        {
            List<BaseModule> modules = new List<BaseModule>();
            if (!Directory.Exists(ModulesFolder)) Directory.CreateDirectory(ModulesFolder);
            var dlls = Directory.EnumerateFiles(Path.Combine(Environment.CurrentDirectory,ModulesFolder), "*.dll").ToArray();
            foreach(var dll in dlls)
            {
                try
                {
                    var assembly = Assembly.LoadFile(dll);
                    if (assembly == null) continue;
                    var types = assembly.GetTypes();
                    var mods = types.Where(x => x.BaseType == typeof(BaseModule))
                        .Select(x=>(BaseModule)Activator.CreateInstance(x)).ToArray();
                    modules.AddRange(mods);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Exception:\n{ex.Message}\nSource:\n{ex.Source}\nInner:\n{ex.InnerException}");
                }
            }
            return modules.OrderBy(x=>x.ModuleName).ToArray();
        }
    }
}
