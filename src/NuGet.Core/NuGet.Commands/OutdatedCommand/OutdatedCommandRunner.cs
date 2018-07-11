// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.ProjectModel;
using NuGet.Frameworks;
using NuGet.Versioning;

public struct OutdatedPackage
{
    public string name;
    public string current;
    public string wanted;
    public string latest;
}

namespace NuGet.Commands
{

    public class OutdatedCommandRunner : IOutdatedCommandRunner
    {
        /// <summary>
        /// Executes the logic for nuget outdated command.
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteCommandAsync(OutdatedArgs outdatedArgs, IList<string> projectsPaths)

        {
            /*
             * Reading from PC 
             */
             

            /*
             * Reading from PR
             */
            Debugger.Launch();

            foreach (var path in projectsPaths)
            {
                await processProject(path, outdatedArgs);
            }
            

            //Go through the packagespec frameworks, get the names and the wanted versions
            //Use the names to get the top-level targets and get the current versions

        }

        private async Task processProject(string assetsFilePath, OutdatedArgs outdatedArgs)
        {
            var lockFileFormat = new LockFileFormat();
            var assetsFile = lockFileFormat.Read(assetsFilePath);
            var topLevelPackages = assetsFile.PackageSpec.TargetFrameworks.SelectMany(p => p.Dependencies).ToList();
            await PackagesVersions(assetsFile, topLevelPackages, outdatedArgs);
        }

        private async Task PackagesVersions(LockFile assetsFile, List<LibraryModel.LibraryDependency> topLevelPackages, OutdatedArgs outdatedArgs)
        {

            foreach (var target in assetsFile.Targets)
            {
                var framework = target.TargetFramework;
                foreach (var library in target.Libraries)
                {
                    var topLevelPackage = topLevelPackages.Where(p => p.Name.Equals(library.Name)).FirstOrDefault();

                    if (topLevelPackage != null || outdatedArgs.Transitive)
                    {
                        var currentVersion = library.Version.ToString();
                        var wantedVersion = topLevelPackage != null ? topLevelPackage.LibraryRange.VersionRange.ToString() : null;
                        await GetLatestVersion(library.Name, framework, outdatedArgs, library.Version);
                    }

                }
            }
        }

        private async Task GetLatestVersion(string packageId, NuGetFramework framework, OutdatedArgs outdatedArgs, NuGetVersion currentVersion)
        {
            var sources = outdatedArgs.SourceProvider.LoadPackageSources();
            var latestVersion = currentVersion;

            foreach (var packageSource in sources)
            {
                var sourceRepository = Repository.Factory.GetCoreV3(packageSource.Source);
                var dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>(outdatedArgs.CancellationToken);
                var packages = (await dependencyInfoResource.ResolvePackages(packageId, framework, new SourceCacheContext(), outdatedArgs.Logger, outdatedArgs.CancellationToken)).ToList();

                var latestVersionAtSource = packages.Where(package => package.Listed
                && (outdatedArgs.Prerelease || !package.Version.IsPrerelease))
                .OrderByDescending(package => package.Version, VersionComparer.Default)
                .Select(package => package.Version)
                .FirstOrDefault();

                latestVersion = latestVersionAtSource > latestVersion ? latestVersionAtSource : latestVersion;
            }


        }
    }
}