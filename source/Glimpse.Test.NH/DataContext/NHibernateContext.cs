using System;
using System.Diagnostics;
using System.IO;

namespace Glimpse.Test.NH.DataContext
{
    public interface INHibernateContext : IDisposable
    {
    }

    public class NHibernateContext : INHibernateContext
    {
        private readonly NHibernateVersion _version;

        public NHibernateContext(NHibernateVersion version)
        {
            _version = version;
            LoadAssembliesForVersion();
            BuildSessionFactoryForVersion();
        }

        public void Dispose()
        {
            UnloadAssembliesForVersion();
        }

        private void LoadAssembliesForVersion()
        {
            var nhibernatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(@"DataContext\{0}", _version));
            var assembliesToLoad = Directory.GetFiles(nhibernatePath, "*.dll");
            foreach (var assemblyFile in assembliesToLoad)
            {
                var assemblyFileToCreate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, new FileInfo(assemblyFile).Name);
                File.Copy(assemblyFile, assemblyFileToCreate, true);
            }
        }

        private void BuildSessionFactoryForVersion()
        {
            var configurationType = Type.GetType("NHibernate.Cfg.Configuration, NHibernate", true, true);
            var configuration = configurationType.GetConstructor(new Type[] { }).Invoke(null);
            var configureMethodInfo = configurationType.GetMethod("Configure", new[] { typeof(string) });
            var buildSessionFactoryMethodInfo = configurationType.GetMethod("BuildSessionFactory", new Type[] { });
            configuration = configureMethodInfo.Invoke(configuration, new[] { Path.Combine(AppDomain.CurrentDomain.BaseDirectory + string.Format(@"\DataContext\{0}\", _version), "hibernate.cfg.xml") });

            var sessionFactory = buildSessionFactoryMethodInfo.Invoke(configuration, null);
            Trace.WriteLine("Loaded: " + sessionFactory);
        }

        private void UnloadAssembliesForVersion()
        {
            var nhibernatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(@"DataContext\{0}", _version));
            var assembliesToLoad = Directory.GetFiles(nhibernatePath, "*.dll");
            foreach (var assemblyFile in assembliesToLoad)
            {
                var assemblyFileToDelete = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, new FileInfo(assemblyFile).Name);
                File.Delete(assemblyFileToDelete);
            }
        }
    }
}