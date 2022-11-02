var target = Argument("target", "Pack");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory("./output");
});

Task("BuildAll")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetBuild("./Safe.sln", new DotNetBuildSettings
    {
        Configuration = configuration
    });
});

Task("BuildSafe")
    .IsDependentOn("Test")
    .Does(() =>
{
    DotNetBuild("./Safe/Safe.csproj", new DotNetBuildSettings
    {
        Configuration = configuration,
        OutputDirectory = "./output/"
    });
});

Task("Test")
    .IsDependentOn("BuildAll")
    .Does(() =>
{
    DotNetTest("./Safe.sln", new DotNetTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
    });
});


Task("Pack")
    .IsDependentOn("BuildSafe")
    .Does(() =>
{
    CleanDirectory("./package");
    Zip("./output", "./package/Safe.zip");
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);