using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Sql
{
	public class Bootstrap : 
		IComponentRegistryBootstrap,
		IComponentResolverBootstrap
	{
		public void Register(IComponentRegistry registry)
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
		}

		public void Resolve(IComponentResolver resolver)
		{
			Guard.AgainstNull(resolver, "resolver");

			resolver.Resolve<EventProcessingModule>();
		}
	}
}