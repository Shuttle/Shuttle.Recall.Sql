using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Sql
{
    public class DatabaseContextModule
    {
        private readonly DatabaseContextObserver _databaseContextObserver;
        private readonly string _pipelineName = typeof(EventProcessingPipeline).FullName;

        public DatabaseContextModule(IPipelineFactory pipelineFactory, DatabaseContextObserver databaseContextObserver)
        {
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");
            Guard.AgainstNull(databaseContextObserver, "databaseContextObserver");

            _databaseContextObserver = databaseContextObserver;

            pipelineFactory.PipelineCreated += PipelineCreated;
        }

        private void PipelineCreated(object sender, PipelineEventArgs e)
        {
            if (!e.Pipeline.GetType().FullName.Equals(_pipelineName, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            e.Pipeline.RegisterObserver(_databaseContextObserver);
        }
    }
}