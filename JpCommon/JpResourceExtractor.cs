using System;
using System.IO;
using System.Reflection;

namespace JpCommon
{
    public class JpResourceExtractor
    {
        public JpResourceExtractor()
        {
        }

        public static void ExtractEmbeddedResource(Assembly assembly, string resourceName, string extractPath)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException("Resource name cannot be null or empty.", nameof(resourceName));
            if (string.IsNullOrEmpty(extractPath))
                throw new ArgumentException("Extract path cannot be null or empty.", nameof(extractPath));

            // Obtain all resource names in the assembly
            string[] resourceNames = assembly.GetManifestResourceNames();
            string? matchingResourceName = Array.Find(resourceNames, name => name.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));

            if (matchingResourceName == null)
            {
                throw new FileNotFoundException($"Resource '{resourceName}' not found in the assembly.");
            }

            // Retrieve the resource stream
            using Stream? resourceStream = assembly.GetManifestResourceStream(matchingResourceName);
            if (resourceStream is null)
            {
                throw new InvalidOperationException($"Failed to retrieve the stream for resource '{resourceName}'.");
            }

            string filePath = Path.Combine(extractPath, resourceName);

            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            // Write the resource to a file
            using FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            resourceStream.CopyTo(fileStream);
        }
    }
}
