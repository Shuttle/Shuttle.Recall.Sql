﻿using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Sql
{
    public class PrimitiveEventQueryFactory : IPrimitiveEventQueryFactory
    {
        private readonly IScriptProvider _scriptProvider;

        public PrimitiveEventQueryFactory(IScriptProvider scriptProvider)
        {
            Guard.AgainstNull(scriptProvider, "scriptProvider");

            _scriptProvider = scriptProvider;
        }

        public IQuery RemoveSnapshot(Guid id)
        {
            return new RawQuery(_scriptProvider.Get("RemoveSanpshot")).AddParameterValue(EventStoreColumns.Id, id);
        }

        public IQuery RemoveEventStream(Guid id)
        {
            return new RawQuery(_scriptProvider.Get("RemoveEventStream")).AddParameterValue(EventStoreColumns.Id, id);
        }

        public IQuery GetEventStream(Guid id)
        {
            return new RawQuery(_scriptProvider.Get("GetEventStream")).AddParameterValue(EventStoreColumns.Id, id);
        }

        public IQuery GetProjectionEvents(long fromSequenceNumber, IEnumerable<Type> eventTypes, int limit)
        {
            return
                new RawQuery(string.Format(_scriptProvider.Get("GetProjectionEvents"), limit,
                    eventTypes == null
                        ? string.Empty
                        : string.Format("and EventType in ({0})",
                            string.Join(",",
                                eventTypes.Select(eventType => string.Concat("'", eventType, "'")).ToArray()))))
                    .AddParameterValue(EventStoreColumns.SequenceNumber, fromSequenceNumber);
        }
    }
}