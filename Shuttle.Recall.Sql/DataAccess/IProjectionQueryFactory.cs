using System;
using System.Collections.Generic;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql
{
	public interface IProjectionQueryFactory
	{
		IQuery GetSequenceNumber(string name);
		IQuery SetSequenceNumber(string name, long sequenceNumber);
		IQuery Get(long sequenceNumber, int limit);
		IQuery Get(long sequenceNumber, int limit, IEnumerable<Type> eventTypes);
	}
}