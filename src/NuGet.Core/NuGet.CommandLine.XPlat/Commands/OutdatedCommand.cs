// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using NuGet.Commands;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.CommandLine;
using System.Linq;
using NuGet.ProjectModel;

namespace NuGet.CommandLine.XPlat
{
    internal static class OutdatedCommand
    {
        public struct PRProjectData
        {
            public string _projectPath;
            public string _configOrAssetsPath;
            public bool _config;
        }

        public static void Register(CommandLineApplication app, Func<ILogger> getLogger)
        {
            app.Command("outdated", outdated =>
            {
                outdated.Description = Strings.Outdated_Description;
                outdated.HelpOption(XPlatUtility.HelpOption);

                outdated.Option(
                    CommandConstants.ForceEnglishOutputOption,
                    Strings.ForceEnglishOutput_Description,
                    CommandOptionType.NoValue);

                var prerelease = outdated.Option(
                    "-prerelease|--include-prerelease",
                    Strings.OutdatedPrerelease_Description,
                    CommandOptionType.SingleValue);

                var deprecated = outdated.Option(
                    "-deprecated|--show-deprecated",
                    Strings.OutdatedDeprecated_Description,
                    CommandOptionType.SingleValue);

                var patch = outdated.Option(
                    "-safe|--show-latest-patch",
                    Strings.OutdatedPatch_Description,
                    CommandOptionType.SingleValue);

                var transitive = outdated.Option(
                    "-transitive|--include-transitive",
                    Strings.OutdatedTransitive_Description,
                    CommandOptionType.SingleValue);

                var all = outdated.Option(
                    "-all|--all",
                    Strings.OutdatedAll_Description,
                    CommandOptionType.SingleValue);

                outdated.OnExecute(async () =>
                {
                    Debugger.Launch();

                    var possibleSolutionFiles = Directory.GetFiles(
                   Directory.GetCurrentDirectory(), "*.sln", SearchOption.TopDirectoryOnly);

                    var possibleProjects = new List<string>();
                    if (possibleSolutionFiles.Length == 1)
                    {
                        var solutionPath = possibleSolutionFiles.FirstOrDefault();
                        possibleProjects = MSBuildAPIUtility.GetProjectsFromSolution(solutionPath);
                    }
                    else if (possibleSolutionFiles.Length > 1)
                    {
                        //Err ambigious
                        return 0;
                    }
                    else if (possibleSolutionFiles.Length == 0)
                    {
                        //search for proj files
                        possibleProjects = Directory.GetFiles(
                            Directory.GetCurrentDirectory(), "*.*proj", SearchOption.TopDirectoryOnly)
                            .Where(path => !path.EndsWith(".xproj", StringComparison.OrdinalIgnoreCase))
                            .ToList();
                    }

                    var projectsData = GetAssetsFiles(possibleProjects);

                    var logger = getLogger();
                    var settings = XPlatUtility.CreateDefaultSettings();
                    var arguments = new List<string>();

                    var sourceProvider = new PackageSourceProvider(settings);
                    var outdatedCommandRunner = new OutdatedCommandRunner();
                    
                    var list = new OutdatedArgs(arguments,
                        settings,
                        logger,
                        sourceProvider,
                        prerelease.HasValue(),
                        deprecated.HasValue(),
                        patch.HasValue(),
                        transitive.HasValue(),
                        all.HasValue(),
                        CancellationToken.None);

                    await outdatedCommandRunner.ExecuteCommandAsync(list, arguments);

                    return 0;
                });
            });
        }
        public static List<PRProjectData> GetAssetsFiles(List<string> projects)
        {
            Debugger.Launch();

            var result = new List<PRProjectData>();

            foreach (var projectPath in projects)
            {
                var project = MSBuildAPIUtility.GetProject(projectPath);
                try
                {
                    project = MSBuildAPIUtility.GetProject(projectPath);
                }
                catch (Exception)
                {
                    continue;
                }

                var assetsDirectory = project.GetPropertyValue("MSBuildProjectExtensionsPath");
                var assetsPath = Path.Combine(assetsDirectory + LockFileFormat.AssetsFileName);


                if (File.Exists(assetsPath))
                {
                    result.Add(new PRProjectData { _projectPath = projectPath, _configOrAssetsPath = assetsPath, _config = false });
                }
                else
                {
                    var configPath = Directory.GetFiles(project.DirectoryPath, "*.config", SearchOption.TopDirectoryOnly)
                    .Where(s => Path.GetFileName(s)
                    .StartsWith("packages.", StringComparison.OrdinalIgnoreCase)).ToList().FirstOrDefault();

                    result.Add(new PRProjectData { _projectPath = projectPath, _configOrAssetsPath = configPath, _config = true });
                    //TODO: Check if it's config
                }


            }
            return result;

        }


    }
}