using Mono.Cecil;
using System;
using System.IO;
using System.Linq;

namespace JsonIgnore.Fody
{
    public class ModuleWeaver
    {
        public Action<string> LogInfo { get; set; }
        public Action<string> LogError { get; set; }

        public ModuleDefinition ModuleDefinition { get; set; }

        public string SolutionDirectoryPath { get; set; }
        public string ProjectDirectoryPath { get; set; }

        private Tuple<bool, string> JsonPath(string root)
        {
            try
            {
                var name = "Newtonsoft.Json.*";
                var packages = Path.Combine(root, "packages");

                LogInfo($"Process | {packages}");

                var jsonDir = new DirectoryInfo(packages).GetDirectories(name).LastOrDefault();
                if (jsonDir == null)
                {
                    LogInfo($"Cannot fond | {name}");
                    return new Tuple<bool, string>(false, "");
                }

                var path = Path.Combine(jsonDir.FullName, @"lib\net45\Newtonsoft.Json.dll");
                if (!File.Exists(path))
                {
                    LogInfo($"Cannot find | {path}");
                    return new Tuple<bool, string>(false, "");
                }
                return new Tuple<bool, string>(true, path);
            }
            catch (Exception ex)
            {
                LogInfo($">> Error | {ex.Message}");
                return new Tuple<bool, string>(false, "");
            }
        }

        public void Execute()
        {
            AddAttribute(ModuleDefinition);
        }

        private string GetDllPath()
        {
            var solution = JsonPath(SolutionDirectoryPath);
            if (solution.Item1) return solution.Item2;

            var project = JsonPath(ProjectDirectoryPath);
            if (project.Item1) return project.Item2;

            var projectParent = JsonPath(new DirectoryInfo(ProjectDirectoryPath).Parent.FullName);
            if (projectParent.Item1) return projectParent.Item2;

            return string.Empty;
        }

        public void AddAttribute(ModuleDefinition module)
        {
            var dllPath = GetDllPath();
            if (dllPath == String.Empty)
            {
                LogError("|| Cannot find | Newtonsoft.Json.dll");
                return;
            }

            var json = AssemblyDefinition.ReadAssembly(dllPath);
            var ignoreName = "Newtonsoft.Json.JsonIgnoreAttribute";
            var attr = json.MainModule.GetType(ignoreName);
            var ctor = attr.Methods.First(x => x.IsConstructor);
            var ctorReference = module.ImportReference(ctor);

            var exlucdes = new[] {
                 "System.Int32",
                 "System.String",
                 "System.DateTime"
            }.ToList();


            module.Types.ToList().ForEach(type =>
            {
                var targetProperties = type.Properties.Where(p => p.Name.StartsWith("Q") && Char.IsUpper(p.Name.ElementAt(1)));
                targetProperties.ToList().ForEach(property =>
                {
                    var exist = property.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == ignoreName);
                    if (exist == null)
                    {
                        var fullName = property.PropertyType.FullName;
                        if (!exlucdes.Contains(fullName))
                        {
                            property.CustomAttributes.Add(new CustomAttribute(ctorReference));
                        }
                    }
                });
            });
        }
    }
}
