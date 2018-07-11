// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using NuGet.Common;
using NuGet.Configuration;

namespace NuGet.Commands
{
    public class OutdatedArgs
    {
        public delegate void Log(int startIndex, string message);

        public IList<string> Arguments { get; }

        public ISettings Settings { get; }

        public ILogger Logger { get; }

        public IPackageSourceProvider SourceProvider { get; set; }
        public bool Prerelease { get; }

        public bool Deprecated { get; }

        public bool Patch { get; }

        public bool Transitive { get; }

        public bool All{ get; }

        public CancellationToken CancellationToken { get; }

        public OutdatedArgs(IList<string> arguments, ISettings settings, ILogger logger,
                            IPackageSourceProvider sourceProvider, bool prerelease, bool deprecated,
                            bool patch, bool transitive, bool all, CancellationToken token)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            Arguments = arguments;
            Settings = settings;
            Prerelease = prerelease;
            Logger = logger;
            SourceProvider = sourceProvider;
            Deprecated = deprecated;
            Patch = patch;
            Transitive = transitive;
            All = all;
            CancellationToken = token;
        }
    }
}