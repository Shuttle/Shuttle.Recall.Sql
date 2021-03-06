﻿using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Sql
{
	public class ProjectionQueryFactory : IProjectionQueryFactory
	{
	    private readonly IScriptProvider _scriptProvider;

	    public ProjectionQueryFactory(IScriptProvider scriptProvider)
	    {
            Guard.AgainstNull(scriptProvider, "scriptProvider");

	        _scriptProvider = scriptProvider;
	    }

	    public IQuery GetSequenceNumber(string name)
	    {
			return RawQuery.Create(_scriptProvider.Get("Projection.GetSequenceNumber"))
				.AddParameterValue(ProjectionPositionColumns.Name, name);
		}

		public IQuery SetSequenceNumber(string name, long sequenceNumber)
		{
			return RawQuery.Create(_scriptProvider.Get("Projection.SetSequenceNumber"))
				.AddParameterValue(ProjectionPositionColumns.Name, name)
				.AddParameterValue(ProjectionPositionColumns.SequenceNumber, sequenceNumber);
		}
	}
}