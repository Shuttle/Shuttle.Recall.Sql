using System;
using Shuttle.Core.Data;
using Shuttle.Recall;

namespace Shuttle.Recall.Sql
{
	public interface IEventStoreQueryFactory
	{
		IQuery Get(Guid id, int afterVersion);
		IQuery GetVersion(Guid id);
		IQuery AddEvent(Guid id, Event @event, byte[] data);
		IQuery GetSnapshot(Guid id);
		IQuery SaveSnapshot(Guid id, Event @event, byte[] data);
	    IQuery RemoveSnapshot(Guid id);
	    IQuery RemoveEventStream(Guid id);
	}
}