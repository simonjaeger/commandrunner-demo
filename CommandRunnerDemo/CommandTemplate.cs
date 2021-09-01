using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace CommandRunnerDemo
{
    public static class CommandTemplate
    {
        private const string DefaultTemplateFilePath = "DefaultTemplate.sh";
        private const string DeliveryOptimizationTemplateFilePath = "DeliveryOptimizationTemplate.sh";

        public static string DefaultTemplate { get; }
        public static string DeliveryOptimizationTemplate { get; }

        static CommandTemplate()
        {
            DefaultTemplate = LoadTemplateFile(DefaultTemplateFilePath);
            DeliveryOptimizationTemplate = LoadTemplateFile(DeliveryOptimizationTemplateFilePath);
        }

        /// <summary>
        /// Load a template file and process it to minimize its size.
        /// </summary>
        private static string LoadTemplateFile(string templateFilePath)
        {
            if (!File.Exists(templateFilePath))
            {
                Log.Error("Missing template file.");
            }

            // Load and minimize the content as much as possible by removing
            // comments and whitespace.
            return File.ReadAllLines(templateFilePath)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !x.StartsWith("#") || x.StartsWith("#!"))
                .Select(x => x.Trim())
                .Aggregate((s1, s2) => $"{s1}\n{s2}");
        }

        /// <summary>
        /// Create a command that uses the delivery optimization agent to download a file 
        /// and then start it.
        /// </summary>
        public static string Create(string commandId, string fileName, Uri uri, bool useDeliveryOptimization = false)
        {
            var template = useDeliveryOptimization ? DeliveryOptimizationTemplate : DefaultTemplate;
            var content = template
                .Replace("[DOWNLOAD_FILE_NAME]", fileName)
                .Replace("[URI]", uri.AbsoluteUri);
            var encodedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(content));
            return $"CID='{commandId}'; echo '{encodedContent}' | base64 --decode > /tmp/$CID; chmod +x /tmp/$CID; /tmp/$CID";
        }
    }
}
