using System;
using System.Collections.Generic;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql
{
    public interface IPrimitiveEventQueryFactory
    {
        IQuery RemoveSnapshot(Guid id);
        IQuery RemoveEventStream(Guid id);
        IQuery GetEventStream(Guid id);
        IQuery GetProjectionEvents(long fromSequenceNumber, IEnumerable<Type> eventTypes, int limit);
    }
}