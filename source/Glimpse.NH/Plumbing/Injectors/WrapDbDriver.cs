using Glimpse.Core.Extensibility;

namespace Glimpse.NH.Plumbing.Injectors
{
    public class WrapDbDriver
    {
        private IGlimpseLogger Logger { get; set; }

        public WrapDbDriver(IGlimpseLogger logger)
        {
            Logger = logger;
        }

        public void Inject()
        {
            // TODO: figure out how to inject the GlimpseProfileDbDriver
            Logger.Info("AdoPipelineInitiator for EF: Unable to automatically wrap the DbDriver");
        }
    }
}