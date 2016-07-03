using Mono.Cecil;
using System.IO;
using Xunit;

namespace JsonIgnore.Fody.Tests
{
    public class AddAttributeSpec
    {
        [Fact]
        public void ShouldInjectAttribute()
        {
            //var dll = Assembly.GetExecutingAssembly().Location;
            var dll = "AddAttribute.Fody.Tests";
            var dir = new FileInfo(dll).Directory.FullName;
            var target = Path.Combine(dir, "AssemblyToProcess.dll");
            var output = Path.Combine(dir, "AssemblyToProcess.Output.dll");

            var moduleDefinition = ModuleDefinition.ReadModule(target);
            var weaver = new ModuleWeaver {
                ModuleDefinition = moduleDefinition,
                DllPath = "Newtonsoft.Json.dll"
            };
            weaver.Execute();
            moduleDefinition.Write(output);
        }
    }
}
