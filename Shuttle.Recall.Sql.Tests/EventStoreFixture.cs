using System.Reflection;
using Castle.Windsor;
using NUnit.Framework;
using Shuttle.Core.Castle;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;
using Shuttle.Recall.Tests;

namespace Shuttle.Recall.Sql.Tests
{
    public class EventStoreFixture : Fixture
    {
        [Test]
        public void ExerciseEventStore()
        {
            var container = new WindsorComponentContainer(new WindsorContainer());

            container.Register<IScriptProvider>(new ScriptProvider(new ScriptProviderConfiguration
            {
                ResourceAssembly = Assembly.GetAssembly(typeof (PrimitiveEventRepository)),
                ResourceNameFormat = SqlResources.SqlClientResourceNameFormat
            }));

            container.Register<IDatabaseContextCache, ThreadStaticDatabaseContextCache>();
            container.Register<IDatabaseContextFactory, DatabaseContextFactory>();
            container.Register<IDbConnectionFactory, DbConnectionFactory>();
            container.Register<IDbCommandFactory, DbCommandFactory>();
            container.Register<IDatabaseGateway, DatabaseGateway>();
            container.Register<IQueryMapper, QueryMapper>();
            container.Register<IPrimitiveEventRepository, PrimitiveEventRepository>();
            container.Register<IPrimitiveEventQueryFactory, PrimitiveEventQueryFactory>();

            EventStoreConfigurator.Configure(container);

            using (container.Resolve<IDatabaseContextFactory>().Create(EventStoreConnectionStringName))
            {
                RecallFixture.ExerciseEventStore(EventStore.Create(container));
            }
        }
    }
}