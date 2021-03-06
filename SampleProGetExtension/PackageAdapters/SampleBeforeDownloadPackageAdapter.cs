﻿using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using Inedo.Diagnostics;
using Inedo.IO;
using Inedo.ProGet.Extensibility.Adapters;
using Inedo.ProGet.Feeds;

/*
 * This package download adapter has the following behavior:
 * 
 * If the feed type is NuGet, Chocolatey, PowerShell, or ProGet (upack):
 *   add a file to the root of the package called "downloaded.txt" containing some generated metadata
 * For all other feed types:
 *   do nothing
 * 
 * The persisted XML for this adapter is:
 *   <SampleProGetExtension.PackageAdapters.SampleBeforeDownloadPackageAdapter Assembly="SampleProGetExtension"></SampleProGetExtension.PackageAdapters.SampleBeforeDownloadPackageAdapter>
 */

namespace SampleProGetExtension.PackageAdapters
{
    [DisplayName("Sample Before Download Package Adapter")]
    public sealed class SampleBeforeDownloadPackageAdapter : BeforeDownloadPackageAdapter
    {
        public override Stream Adapt(PackageAdapterContext packageInfo)
        {
            if (packageInfo == null)
                throw new ArgumentNullException(nameof(packageInfo));

            // this extension will only work on packages that are standard zip files
            bool isSupportedFeedType = packageInfo.Feed.FeedType == FeedType.NuGet
                || packageInfo.Feed.FeedType == FeedType.Chocolatey
                || packageInfo.Feed.FeedType == FeedType.PowerShell
                || packageInfo.Feed.FeedType == FeedType.ProGet;

            // return null to indicate that the original package should be used
            if (!isSupportedFeedType)
                return null;

            // acquire the package stream and copy it to a writable temporary stream
            Stream tempStream = null;
            try
            {
                using (var packageStream = packageInfo.GetPackageStream(true))
                {
                    tempStream = TemporaryStream.Create(packageStream.Length);
                    packageStream.CopyTo(tempStream);
                }

                // open the copied stream a a zip archive and inject the downloaded.txt file
                tempStream.Position = 0;
                using (var zip = new ZipArchive(tempStream, ZipArchiveMode.Update, true))
                {
                    var entry = zip.CreateEntry("downloaded.txt");
                    using (var entryStream = entry.Open())
                    using (var entryWriter = new StreamWriter(entryStream))
                    {
                        entryWriter.WriteLine("This file was generated by the ProGet Sample Extension.");
                        entryWriter.WriteLine();
                        entryWriter.WriteLine("Download details:");
                        entryWriter.WriteLine("  Feed: " + packageInfo.Feed.FeedName);
                        entryWriter.WriteLine("  Date: " + DateTime.Now);
                    }
                }

                tempStream.Position = 0;
                return tempStream;
            }
            catch (Exception ex)
            {
                // if there was an error, release the temporary stream and log the error to the ProGet error log
                tempStream?.Dispose();
                Logger.Log(MessageLevel.Error, $"Unhandled exception in {nameof(SampleBeforeDownloadPackageAdapter)} download adapter.", details: ex.ToString());
                return null;
            }
        }
    }
}
