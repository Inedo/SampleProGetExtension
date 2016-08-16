using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Inedo.Documentation;
using Inedo.ProGet.Extensibility.PackageStores;
using Inedo.Serialization;

/*
 * This package store has the following behavior:
 * 
 * Store packages in a configurable root path in a similar fashion as the built in default package store.
 */

namespace SampleProGetExtension.PackageStores
{
    // For finer-grained control, the PackageStore class can be inherited directly instead of CommonIndexedPackageStore,
    // but this is generally not recommended as it adds a great deal of complexity.
    // For Docker feed support, the IDockerPackageStore interface must be implemented.
    [DisplayName("Sample Package Store")]
    [Description("A package store backed by a directory on disk similar to the default package store.")]
    public sealed class SamplePackageStore : CommonIndexedPackageStore
    {
        [Required]
        [Persistent]
        [DisplayName("Root path")]
        public string RootPath { get; set; }

        protected override Task<Stream> CreateAsync(string path) => Task.FromResult<Stream>(File.Create(path));

        protected override Task DeleteAsync(string path)
        {
            File.Delete(path);
            return Task.FromResult<object>(null);
        }

        protected override Task<IEnumerable<string>> EnumerateFilesAsync(string extension)
        {
            return Task.FromResult(
                from p in Directory.EnumerateFiles(this.RootPath, "*" + extension, SearchOption.AllDirectories)
                where p.EndsWith(extension)
                select p
            );
        }

        protected override string GetFullPackagePath(PackageStorePackageId packageId) => Path.Combine(this.RootPath, this.GetRelativePackagePath(packageId, Path.DirectorySeparatorChar));

        protected override Task<Stream> OpenReadAsync(string path)
        {
            try
            {
                return Task.FromResult<Stream>(File.OpenRead(path));
            }
            catch (FileNotFoundException)
            {
            }
            catch (DirectoryNotFoundException)
            {
            }

            return Task.FromResult<Stream>(null);
        }

        protected override Task RenameAsync(string originalName, string newName)
        {
            File.Move(originalName, newName);
            return Task.FromResult<object>(null);
        }
    }
}
