using System.Diagnostics;
using Glimpse.Core.Extensibility;
using Glimpse.Nh.Plumbing.Profiler;

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
            var nhibernateAssembly = System.Reflection.Assembly.Load("NHibernate");
            if (nhibernateAssembly == null)
                return;
            
            var sessionFactoryObjectFactoryType = nhibernateAssembly.GetType("NHibernate.Impl.SessionFactoryObjectFactory", false, true);
            var intancesField = sessionFactoryObjectFactoryType.GetField("Instances", System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (intancesField == null)
                return;

            var sessionFactoryImplType = nhibernateAssembly.GetType("NHibernate.Impl.SessionFactoryImpl", false, true);
            var settingsField = sessionFactoryImplType.GetField("Settings", System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (settingsField == null)
                return;

            var settingsType = nhibernateAssembly.GetType("NHibernate.Cfg.Settings", false, true);
            var connectionProviderField = settingsType.GetProperty("ConnectionProvider", System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (connectionProviderField == null)
                return;

            var connectionProviderType = nhibernateAssembly.GetType("NHibernate.Connection.ConnectionProvider", false, true);
            var driverField = connectionProviderType.GetField("Driver", System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (driverField == null)
                return;

            var intances = (System.Collections.IDictionary)intancesField.GetValue(null);
            foreach (System.Collections.DictionaryEntry intance in intances)
            {
                var settings = settingsField.GetValue(intance.Value);
                var connectionProvider = connectionProviderField.GetValue(settings, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, null, null);
                var driver = driverField.GetValue(connectionProvider);
                var profiledDriverType = typeof (GlimpseProfileDbDriver<>).MakeGenericType(driver.GetType());
                var profiledDriverTypeConstructor = profiledDriverType.GetConstructor(new[] {driver.GetType()});
                var profiledDriver = profiledDriverTypeConstructor.Invoke(new[] {driver});

                driverField.SetValue(connectionProvider, profiledDriver, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, null);
                Debug.WriteLine(driver);
            }

            // TODO: figure out how to inject the GlimpseProfileDbDriver
            Logger.Info("AdoPipelineInitiator for EF: Unable to automatically wrap the DbDriver");
        }
    }
}