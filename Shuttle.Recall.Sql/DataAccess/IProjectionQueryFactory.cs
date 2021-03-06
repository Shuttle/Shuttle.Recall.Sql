﻿using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql
{
	public interface IProjectionQueryFactory
	{
		IQuery GetSequenceNumber(string name);
		IQuery SetSequenceNumber(string name, long sequenceNumber);
	}
}