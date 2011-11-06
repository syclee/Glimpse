using System.Collections.Generic;
using System.ComponentModel.Composition;
using Glimpse.Core.Extensibility;
using Glimpse.EF.Plumbing.Injectors;

namespace Glimpse.EF.Service
{
    [GlimpseService]
    internal class EF : IGlimpseService
    {
        private IGlimpseLogger Logger { get; set; }
        private IList<IWrapperInjectorProvider> Providers { get; set; }
        
        [ImportingConstructor]
        public EF(IGlimpseFactory factory)
        {
            Logger = factory.CreateLogger();

            //Note order of execution is important.
            Providers = new List<IWrapperInjectorProvider>
                            {
                                new WrapDbProviderFactories(Logger),
                                new WrapDbConnectionFactories(Logger),
                                new WrapCachedMetadata(Logger)
                            };

        }

        public string Name
        {
            get { return "EF"; }
        }

        public void SetupInit()
        {
            Logger.Info("AdoPipelineInitiator: Starting");

            foreach (var provider in Providers)
                provider.Inject();

            Logger.Info("AdoPipelineInitiator: Finished");
        }
    }
}