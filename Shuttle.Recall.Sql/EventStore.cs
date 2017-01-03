using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Sql
{
    public class EventStore : IEventStore
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IEventStoreQueryFactory _queryFactory;

        private readonly ISerializer _serializer;

        public EventStore(ISerializer serializer, IDatabaseGateway databaseGateway, IEventStoreQueryFactory queryFactory)
        {
            Guard.AgainstNull(serializer, "serializer");
            Guard.AgainstNull(databaseGateway, "databaseGateway");
            Guard.AgainstNull(queryFactory, "queryFactory");

            _serializer = serializer;
            _databaseGateway = databaseGateway;
            _queryFactory = queryFactory;
        }

        public EventStream Get(Guid id)
        {
            var version = 0;
            Event snapshot = null;
            var snapshotRow = _databaseGateway.GetSingleRowUsing(_queryFactory.GetSnapshot(id));

            if (snapshotRow != null)
            {
                version = SnapshotStoreColumns.Version.MapFrom(snapshotRow);

                var assemblyQualifiedName = SnapshotStoreColumns.AssemblyQualifiedName.MapFrom(snapshotRow);

                using (var stream = new MemoryStream(SnapshotStoreColumns.Data.MapFrom(snapshotRow)))
                {
                    snapshot = new Event(version, assemblyQualifiedName,
                        _serializer.Deserialize(Type.GetType(assemblyQualifiedName), stream));
                }
            }

            var events = Events(id, version);

            return new EventStream(id, events.Version, events.Events, snapshot);
        }

        public EventStream GetRaw(Guid id)
        {
            var events = Events(id, 0);

            return new EventStream(id, events.Version, events.Events, null);
        }

        public void Remove(Guid id)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.RemoveSnapshot(id));
            _databaseGateway.ExecuteUsing(_queryFactory.RemoveEventStream(id));
        }

        public void SaveEventStream(EventStream eventStream)
        {
            Guard.AgainstNull(eventStream, "eventStream");

            if (eventStream.Removed)
            {
                Remove(eventStream.Id);

                return;
            }

            eventStream.ConcurrencyInvariant(
                _databaseGateway.GetScalarUsing<int>(_queryFactory.GetVersion(eventStream.Id)));

            if (eventStream.HasSnapshot)
            {
                using (var stream = _serializer.Serialize(eventStream.Snapshot.Data))
                {
                    _databaseGateway.ExecuteUsing(_queryFactory.SaveSnapshot(eventStream.Id, eventStream.Snapshot,
                        stream.ToBytes()));
                }
            }

            foreach (var @event in eventStream.NewEvents())
            {
                using (var stream = _serializer.Serialize(@event.Data))
                {
                    _databaseGateway.ExecuteUsing(_queryFactory.AddEvent(eventStream.Id, @event, stream.ToBytes()));
                }
            }

            eventStream.CommitVersion();
        }

        private EventResult Events(Guid id, int fromVersion)
        {
            var table = _databaseGateway.GetDataTableFor(_queryFactory.Get(id, fromVersion));
            var result = new EventResult(fromVersion);

            foreach (DataRow row in table.Rows)
            {
                fromVersion = EventStoreColumns.Version.MapFrom(row);
                var assemblyQualifiedName = EventStoreColumns.AssemblyQualifiedName.MapFrom(row);

                using (var stream = new MemoryStream(EventStoreColumns.Data.MapFrom(row)))
                {
                    result.Add(new Event(fromVersion, assemblyQualifiedName,
                        _serializer.Deserialize(Type.GetType(assemblyQualifiedName), stream)));
                }
            }

            return result;
        }

        private class EventResult
        {
            public EventResult(int version)
            {
                Events = new List<Event>();
                Version = version;
            }

            public List<Event> Events { get; private set; }
            public int Version { get; private set; }

            public void Add(Event @event)
            {
                if (Version < @event.Version)
                {
                    Version = @event.Version;
                }

                Events.Add(@event);
            }
        }
    }
}