var target = Argument("target", "Default");
var npi = EnvironmentVariable("npi");

Task("push")
    .IsDependentOn("pack")
    .Description("Push nuget")
    .Does(() => {
        var nupkg = new DirectoryInfo("./Nuget").GetFiles("*.nupkg").LastOrDefault();
        var package = nupkg.FullName;
        NuGetPush(package, new NuGetPushSettings {
            Source = "https://www.nuget.org/api/v2/package",
            ApiKey = npi
        });
    });

Task("pack")
    .IsDependentOn("build")
    .Does(() => {
        CleanDirectory("Nuget");
        var settings = new ProcessSettings {
            Arguments = "pack JsonIgnore.Fody/JsonIgnore.Fody.csproj -OutputDirectory Nuget"
        };
        StartProcess("nuget", settings);
    });

Task("build")
    .Does(() => {

        DotNetBuild("JsonIgnore.Fody.sln", settings =>  {
            settings.SetConfiguration("Release")
                .WithTarget("Rebuild");
        });
    });

RunTarget(target);