using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.CompressionTasks;

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Pack);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter]
    readonly AbsolutePath Solution;

    [Parameter]
    readonly AbsolutePath MainProject;

    [Parameter]
    readonly AbsolutePath OutputPath;

    [Parameter]
    readonly AbsolutePath PackagePath;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            EnsureCleanDirectory(OutputPath);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore();
        });

    Target BuildAll => _ => _
        .DependsOn(Clean, Restore)
        .Executes(() =>
        {
            DotNetBuild(_ => _
                .SetConfiguration(Configuration)
                .SetProjectFile(Solution)
                .EnableNoRestore()
            );
        });

    Target Test => _ => _
        .DependsOn(BuildAll)
        .Executes(() =>
        {
            DotNetTest(_ => _
                .SetConfiguration(Configuration)
                .SetProjectFile(Solution)
                .EnableNoRestore()
                .EnableNoBuild()
            );
        });

    Target PublishSafe => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            DotNetPublish(_ => _
                .SetConfiguration(Configuration)
                .SetProject(MainProject)
                .SetOutput(OutputPath)
                .EnableNoRestore()
                .EnableNoBuild()
            );
        });

    Target Pack => _ => _
        .DependsOn(PublishSafe)
        .Executes(() =>
        {
            EnsureCleanDirectory(PackagePath);

            CompressZip(
                OutputPath, 
                PackagePath / "Safe.zip"
            );
        });
}
