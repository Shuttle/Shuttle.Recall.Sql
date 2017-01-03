using System.Transactions;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Sql
{
    public class DatabaseContextObserver :
        IPipelineObserver<OnAfterStartTransactionScope>,
        IPipelineObserver<OnAfterGetEvent>,
        IPipelineObserver<OnAfterAcknowledgeEvent>,
        IPipelineObserver<OnAbortPipeline>
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IProjectionConfiguration _projectionConfiguration;

        public DatabaseContextObserver(IDatabaseContextFactory databaseContextFactory,
            IProjectionConfiguration projectionConfiguration)
        {
            Guard.AgainstNull(databaseContextFactory, "databaseContextFactory");
            Guard.AgainstNull(projectionConfiguration, "projectionConfiguration");

            _databaseContextFactory = databaseContextFactory;
            _projectionConfiguration = projectionConfiguration;
        }

        public void Execute(OnAbortPipeline pipelineEvent)
        {
            var context = pipelineEvent.Pipeline.State.Get<IDatabaseContext>();

            if (context == null)
            {
                return;
            }

            context.AttemptDispose();
        }

        public void Execute(OnAfterAcknowledgeEvent pipelineEvent)
        {
            pipelineEvent.Pipeline.State.Get<IDatabaseContext>().AttemptDispose();
        }

        public void Execute(OnAfterStartTransactionScope pipelineEvent)
        {
            if (_projectionConfiguration.SharedConnection)
            {
                var databaseContext = _databaseContextFactory.Create(_projectionConfiguration.EventStoreProviderName,
                    _projectionConfiguration.EventStoreConnectionString);

                pipelineEvent.Pipeline.State.Add(databaseContext);
            }
            else
            {
                pipelineEvent.Pipeline.State.Add("EventProjectionDatabaseContext",
                    _databaseContextFactory.Create(_projectionConfiguration.EventProjectionProviderName,
                        _projectionConfiguration.EventProjectionConnectionString)
                        .WithName("EventProjectionDatabaseContext"));

                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    pipelineEvent.Pipeline.State.Add("EventStoreDatabaseContext",
                        _databaseContextFactory.Create(_projectionConfiguration.EventStoreProviderName,
                            _projectionConfiguration.EventStoreConnectionString)
                            .WithName("EventStoreDatabaseContext"));
                }
            }
        }

        public void Execute(OnAfterGetEvent pipelineEvent1)
        {
            if (_projectionConfiguration.SharedConnection)
            {
                return;
            }

            _databaseContextFactory.DatabaseContextCache.Use("EventProjectionDatabaseContext");
        }
    }
}