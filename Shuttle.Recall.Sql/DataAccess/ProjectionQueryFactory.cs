using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql
{
	public class ProjectionQueryFactory : IProjectionQueryFactory
	{
	    private const string SelectFrom =
	        @"
select top {0} 
    es.Id, 
    Version, 
    AssemblyQualifiedName, 
    Data, 
    SequenceNumber, 
    DateRegistered 
from 
    [dbo].[EventStore] es 
inner join 
    [dbo].[TypeStore] ts on (es.TypeId = ts.Id)
";

		public IQuery GetSequenceNumber(string name)
		{
			return RawQuery.Create("select isnull((select SequenceNumber from [dbo].[ProjectionPosition] where [Name] = @Name), 0) as SequenceNumber")
				.AddParameterValue(ProjectionPositionColumns.Name, name);
		}

		public IQuery SetSequenceNumber(string name, long sequenceNumber)
		{
			return RawQuery.Create(@"
if exists (select SequenceNumber from [dbo].[ProjectionPosition] where [Name] = @Name)
	update [dbo].[ProjectionPosition] set SequenceNumber = @SequenceNumber where [Name] = @Name
else
	insert into [dbo].[ProjectionPosition] (Name, SequenceNumber) values (@Name, @SequenceNumber)
")
				.AddParameterValue(ProjectionPositionColumns.Name, name)
				.AddParameterValue(ProjectionPositionColumns.SequenceNumber, sequenceNumber);
		}

		public IQuery Get(long sequenceNumber, int limit)
		{
			return
				RawQuery.Create(string.Concat(string.Format(SelectFrom, limit > 0 ? limit : 1), "where es.SequenceNumber >= @SequenceNumber order by es.SequenceNumber"))
					.AddParameterValue(EventStoreColumns.SequenceNumber, sequenceNumber);
		}

		public IQuery Get(long sequenceNumber, int limit, IEnumerable<Type> eventTypes)
		{
			var types = eventTypes as IList<Type> ?? eventTypes.ToList();

            if (eventTypes == null || !types.Any())
			{
				return Get(sequenceNumber, limit);
			}

			var typesValue = new StringBuilder();

			foreach (var type in types)
			{
				typesValue.Append(string.Format("'{0}'", type.AssemblyQualifiedName));
			}

			return
				RawQuery.Create(
					string.Concat(string.Format(SelectFrom, limit > 0 ? limit : 1), string.Format("where es.SequenceNumber >= @SequenceNumber and ts.AssemblyQualifiedName in ({0}) order by es.SequenceNumber", typesValue)))
					.AddParameterValue(EventStoreColumns.SequenceNumber, sequenceNumber);
		}
	}
}