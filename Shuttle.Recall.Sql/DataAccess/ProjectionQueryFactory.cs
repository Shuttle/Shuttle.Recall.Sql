using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql
{
	public class ProjectionQueryFactory : IProjectionQueryFactory
	{
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
	}
}