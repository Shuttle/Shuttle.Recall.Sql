# Shuttle.Recall.Sql

***OBSOLETE***

Please use [Shuttle.Recall.Sql.Storage](https://github.com/Shuttle/Shuttle.Recall.Sql.Storage) for storage SQL-based storage implementations and [Shuttle.Recall.Sql.EventProcessing](https://github.com/Shuttle/Shuttle.Recall.Sql.EventProcessing) for projections.

A Sql Server implementation of the `Shuttle.Recall` event sourcing mechanism.

### Event Sourcing / Processing

~~~ c#
// use any of the supported DI containers
var container = new WindsorComponentContainer(new WindsorContainer());

container.Register<IScriptProvider>(new ScriptProvider(new ScriptProviderConfiguration
{
	ResourceAssembly = Assembly.GetAssembly(typeof(PrimitiveEventRepository)),
	ResourceNameFormat = SqlResources.SqlClientResourceNameFormat
}));

// register event/projection handlers for event processing along with any other dependencies
container.Register<MyHandler, MyHandler>();
container.Register<IMyQueryFactory, MyQueryFactory>();
container.Register<IMyQuery, MyQuery>();

var processor = EventProcessor.Create(container);

// create and add all your projections
\_eventProcessor.AddProjection(new Projection("MyProjection").AddEventHandler(container.Resolve<MyHandlerHandler>()));

processor.Start();

// dispose when done
processor.Dispose();
~~~

### Application Configuration File

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
