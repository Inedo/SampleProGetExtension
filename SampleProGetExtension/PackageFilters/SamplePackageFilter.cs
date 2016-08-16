using System;
using System.ComponentModel;
using Inedo.ProGet.Extensibility.PackageFilters;
using Inedo.ProGet.Feeds;

/*
 * This package filter has the following behavior:
 * 
 * Include only packages that start with "Microsoft." or "Inedo."
 * 
 * The persisted XML for this directory is:
 *   <SampleProGetExtension.PackageFilters.SamplePackageFilter Assembly="SampleProGetExtension"></SampleProGetExtension.PackageFilters.SamplePackageFilter>
 */
 
namespace SampleProGetExtension.PackageFilters
{
    [DisplayName("Sample Package Filter")]
    public sealed class SamplePackageFilter : PackageFilter
    {
        private static readonly string[] Prefixes = new[]
        {
            "Microsoft.", "Inedo."
        };

        public override bool IsPackageIncluded(IPackageIdentifier package)
        {
            // Only allow packages that start with one of the hardcoded strings
            foreach (var prefix in Prefixes)
            {
                if (package.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
