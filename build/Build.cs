using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("ApiKey for the specified source.")]
    readonly string ApiKey;

    [Parameter("Source url for the nuget/myget repository")]
    readonly string Source = @"https://www.myget.org/F/ajaganathan/api/v2/package";

    [Solution]
    readonly Solution Solution;

    [GitVersion]
    readonly GitVersion GitVersion;

    string version = "1.1.6";

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ProjectDirectory => SourceDirectory / "RouteServiceIwaWcfInterceptor";
    AbsolutePath ProjectFile => ProjectDirectory / "RouteServiceIwaWcfInterceptor.csproj";
    AbsolutePath NuspecFile => ProjectDirectory / "RouteServiceIwaWcfInterceptor.nuspec";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            MSBuild(s => s
                .SetTargetPath(Solution)
                .SetTargets("Restore"));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            MSBuild(s => s
                .SetTargetPath(Solution)
                .SetTargets("Rebuild")
                .SetConfiguration(Configuration)
                .SetAssemblyVersion($"{version}.0")
                .SetFileVersion($"{version}.0")
                .SetInformationalVersion($"{version}.0")
                .SetMaxCpuCount(Environment.ProcessorCount)
                .SetNodeReuse(IsLocalBuild));
        });

    Target Pack => _ => _
    .DependsOn(Compile)
    .Executes(() =>
    {
        RunProcess("nuget.exe", $"pack {ProjectFile} -Version {version}-beta -OutputDirectory {ArtifactsDirectory} -Properties Configuration={Configuration}");
    });

    Target Push => _ => _
    .DependsOn(Pack)
    .Requires(() => Source)
    .Requires(() => Configuration == "Release")
    .Requires(() => !string.IsNullOrWhiteSpace(ApiKey) ||  Path.IsPathRooted(Source))
    .Executes(() =>
    {
        GlobFiles(ArtifactsDirectory, "*.nupkg").NotEmpty()
            .Where(x => !x.EndsWith(".symbols.nupkg"))
            .ForEach(x =>
            {
                if (Path.IsPathRooted(Source))
                {
                    RunProcess("nuget.exe", $"add {Path.Combine(ArtifactsDirectory, x)} -Source {Source}");
                }
                else
                {
                    DotNetTasks.DotNetNuGetPush(s => s
                        .SetApiKey(ApiKey)
                        .SetTargetPath(x)
                        .SetSource(Source));
                }
            });
    });

    private void RunProcess(string processFullName, string argument)
    {
        var startInfo = new ProcessStartInfo(processFullName)
        {
            Arguments = argument,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        var process = Process.Start(startInfo);
        var result = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        if (!string.IsNullOrWhiteSpace(error))
            throw new Exception(error);

        Console.Write(result);

        process.WaitForExit();
    }
}
