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
            // AddHello(ModuleDefinition);
            AddAttribute(ModuleDefinition);
        }

        public void AddHello(ModuleDefinition module)
        {
            var ts = ModuleDefinition.TypeSystem;
            var newType = new TypeDefinition(null, "Hello", TypeAttributes.Public, ts.Object);
            module.Types.Add(newType);
        }

        public void AddAttribute(ModuleDefinition module)
        {
            var ignoreName = "Newtonsoft.Json.JsonIgnoreAttribute";
            var json = AssemblyDefinition.ReadAssembly(JsonPath());
            var attr = json.MainModule.GetType(ignoreName);
            var ctor = attr.Methods.First(x => x.IsConstructor);
            var ctorReference = module.ImportReference(ctor);

            module.Types.ToList().ForEach(type =>
            {
                var targetProperties = type.Properties.Where(p => p.Name.StartsWith("Q"));
                targetProperties.ToList().ForEach(property =>
                {
                    var exist = property.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == ignoreName);
                    if (exist == null)
                    {
                        if (property.PropertyType.FullName != "System.Int32")
                        {
                            property.CustomAttributes.Add(new CustomAttribute(ctorReference));
                        }
                    }
                });
            });
        }
    }
}
