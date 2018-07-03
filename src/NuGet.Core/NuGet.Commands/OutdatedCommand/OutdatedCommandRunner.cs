// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace NuGet.Commands
{
    public class OutdatedCommandRunner : IOutdatedCommandRunner
    {
        /// <summary>
        /// Executes the logic for nuget outdated command.
        /// </summary>
        /// <returns></returns>
        public Task ExecuteCommand(OutdatedArgs outdatedArgs)

        {
            return null;
        }
       
    }
}