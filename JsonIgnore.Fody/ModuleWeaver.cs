using Mono.Cecil;
using System;
using System.IO;
using System.Linq;

namespace JsonIgnore.Fody
{
    public class ModuleWeaver
    {
        public Action<string> LogInfo { get; set; }

        public ModuleDefinition ModuleDefinition { get; set; }

        public string SolutionDirectoryPath { get; set; }

        private string JsonPath()
        {
            var packages = Path.Combine(SolutionDirectoryPath, "packages");
            var jsonDir = new DirectoryInfo(packages).GetDirectories("Newtonsoft.Json.*").LastOrDefault().FullName;

            return Path.Combine(jsonDir, @"lib\net45\Newtonsoft.Json.dll");
        }

        public void Execute()
        {
            var ts = ModuleDefinition.TypeSystem;
            var newType = new TypeDefinition(null, "Hello", TypeAttributes.Public, ts.Object);
            ModuleDefinition.Types.Add(newType);

            AddAttribute(ModuleDefinition);
        }

        public void AddAttribute(ModuleDefinition module)
        {
            var json = AssemblyDefinition.ReadAssembly(JsonPath());
            var attr = json.MainModule.GetType("Newtonsoft.Json.JsonIgnoreAttribute");
            var ctor = attr.Methods.First(x => x.IsConstructor);
            var ctorReference = module.ImportReference(ctor);

            module.Types.ToList().ForEach(type =>
            {
                var targetProperites = type.Properties.Where(p => p.Name.StartsWith("Q"));
                targetProperites.ToList().ForEach(property =>
                {
                    property.CustomAttributes.Add(new CustomAttribute(ctorReference));
                });
            });
        }
    }
}
