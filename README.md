# Shuttle.Recall.Sql

A Sql Server implementation of the `Shuttle.Recall` event sourcing mechanism.

### Event Sourcing

~~~ c#
var databaseContextFactory = DatabaseContextFactory.Default();
var eventStore = new EventStore(new DefaultSerializer(), new DatabaseGateway(), new EventStoreQueryFactory());

using (databaseContextFactory.Create("eventStringConnectionStringName"))
{
	var stream = eventStore.Get(sampleAggregateId);

	if (stream.IsEmpty)
	{
		return;
	}

	sampleAggregate = new SampleAggregate(sampleAggregateId;
	stream.Apply(sampleAggregate);

	stream.AddEvent(sampleAggregate.CommandReturningEvent("Some Data"));

	eventStore.SaveEventStream(stream);
}
~~~

### Event Processing

~~~ c#
public ProjectionService(
	ISerializer serializer, 
	IProjectionConfiguration projectionConfiguration, 
	IDatabaseContextFactory databaseContextFactory, 
	IDatabaseGateway databaseGateway, 
	IProjectionQueryFactory queryFactory)
~~~

You can use the `DefaultSerializer` implementation for the `ISerializer` from the [Shuttle.Core.Infrastructure](http://shuttle.github.io/shuttle-core/overview-serializer/) package as a starting point.

The `IProjectionConfiguration` specifies the `ProviderName` and `ConnectionString` to use to connect to the database.  These can be configured using the `ProjectionSection` configuration section in the application configuration file:

~~~ xml
<configuration>
	<configSections>
		<sectionGroup name="shuttle">
			<section 
				name="projection" 
				type="Shuttle.Recall.Sql.ProjectionSection, Shuttle.Recall.Sql" />
		</sectionGroup>
	</configSections>

	<shuttle>
		<projection connectionStringName="EventStore" />
	</shuttle>

	<connectionStrings>
		<clear />
		<add 
			name="EventStore" 
			connectionString="Data Source=.\sqlexpress;Initial Catalog=shuttle;Integrated Security=SSPI;" 
			providerName="System.Data.SqlClient" />
	</connectionStrings>
</configuration>
~~~

Use can then call `ProjectionSection.Configuration()` to return the configuration set up according to the application configuration files `ProjectionSection`.

The `IDatabaseContextFactory` and `IDatabaseGateway` implementation follow the structures as defined in the [Shuttle.Core.Data](http://shuttle.github.io/shuttle-core/overview-data/) package.

For the `IProjectionQueryFactory` you can simply specify `new ProjectionQueryFactory()`.