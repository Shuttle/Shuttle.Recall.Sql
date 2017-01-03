using NUnit.Framework;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.Tests
{
	[TestFixture]
	public class Fixture : Recall.Tests.EventStoreFixture
	{
		[SetUp]
		public void TestSetUp()
		{
			DatabaseGateway = new DatabaseGateway();
			DatabaseContextFactory = new DatabaseContextFactory(new DbConnectionFactory(), new DbCommandFactory(),
				new ThreadStaticDatabaseContextCache());

			EmptyDataStore();
		}

		public DatabaseGateway DatabaseGateway { get; private set; }
		public IDatabaseContextFactory DatabaseContextFactory { get; private set; }

		public string EventStoreConnectionStringName = "EventStore";
		public string EventStoreProjectionConnectionStringName = "EventStoreProjection";

		[TearDown]
		protected void EmptyDataStore()
		{
			using (DatabaseContextFactory.Create(EventStoreConnectionStringName))
			{
				DatabaseGateway.ExecuteUsing(RawQuery.Create("delete from EventStore"));
				DatabaseGateway.ExecuteUsing(RawQuery.Create("delete from KeyStore"));
				DatabaseGateway.ExecuteUsing(RawQuery.Create("delete from SnapshotStore"));
				DatabaseGateway.ExecuteUsing(RawQuery.Create("delete from TypeStore"));
			}

            using (DatabaseContextFactory.Create(EventStoreProjectionConnectionStringName))
			{
                DatabaseGateway.ExecuteUsing(RawQuery.Create("delete from ProjectionPosition"));
            }
        }
	}
}