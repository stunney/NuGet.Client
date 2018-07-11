extern alias CoreV2;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuGet.Commands;
using NuGet.ProjectModel;

namespace NuGet.CommandLine
{
    public struct PRProjectData
    {
        public string _projectPath;
        public string _configOrAssetsPath;
        public bool _config;
    }

    [Command(typeof(NuGetCommand), "outdated", "OutdatedCommandDescription", UsageSummary = "<packages.config|solution|project>",
        UsageExampleResourceName = "OutdatedCommandUsageExamples")]
    public class OutdatedCommand : Command
    {
        [Option(typeof(NuGetCommand), "OutdatedCommandPrereleaseDescription", AltName = "include-prerelease")]
        public bool Prerelease { get; set; }

        [Option(typeof(NuGetCommand), "OutdatedCommandDeprecatedDescription", AltName = "show-deprecated")]
        public bool Deprecated { get; set; }

        [Option(typeof(NuGetCommand), "OutdatedCommandLatestPatchDescription", AltName = "show-latest-patch")]
        public bool Patch { get; set; }

        [Option(typeof(NuGetCommand), "OutdatedCommandTransitiveDescription", AltName = "include-transitive")]
        public bool Transitive { get; set; }

        [Option(typeof(NuGetCommand), "OutdatedCommandAllDescription", AltName = "all")]
        public bool All { get; set; }

        public override async Task ExecuteCommandAsync()
        {
            var sourceRepositoryProvider = new CommandLineSourceRepositoryProvider(SourceProvider);
            var msBuildDir = MsBuildUtility.GetMsBuildDirectory(null, Console);

            var possibleSolutionFiles = Directory.GetFiles(
                    Directory.GetCurrentDirectory(), "*.sln", SearchOption.TopDirectoryOnly);

            var possibleProjects = new List<string>();
            if (possibleSolutionFiles.Length == 1)
            {
                var solutionPath = possibleSolutionFiles.FirstOrDefault();
                possibleProjects = GetProjectsFromSolution(solutionPath, msBuildDir);
                
            }
            else if (possibleSolutionFiles.Length > 1)
            {
                //Err ambigious
                return;
            }
            else if (possibleSolutionFiles.Length == 0)
            {
                //search for proj files
                possibleProjects = Directory.GetFiles(
                    Directory.GetCurrentDirectory(), "*.*proj", SearchOption.TopDirectoryOnly)
                    .Where(path => !path.EndsWith(".xproj", StringComparison.OrdinalIgnoreCase))
                    .ToList();

            }

            var projectsData = GetAssetsFiles(possibleProjects, msBuildDir);

            var outdatedCommandRunner = new OutdatedCommandRunner();

            var list = new OutdatedArgs(Arguments,
                Settings,
                Console,
                sourceRepositoryProvider.PackageSourceProvider,
                Prerelease,
                Deprecated,
                Patch,
                Transitive,
                All,
                CancellationToken.None);

            await outdatedCommandRunner.ExecuteCommandAsync(list, possibleProjects);
        }

        public List<PRProjectData> GetAssetsFiles(List<string> projects, string msBuildDir)
        {
            Debugger.Launch();
           
            var result = new List<PRProjectData>();

            var projectContext = new ConsoleProjectContext(Console);

            foreach (var projectPath in projects)
            {

                MSBuildNuGetProject project;
                try
                {
                    var projectSystem = new MSBuildProjectSystem(msBuildDir, projectPath, projectContext);
                    project = new MSBuildNuGetProject(projectSystem, projectSystem.ProjectFullPath, projectSystem.ProjectFullPath);
                }
                catch (Exception) {
                    //If we failed to parse a project it probably doesn't have the right properties, so just ignore it
                    continue;
                }


                if (!project.DoesPackagesConfigExists())
                {
                    var assetsDirectory = project.ProjectSystem.GetPropertyValue("MSBuildProjectExtensionsPath");
                    var assetsPath = Path.Combine(assetsDirectory + LockFileFormat.AssetsFileName);
                    if (!File.Exists(assetsPath))
                    {
                        //Throw that neither config nor assets were found
                        continue;
                    }
                    result.Add(new PRProjectData { _projectPath = projectPath, _configOrAssetsPath = assetsPath, _config = false });
                }
                else
                {
                    var configPath = project.PackagesConfigNuGetProject.PackagesConfigPath;
                    result.Add(new PRProjectData { _projectPath = projectPath, _configOrAssetsPath = configPath, _config = true });
                    //TODO: Check if it's config
                }


            }
            return result;
            
        }

        public List<string> GetProjectsFromSolution(string solutionPath, string msBuildDir)
        {
            var projects = MsBuildUtility.GetAllProjectFileNames(solutionPath, msBuildDir).ToList();
            return projects;
        }
    }
}
