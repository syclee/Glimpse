using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Text.RegularExpressions;
using Glimpse.Core.Extensions;

namespace Glimpse.Core.Configuration
{
    public static class Glimpse
    {
        private static readonly Settings settings = new Settings();

        public static Settings Settings 
        {
            get
            {
                return settings;
            }
        }
    }
    
    public class Settings
    {
        public Settings()
        {
            ClientScripts = new DiscoverableCollectionSettings();
            Logging = new LoggingSettings();
            PipelineInspectors = new DiscoverableCollectionSettings();
            Resources = new DiscoverableCollectionSettings();

            var processor = new ConfigProcessor();
            processor.Run(ConfigurationManager.GetSection("glimpse") as GlimpseSection ?? new GlimpseSection(), this);
        }

        public DiscoverableCollectionSettings ClientScripts { get; private set; }

        public RuntimePolicy DefaultRuntimePolicy { get; set; }

        public string EndpointBaseUri { get; set; }

        public LoggingSettings Logging { get; set; }

        public DiscoverableCollectionSettings PipelineInspectors { get; private set; }

        public DiscoverableCollectionSettings Resources { get; private set; }

        public RuntimePoliciesSettings RuntimePolicies { get; set; }

        public DiscoverableCollectionSettings SerializationConverters { get; set; }

        public Type ServiceLocatorType { get; set; }

        public DiscoverableCollectionSettings Tabs { get; set; }
    }

    public class ConfigProcessor
    {
        public void Run(GlimpseSection glimpseSection, Settings settings)
        {
            this.ProcessDiscoverableCollection(settings.ClientScripts, glimpseSection.ClientScripts);  
            settings.DefaultRuntimePolicy = glimpseSection.DefaultRuntimePolicy; 
            settings.EndpointBaseUri = glimpseSection.EndpointBaseUri; 
            settings.Logging.Level = glimpseSection.Logging.Level; 
            this.ProcessDiscoverableCollection(settings.PipelineInspectors, glimpseSection.PipelineInspectors);  
            this.ProcessDiscoverableCollection(settings.Resources, glimpseSection.Resources);
            this.ProcessDiscoverableCollection(settings.RuntimePolicies, glimpseSection.RuntimePolicies);
            settings.RuntimePolicies.ContentTypes.AddRange(glimpseSection.RuntimePolicies.ContentTypes.ToEnumerable().Select(x => new ContentTypeSettings { ContentType = x }));
            settings.RuntimePolicies.StatusCodes.AddRange(glimpseSection.RuntimePolicies.StatusCodes.ToEnumerable().Select(x => new StatusCodeSettings { StatusCode = x }));
            settings.RuntimePolicies.Uris.AddRange(glimpseSection.RuntimePolicies.Uris.ToEnumerable().Select(x => new UrisSettings { Regex = x }));
            this.ProcessDiscoverableCollection(settings.SerializationConverters, glimpseSection.SerializationConverters);
            settings.ServiceLocatorType = glimpseSection.ServiceLocatorType;
            this.ProcessDiscoverableCollection(settings.Tabs, glimpseSection.Tabs); 
        }

        private void ProcessDiscoverableCollection(DiscoverableCollectionSettings settings, DiscoverableCollectionElement element)
        {
            settings.AutoDiscover = element.AutoDiscover;
            settings.DiscoveryLocation = element.DiscoveryLocation;
            settings.IgnoredTypes.AddRange(element.IgnoredTypes.ToEnumerable());
        }
    } 

    public class LoggingSettings
    {
        public LoggingLevel Level { get; set; }
    }

    public class DiscoverableCollectionSettings
    {
        public DiscoverableCollectionSettings()
        {
            IgnoredTypes = new List<Type>();
        }

        public bool AutoDiscover { get; set; }

        public string DiscoveryLocation { get; set; }

        public List<Type> IgnoredTypes { get; private set; }
    }

    public class RuntimePoliciesSettings : DiscoverableCollectionSettings
    {
        public RuntimePoliciesSettings()
        {
            ContentTypes = new List<ContentTypeSettings>();
            StatusCodes = new List<StatusCodeSettings>();
            Uris = new List<UrisSettings>();
        }

        public List<ContentTypeSettings> ContentTypes { get; private set; }

        public List<StatusCodeSettings> StatusCodes { get; private set; }

        public List<UrisSettings> Uris { get; private set; }
    }

    public class ContentTypeSettings
    {
        public string ContentType { get; set; }
    }

    public class StatusCodeSettings
    {
        public int StatusCode { get; set; }
    }

    public class UrisSettings
    {
        public Regex Regex { get; set; }
    }


    public static class ContentTypeElementCollectionExtensions
    {
        public static IEnumerable<string> ToEnumerable(this ContentTypeElementCollection collection)
        {
            foreach (ContentTypeElement typeElement in collection)
            {
                yield return typeElement.ContentType;
            }
        }
    }

    public static class StatusCodeElementCollectionExtensions
    {
        public static IEnumerable<int> ToEnumerable(this StatusCodeElementCollection collection)
        {
            foreach (StatusCodeElement typeElement in collection)
            {
                yield return typeElement.StatusCode;
            }
        }
    }

    public static class UrisElementCollectionExtensions
    {
        public static IEnumerable<Regex> ToEnumerable(this RegexElementCollection collection)
        {
            foreach (RegexElement typeElement in collection)
            {
                yield return typeElement.Regex;
            }
        }
    }
}
