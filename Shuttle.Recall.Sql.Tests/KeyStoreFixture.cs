using System;
using System.Data.SqlClient;
using NUnit.Framework;

namespace Shuttle.Recall.Sql.Tests
{
	public class KeyStoreFixture : Fixture
	{
		[Test]
		public void Should_be_able_to_use_key_store()
		{
			var store = new KeyStore(DatabaseGateway, new KeyStoreQueryFactory());

			var id = Guid.NewGuid();

			var value = string.Concat("value=", id.ToString());
			var anotherValue = string.Concat("anotherValue=", id.ToString());

			using (DatabaseContextFactory.Create(EventStoreConnectionStringName))
			{
				store.Add(id, value);

				Assert.Throws<SqlException>(() => store.Add(id, value));

				var idGet = store.Get(value);

				Assert.IsNotNull(idGet);
				Assert.AreEqual(id, idGet);

				idGet = store.Get(anotherValue);

				Assert.IsNull(idGet);
			}
		}
	}
}