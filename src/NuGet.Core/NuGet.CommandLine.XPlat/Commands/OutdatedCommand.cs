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

namespace NuGet.CommandLine.XPlat
{
    internal static class OutdatedCommand
    {
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

                var noConstraints = outdated.Option(
                    "-noconstraints|--no-constraints",
                    Strings.OutdatedConstraints_Description,
                    CommandOptionType.SingleValue);

                outdated.OnExecute(async () =>
                {

                    var logger = getLogger();
                    var settings = XPlatUtility.CreateDefaultSettings();
                    var arguments = new List<string>();
                    
                    var outdatedCommandRunner = new OutdatedCommandRunner();

                    var list = new OutdatedArgs(arguments,
                        settings,
                        logger,
                        prerelease.HasValue(),
                        deprecated.HasValue(),
                        patch.HasValue(),
                        transitive.HasValue(),
                        noConstraints.HasValue(),
                        CancellationToken.None);

                    await outdatedCommandRunner.ExecuteCommand(list);

                    return 0;
                });
            });
        }
    }
}