namespace Shuttle.Recall.Sql
{
	public interface IProjectionConfiguration
	{
		string EventStoreProviderName { get; set; }
		string EventStoreConnectionString { get; set; }
		string EventProjectionProviderName { get; set; }
		string EventProjectionConnectionString { get; set; }
        int EventProjectionPrefetchCount { get; set; }

        bool SharedConnection { get; }
	}
}