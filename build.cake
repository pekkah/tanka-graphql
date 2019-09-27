#tool nuget:?package=GitVersion.CommandLine&version=4.0.0
#addin nuget:?package=Cake.Npm&version=0.16.0

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");
var artifactsDir = Directory(Argument<string>("artifactsDir", "./artifacts"));
var publishDir = Directory(Argument<string>("publishDir", "./publish"));
var runtime = Argument<string>("runtime", "win-x64");
var sln = Argument<string>("sln", "./tanka-graphql.sln");
var fullBenchmarks = Argument<bool>("fullBenchmarks", false);

var netstandard20 = "netstandard2.0";
var netcoreapp21 = "netcoreapp3.0";
var projectFiles = GetFiles("./src/**/*.csproj")
	.Select(f => f.FullPath);

var packageFolders = GetFiles("./src/*/package.json")
                .Select(f => f.GetDirectory().FullPath);

var version = "0.0.0-dev";
var preRelease = true;
var isMasterOrTag = false;

Task("Default")
  .IsDependentOn("SetVersion")
  .IsDependentOn("Pack");

Task("Publish")
  .IsDependentOn("Build")
  .Does(()=>
  {
      var settings = new DotNetCorePublishSettings
      {
          //Framework = framework,
          Configuration = configuration,
          OutputDirectory = publishDir,
          Runtime = runtime
      };

      foreach(var projectFile in projectFiles)
      {
        DotNetCorePublish(projectFile, settings);
      }
  });

Task("Pack")
  .IsDependentOn("Build")
  .IsDependentOn("Test")
  .Does(()=>
  {
      Information($"Pack to: {artifactsDir}");
      var buildSettings = new DotNetCoreMSBuildSettings();
      buildSettings.SetVersion(version);
      var settings = new DotNetCorePackSettings
      {
          Configuration = configuration,
          OutputDirectory = artifactsDir,
          IncludeSymbols = true,
          MSBuildSettings = buildSettings
      };

      foreach(var projectFile in projectFiles)
      {
        DotNetCorePack(projectFile, settings);
      }

      foreach(var packageFolder in packageFolders)
      {
        Information($"NPM version {packageFolder}");
        var args = ProcessArgumentBuilder.FromString($"-Command npm --no-git-tag-version --allow-same-version version {version}");
        var exitCode = StartProcess(
          "powershell",
          new ProcessSettings() {
            Arguments = args,
            WorkingDirectory = packageFolder
          }
        );

        if (exitCode != 0)
        {
          throw new Exception($"NPM version failed for {packageFolder} with version {version}");
        }

        Information($"NPM pack {packageFolder}");
        var npmSettings = new NpmPackSettings();
        npmSettings.LogLevel = NpmLogLevel.Info;
        npmSettings.Source = packageFolder;
        npmSettings.WorkingDirectory = artifactsDir;
        NpmPack(npmSettings);
      }
  });

Task("Build")
  .IsDependentOn("Clean")
  .IsDependentOn("Restore")
  .Does(() =>
  {
      var settings = new DotNetCoreBuildSettings
      {
          //Framework = framework,
          Configuration = configuration
      };

      foreach(var projectFile in projectFiles)
      {
        DotNetCoreBuild(projectFile, settings);
      }

      foreach(var packageFolder in packageFolders)
      {
        var npmSettings = new NpmRunScriptSettings();
        npmSettings.ScriptName = "build";
        npmSettings.LogLevel = NpmLogLevel.Info;
        npmSettings.WorkingDirectory = packageFolder;

        Information($"NPM run build: {packageFolder}");
        NpmRunScript(npmSettings);
      }
  });

Task("Clean")
  .Does(()=>
  {
      Information($"Cleaning: {artifactsDir}");
      CleanDirectory(artifactsDir);
      Information($"Cleaning: {publishDir}");
      CleanDirectory(publishDir);
  });

Task("Restore")
  .Does(()=>
  {
      foreach(var projectFile in projectFiles)
      {
        DotNetCoreRestore(projectFile);
      }

      foreach(var packageFolder in packageFolders)
      {
        var settings = new NpmInstallSettings();
        settings.LogLevel = NpmLogLevel.Info;
        settings.WorkingDirectory = packageFolder;
        settings.Production = false;

        Information($"NPM install: {packageFolder}");
        NpmInstall(settings);
      }
  });

Task("SetVersion")
    .Does(()=> {
        var result = GitVersion(new GitVersionSettings() 
        {
          ArgumentCustomization = args => args.Append("/verbosity debug"),
          LogFilePath = "gitversion.log"
        });
        
        version = result.SemVer;
        preRelease = result.PreReleaseNumber.HasValue;
        isMasterOrTag = result.BranchName.Contains("master") || result.BranchName.Contains("tags");
        Information($"Branch: {result.BranchName}\nVersion: {version}\nFullSemVer: {result.FullSemVer}\nPreRelease: {preRelease}\nisMasterOrTag: {isMasterOrTag}");
        Information($"##vso[build.updatebuildnumber]{version}");
    });

Task("Test")
  .IsDependentOn("Build")
  .Does(()=> {
      var projectFiles = GetFiles("./tests/**/*tests.csproj")
	  .Concat(GetFiles("./tutorials/**/*.csproj"));
      var settings = new DotNetCoreTestSettings()
      {
         ResultsDirectory = new DirectoryPath(artifactsDir),
         Logger = "trx"
      };

      foreach(var file in projectFiles)
      {
          DotNetCoreTest(file.FullPath, settings);
      }
    });

Task("Benchmarks")
  .IsDependentOn("SetVersion")
  .Does(()=> {
	  var projectFiles = GetFiles("./benchmarks/**/*Benchmarks.csproj");

	  foreach(var benchmark in projectFiles)
	  {
		  var args = ProcessArgumentBuilder.FromString(
        $"run --project {benchmark} --configuration release --framework netcoreapp30 -- -i -m");

      if (isMasterOrTag || fullBenchmarks)
        args.Append("--filter *");
      else
        args.Append("--filter * --job short");

      var exitCode = StartProcess(
        "dotnet",
        new ProcessSettings() {
        Arguments = args
        }
      );

      if (exitCode != 0)
      {
        throw new Exception($"Failed to run benchmarks");
      }
    }
   });

Task("Docs")
.IsDependentOn("SetVersion")
.Does(()=> {
    Information("Generate docs");
    var targetFolder = $"{artifactsDir}\\gh-pages";
    var basepath = "/tanka-graphql/";
    if (preRelease)
    {
        targetFolder += "\\beta";
        basepath += "beta/";
    }

     var args = ProcessArgumentBuilder.FromString(
         $"--output=\"{targetFolder}\" "
       + $"--basepath=\"{basepath}\"");
			
      var exitCode = StartProcess(
			  "generate-docs",
			  new ProcessSettings() {
				  Arguments = args
			  }
			);

			if (exitCode != 0)
			{
			  throw new Exception($"Failed to generate-docs");
			}
});

RunTarget(target);
