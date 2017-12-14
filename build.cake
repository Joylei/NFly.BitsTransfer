///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// EXTERNAL NUGET TOOLS
//////////////////////////////////////////////////////////////////////
#tool "nuget:?package=xunit.runner.console"

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////
var projectName = "NFly.BitsTransfer";
var versionPostfix = "-alpha2";
var releaseNotes = "change nuget package structure to meet nuget standard.";
var solutions = GetFiles("./**/*.sln");
var solutionPaths = solutions.Select(solution => solution.GetDirectory());

var srcDir = Directory("./src");
var buildDir = Directory("./build");
var artifactsDir = Directory("./artifacts");
var testResultsDir = artifactsDir + Directory("test-results");
var nupkgDestDir = artifactsDir + Directory("nuget-package");

var platforms = new String[]{"net35","net40", "net45"};

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup((ctx) =>
{
    Information("");
    Information(" ███████████████████ " + projectName + " █████████████████████");
    Information("");
});

Teardown((ctx) =>
{
    Information("Finished running tasks.");
});

//////////////////////////////////////////////////////////////////////
// PRIVATE TASKS
//////////////////////////////////////////////////////////////////////

Task("__Clean")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] {
        buildDir,
        artifactsDir,
        testResultsDir,
        nupkgDestDir
  	});

    foreach(var path in solutionPaths)
    {
        Information("Cleaning {0}", path);
        CleanDirectories(path + "/**/bin/" + configuration);
        CleanDirectories(path + "/**/obj/" + configuration);
    }
});

Task("__RestoreNugetPackages")
    .Does(() =>
{
    foreach(var solution in solutions)
    {
        Information("Restoring NuGet Packages for {0}", solution);
        NuGetRestore(solution);
    }
});

Task("__BuildSolutions").Does(()=>
{
  foreach(var solution in solutions)
    {
        Information("Building {0}", solution);

        MSBuild(solution, settings =>
            settings
                .SetConfiguration(configuration)
                .WithProperty("TreatWarningsAsErrors", "false")
                .UseToolVersion(MSBuildToolVersion.VS2015)
                .SetVerbosity(Verbosity.Minimal)
                .SetNodeReuse(false));
    }
});

Task("__RunTests")
  .Does(()=>
{
  Information("*testing...");
  EnsureDirectoryExists(testResultsDir);
  var testAssemblies = GetFiles("./src/**/bin/"+configuration+"/**/*.Tests.dll");
  XUnit2(testAssemblies, new XUnit2Settings {
        Parallelism = ParallelismOption.All,
        HtmlReport = true,
        NoAppDomain = true,
        OutputDirectory = testResultsDir
    });
  Information("*testing completed!");
});

Task("__CopyOutputToBuild")
  .Does(()=>
{
  foreach(var platform in platforms)
  {
      var files = GetFiles("./src/**/bin/"+configuration+"/"+ platform +"/"+ projectName +".dll");
      var destDir = buildDir + Directory("lib") + Directory(platform);
      EnsureDirectoryExists(destDir);
      CopyFiles(files, destDir);
  }
});


Task("__CreateNugetPackage")
  .Does(()=>
{
  var assemblyInfoFile = srcDir + Directory(projectName) + Directory("Properties") + File("AssemblyInfo.cs");
  var assemblyInfo = ParseAssemblyInfo(assemblyInfoFile);
  var version = assemblyInfo.AssemblyVersion;
  Information("Assembly version: {0}", version);
  
  var nuspecFile = srcDir + File(projectName + ".nuspec");
  var nuspecDestFile = buildDir + File(projectName + ".nuspec");
  CopyFile(nuspecFile, nuspecDestFile);
  
  var settings = new NuGetPackSettings{
    Id = projectName,
    Version = version + versionPostfix,
    ReleaseNotes = new String[]{ releaseNotes },
    OutputDirectory = nupkgDestDir
  };
  NuGetPack(nuspecDestFile, settings);
});

Task("Default")
  .IsDependentOn("Pack");
  
Task("Build")
  .IsDependentOn("__Clean")
  .IsDependentOn("__RestoreNugetPackages")
  .IsDependentOn("__BuildSolutions");

Task("Test")
  .IsDependentOn("Build")
  .IsDependentOn("__RunTests");

Task("Pack")
  .IsDependentOn("Build")
  .IsDependentOn("__CopyOutputToBuild")
  .IsDependentOn("__CreateNugetPackage")
  .Does(()=>
{
  Information("*packing...");
});

RunTarget(target);