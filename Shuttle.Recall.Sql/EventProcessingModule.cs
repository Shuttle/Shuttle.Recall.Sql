using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Sql
{
	public class EventProcessingModule
	{
		private readonly EventProcessingObserver _eventProcessingObserver;
		private readonly string _pipelineName = typeof(EventProcessingPipeline).FullName;

		public EventProcessingModule(IPipelineFactory pipelineFactory, EventProcessingObserver eventProcessingObserver)
		{
			Guard.AgainstNull(pipelineFactory, "pipelineFactory");
			Guard.AgainstNull(eventProcessingObserver, "EventProcessingObserver");

			_eventProcessingObserver = eventProcessingObserver;

			pipelineFactory.PipelineCreated += PipelineCreated;
		}

		private void PipelineCreated(object sender, PipelineEventArgs e)
		{
			if (!e.Pipeline.GetType().FullName.Equals(_pipelineName, StringComparison.InvariantCultureIgnoreCase))
			{
				return;
			}

			e.Pipeline.RegisterObserver(_eventProcessingObserver);
		}

		public static IComponentRegistry Register(IComponentRegistry registry)
		{
			Guard.AgainstNull(registry, "registry");

			if (!registry.IsRegistered<IProjectionConfiguration>())
			{
				registry.Register<IProjectionConfiguration>(ProjectionSection.Configuration());
			}

			if (!registry.IsRegistered<EventProcessingObserver>())
			{
				registry.Register<EventProcessingObserver>();
			}

			if (!registry.IsRegistered<EventProcessingModule>())
			{
				registry.Register<EventProcessingModule>();
			}

			return registry;
		}
	}
}