// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using CmdPalHaExtension.Helpers;
using CmdPalHaExtension.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalHaExtension.Pages;

internal sealed partial class HaEntityDomainFilters : Filters
{
    public override IFilterItem[] GetFilters()
    {
        var domains = EntityCache.Entities
            .Select(e => e.GetDomain())
            .Where(EntityCommandFactory.ControllableDomains.Contains)
            .Distinct()
            .OrderBy(d => d)
            .Select(d => new Filter()
            {
                Id = d,
                Name = MdiIconProvider.GetFriendlyDomain(d),
            })
            .ToList<IFilterItem>();

        domains.Insert(0, new Filter() { Id = "all", Name = "All" });
        return domains.ToArray();
    }
}
