using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using Glimpse.Ado.Plumbing;
using Glimpse.Ado.Plumbing.Profiler;
using NHibernate.AdoNet;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.Impl;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;

namespace Glimpse.NH.Plumbing.Profiler
{
    public class GlimpseProfileDbDriverNh121<TInnerDriver> : IDriver, ISqlParameterFormatter
        where TInnerDriver : class, IDriver, new()
    {
        private readonly TInnerDriver _innerDriver;

        public GlimpseProfileDbDriverNh121()
            : this(new TInnerDriver())
        {
        }

        public GlimpseProfileDbDriverNh121(TInnerDriver innerDriver)
        {
            if (innerDriver == null)
                throw new ArgumentNullException("innerDriver");

            _innerDriver = innerDriver;
        }

        public void Configure(IDictionary settings)
        {
            _innerDriver.Configure(settings);
        }

        public IDbConnection CreateConnection()
        {
            var innerConnection = _innerDriver.CreateConnection();
            if (innerConnection is GlimpseProfileDbConnection)
                return innerConnection;

            var connection = new GlimpseProfileDbConnection(innerConnection as DbConnection, null, new ProviderStats(), Guid.NewGuid());
            return connection;
        }

        public IDbCommand GenerateCommand(CommandType type, SqlString sqlString, SqlType[] parameterTypes)
        {
            var innerCommand = _innerDriver.GenerateCommand(type, sqlString, parameterTypes);
            if (innerCommand is GlimpseProfileDbCommand)
                return innerCommand;

            var command = new GlimpseProfileDbCommand(innerCommand as DbCommand, new ProviderStats());
            return command;
        }

        public void PrepareCommand(IDbCommand command)
        {
            _innerDriver.PrepareCommand(command);
        }

        public bool SupportsMultipleOpenReaders
        {
            get { return _innerDriver.SupportsMultipleOpenReaders; }
        }

        public bool SupportsMultipleQueries
        {
            get { return _innerDriver.SupportsMultipleQueries; }
        }

        public IBatcher CreateBatcher(ConnectionManager connectionManager)
        {
            var batcher = _innerDriver.CreateBatcher(connectionManager);
            return batcher;
        }

        public string GetParameterName(int index)
        {
            var innerSqlParameterFormatter = _innerDriver as ISqlParameterFormatter;
            return innerSqlParameterFormatter != null ? innerSqlParameterFormatter.GetParameterName(index) : null;
        }
    }
}