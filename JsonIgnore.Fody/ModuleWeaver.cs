using Mono.Cecil;
using System;
using System.Linq;

namespace JsonIgnore.Fody
{
    public class ModuleWeaver
    {
        // Will log an informational message to MSBuild
        public Action<string> LogInfo { get; set; }

        // An instance of Mono.Cecil.ModuleDefinition for processing
        public ModuleDefinition ModuleDefinition { get; set; }

        public void Execute()
        {
            var ts = ModuleDefinition.TypeSystem;
            var newType = new TypeDefinition(null, "Hello", TypeAttributes.Public, ts.Object);
            ModuleDefinition.Types.Add(newType);

            AddAttribute(ModuleDefinition);
        }

        public void AddAttribute(ModuleDefinition module)
        {
            var json = AssemblyDefinition.ReadAssembly("Newtonsoft.Json.dll"); 
            var attr = json.MainModule.GetType("Newtonsoft.Json.JsonIgnoreAttribute");
            var ctor = attr.Methods.First(x => x.IsConstructor);
            var ctorReference = module.ImportReference(ctor);

            module.Types.ToList().ForEach(type => {
                var targetProperites = type.Properties.Where(p => p.Name.StartsWith("Q"));
                targetProperites.ToList().ForEach(property => {
                    property.CustomAttributes.Add(new CustomAttribute(ctorReference));
                });
            });
        }
    }
}
