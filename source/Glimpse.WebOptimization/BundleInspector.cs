using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Optimization;
using Glimpse.AspNet.Extensibility;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;

namespace Glimpse.WebOptimization
{
    public class GlimpseBundleBuilder : IBundleBuilder, IWrapper<IBundleBuilder>
    {
        public GlimpseBundleBuilder(IBundleBuilder builder, string path, IMessageBroker broker)
        {
            Builder = builder;
            Path = path;
            Broker = broker;
            Executed = false;
        }

        public IBundleBuilder Builder { get; set; }
        public string Path { get; set; }
        public IMessageBroker Broker { get; set; }
        public bool Executed { get; set; }

        public string BuildBundleContent(System.Web.Optimization.Bundle bundle, BundleContext context, IEnumerable<BundleFile> files)
        {
            var result = Builder.BuildBundleContent(bundle, context, files);

            Trace.Write(string.Format("BuildBundleContent for '{1}' with files: {0}", string.Join(",", files.Select(f => f.IncludedVirtualPath)), Path));

            var server = context.HttpContext.Server;

            Broker.Publish(new Message
                {
                    VirtualPath = Path,
                    IncludedFiles = files.ToDictionary(f => f.IncludedVirtualPath, f => new FileInfo(server.MapPath(f.IncludedVirtualPath)).Length)
                });

            return result;
        }

        public class Message
        {
            public string VirtualPath { get; set; }

            public IDictionary<string, long> IncludedFiles { get; set; }
        }

        public IBundleBuilder GetWrappedObject()
        {
            return Builder;
        }
    }

    public class GlimpseBundleTransform : IBundleTransform, IWrapper<IBundleTransform>
    {
        public GlimpseBundleTransform(IBundleTransform bundleTransform, string path, IMessageBroker broker)
        {
            Transform = bundleTransform;
            Path = path;
            Broker = broker;
        }

        public IBundleTransform Transform { get; set; }
        public string Path { get; set; }
        public IMessageBroker Broker { get; set; }

        public void Process(BundleContext context, BundleResponse response)
        {
            Trace.Write(string.Format("Process for '{1}' with transform: {0}", Transform.GetType().Name, Path));

            var stopwatch = Stopwatch.StartNew();
            Transform.Process(context, response);
            stopwatch.Stop();

            Broker.Publish(new Message
                {
                    VirtualPath = Path,
                    TransformType = Transform.GetType(),
                    ExecutionTime = stopwatch.Elapsed,
                });
        }

        public class Message
        {
            public string VirtualPath { get; set; }

            public Type TransformType { get; set; }

            public TimeSpan ExecutionTime { get; set; }
        }

        public IBundleTransform GetWrappedObject()
        {
            return Transform;
        }
    }

    public class BundleInspector : IInspector
    {
        public void Setup(IInspectorContext context)
        {
            foreach (var bundle in BundleTable.Bundles)
            {
                bundle.Builder = new GlimpseBundleBuilder(bundle.Builder, bundle.Path, context.MessageBroker);

                var bundleTransforms = bundle.Transforms;
                for (int i = 0; i < bundleTransforms.Count; i++)
                {
                    bundleTransforms[i] = new GlimpseBundleTransform(bundleTransforms[i], bundle.Path, context.MessageBroker);
                }
            }

        }
    }

    public class Bundle : AspNetTab, ITabSetup
    {
        public override string Name
        {
            get { return "Bundles"; }
        }

        public override object GetData(ITabContext context)
        {
            var itemsProperty = typeof(Bundle).GetProperty("Items", BindingFlags.Instance | BindingFlags.NonPublic);

            var config = new List<object[]> { new[] { "Builder", "CdnPath", "ConcatenationToken", "FileExtReplacment", "Orderer", "Path", "Transforms", "Files" } };

            var builder = context.GetMessages<GlimpseBundleBuilder.Message>();
            var xform = context.GetMessages<GlimpseBundleTransform.Message>();

            foreach (var bundle in BundleTable.Bundles)
            {
                var bundleBuilder = bundle.Builder as GlimpseBundleBuilder;

                var transforms = GetTransforms(xform, bundle.Path);
                var files = GetFiles(builder, bundle.Path);

                if (files.Count() > 0)
                {
                    bundleBuilder.Executed = true;
                }

                config.Add(
                    new object[]
                        {
                         bundleBuilder.GetWrappedObject().GetType().FullName,
                         bundle.CdnPath,
                         bundle.ConcatenationToken,
                         bundle.EnableFileExtensionReplacements.ToString(),
                         bundle.Orderer.GetType().FullName,
                         bundle.Path,
                         transforms.Count() > 0 ? transforms : bundleBuilder.Executed ? new[]{ "From Cache" } : new[]{ "Not Requested" },
                         files.Count() > 0 ? files : bundleBuilder.Executed ? new[]{ "From Cache" } : new[]{ "Not Requested" },
                        });
            }


            return config;
        }

        public IEnumerable<string> GetTransforms(IEnumerable<GlimpseBundleTransform.Message> messages, string path)
        {
            return
                messages.Where(x => x.VirtualPath.Equals(path))
                     .Select(x => string.Format("{0} ({1} ms)", x.TransformType.Name, x.ExecutionTime.Milliseconds));
        }

        public IEnumerable<string> GetFiles(IEnumerable<GlimpseBundleBuilder.Message> messages, string path)
        {
            foreach (var message in messages.Where(x => x.VirtualPath.Equals(path)))
            {
                foreach (var item in message.IncludedFiles)
                {
                    yield return string.Format("!<a href='{2}'>{0}</a> ({1} bytes)!", item.Key, item.Value, VirtualPathUtility.ToAbsolute(item.Key));
                }
            }
        }

        public void Setup(ITabSetupContext context)
        {
            context.PersistMessages<GlimpseBundleBuilder.Message>();
            context.PersistMessages<GlimpseBundleTransform.Message>();
        }
    }
}