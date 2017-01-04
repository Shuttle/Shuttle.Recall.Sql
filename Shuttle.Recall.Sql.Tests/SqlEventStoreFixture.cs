using System;
using System.Reflection;
using Castle.Windsor;
using NUnit.Framework;
using Shuttle.Core.Castle;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;
using Shuttle.Recall;
using Shuttle.Recall.Tests;

namespace Shuttle.Recall.Sql.Tests
{
    public class SqlEventStoreFixture : EventStoreFixture
    {
        [Test]
        public void ExerciseEventStore()
        {
            var container = new WindsorComponentContainer(new WindsorContainer());

            container.Register<IScriptProvider>(new ScriptProvider(new ScriptProviderConfiguration
            {
                ResourceAssembly = Assembly.GetAssembly(typeof(PrimitiveEventRepository)),
                ResourceNameFormat = ""
            }));

            container.Register<IDatabaseContextCache, ThreadStaticDatabaseContextCache>();
            container.Register<IDatabaseContextFactory, DatabaseContextFactory>();
            container.Register<IDbConnectionFactory, DbConnectionFactory>();
            container.Register<IDbCommandFactory, DbCommandFactory>();
            container.Register<IDatabaseGateway, DatabaseGateway>();
            container.Register<IPrimitiveEventRepository, PrimitiveEventRepository>();

            new DefaultConfigurator(container).Configure();


            base.ExerciseEventStore(EventStore.Create(container));
        }
    }
}