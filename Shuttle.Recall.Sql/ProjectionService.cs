using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Sql
{
    public class ProjectionService : IProjectionService, IRequireInitialization
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IProjectionConfiguration _projectionConfiguration;
        private readonly IProjectionQueryFactory _queryFactory;
        private readonly Dictionary<string, long> _sequenceNumbers = new Dictionary<string, long>();
        private readonly ISerializer _serializer;

        public ProjectionService(ISerializer serializer, IProjectionConfiguration projectionConfiguration,
            IDatabaseContextFactory databaseContextFactory, IDatabaseGateway databaseGateway,
            IProjectionQueryFactory queryFactory)
        {
            Guard.AgainstNull(serializer, "serializer");
            Guard.AgainstNull(projectionConfiguration, "projectionConfiguration");
            Guard.AgainstNull(databaseContextFactory, "databaseContextFactory");
            Guard.AgainstNull(databaseGateway, "databaseGateway");
            Guard.AgainstNull(queryFactory, "queryFactory");

            _serializer = serializer;
            _projectionConfiguration = projectionConfiguration;
            _databaseContextFactory = databaseContextFactory;
            _databaseGateway = databaseGateway;
            _queryFactory = queryFactory;
        }

        public long GetSequenceNumber(string name)
        {
            if (!_projectionConfiguration.SharedConnection)
            {
                _databaseContextFactory.DatabaseContextCache.Use("EventProjectionDatabaseContext");
            }

            return _sequenceNumbers.ContainsKey(name)
                ? _sequenceNumbers[name]
                : _databaseGateway.GetScalarUsing<long>(_queryFactory.GetSequenceNumber(name));
        }

        public void SetSequenceNumber(string name, long sequenceNumber)
        {
            if (!_projectionConfiguration.SharedConnection)
            {
                _databaseContextFactory.DatabaseContextCache.Use("EventProjectionDatabaseContext");
            }

            _databaseGateway.ExecuteUsing(_queryFactory.SetSequenceNumber(name, sequenceNumber));

            if (_sequenceNumbers.ContainsKey(name))
            {
                _sequenceNumbers[name] = sequenceNumber;
            }
            else
            {
                _sequenceNumbers.Add(name, sequenceNumber);
            }
        }

        public ProjectionEvent GetEvent(string name, long sequenceNumber)
        {
            if (!_projectionConfiguration.SharedConnection)
            {
                _databaseContextFactory.DatabaseContextCache.Use("EventStoreDatabaseContext");
            }

            return ProjectionEvent(_databaseGateway.GetSingleRowUsing(_queryFactory.Get(sequenceNumber, 1)));
        }

        public ProjectionEvent GetEvent(string name, long sequenceNumber, IEnumerable<Type> eventTypes)
        {
            if (!_projectionConfiguration.SharedConnection)
            {
                _databaseContextFactory.DatabaseContextCache.Use("EventStoreDatabaseContext");
            }

            return ProjectionEvent(_databaseGateway.GetSingleRowUsing(_queryFactory.Get(sequenceNumber, 1, eventTypes)));
        }

        public void Initialize(IEventProcessor eventProcessor)
        {
            Guard.AgainstNull(eventProcessor, "eventProcessor");

            eventProcessor.Events.PipelineCreated += PipelineCreated;
        }

        private ProjectionEvent ProjectionEvent(DataRow row)
        {
            if (row == null)
            {
                return null;
            }

            var assemblyQualifiedName = SnapshotStoreColumns.AssemblyQualifiedName.MapFrom(row);

            using (var stream = new MemoryStream(EventStoreColumns.Data.MapFrom(row)))
            {
                return new ProjectionEvent(
                    EventStoreColumns.Id.MapFrom(row),
                    new Event(EventStoreColumns.Version.MapFrom(row), assemblyQualifiedName,
                        _serializer.Deserialize(Type.GetType(assemblyQualifiedName), stream)),
                    EventStoreColumns.DateRegistered.MapFrom(row),
                    EventStoreColumns.SequenceNumber.MapFrom(row));
            }
        }

        private void PipelineCreated(object sender, PipelineEventArgs e)
        {
            e.Pipeline.RegisterObserver(new DatabaseContextObserver(_databaseContextFactory, _projectionConfiguration));
        }
    }
}