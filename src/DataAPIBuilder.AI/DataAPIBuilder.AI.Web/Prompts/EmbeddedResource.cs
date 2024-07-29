using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAPIBuilder.AI.Resources
{
    internal static class EmbeddedResource
    {
        private static readonly string? s_namespace = typeof(EmbeddedResource).Namespace;

        internal static string Read(string fileName)
        {
            // Get the current assembly. Note: this class is in the same assembly where the embedded resources are stored.
            Assembly? assembly = typeof(EmbeddedResource).GetTypeInfo().Assembly ?? throw new ConfigurationException($"[{s_namespace}] {fileName} assembly not found");

            // Resources are mapped like types, using the namespace and appending "." (dot) and the file name
            var resourceName = $"{s_namespace}." + fileName;
            using Stream? resource = assembly.GetManifestResourceStream(resourceName) ?? throw new ConfigurationException($"{resourceName} resource not found");

            // Return the resource content, in text format.
            using var reader = new StreamReader(resource);
            return reader.ReadToEnd();
        }

        internal static Stream? ReadStream(string fileName)
        {
            // Get the current assembly. Note: this class is in the same assembly where the embedded resources are stored.
            Assembly? assembly = typeof(EmbeddedResource).GetTypeInfo().Assembly ?? throw new ConfigurationException($"[{s_namespace}] {fileName} assembly not found");

            // Resources are mapped like types, using the namespace and appending "." (dot) and the file name
            var resourceName = $"{s_namespace}." + fileName;
            return assembly.GetManifestResourceStream(resourceName);
        }
    }

    public class ConfigurationException : Exception
    {
        public ConfigurationException()
        {
        }

        public ConfigurationException(string message) : base(message)
        {
        }

        public ConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
