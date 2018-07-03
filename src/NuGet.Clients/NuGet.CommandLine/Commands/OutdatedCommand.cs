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

namespace NuGet.CommandLine
{
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

        [Option(typeof(NuGetCommand), "OutdatedCommandNoConstraintsDescription", AltName = "no-constraints")]
        public bool NoConstraints { get; set; }

        public override async Task ExecuteCommandAsync()
        {
            var outdatedCommandRunner = new OutdatedCommandRunner();

            var list = new OutdatedArgs(Arguments,
                Settings,
                Console,
                Prerelease,
                Deprecated,
                Patch,
                Transitive,
                NoConstraints,
                CancellationToken.None);

            await outdatedCommandRunner.ExecuteCommand(list);
        }
    }
}
