using System;
using NUnit.Framework;
using Shuttle.Core.Infrastructure;
using Shuttle.Recall;

namespace Shuttle.Recall.Sql.Tests
{
	public class EventStoreFixture : Fixture
	{
		[Test]
		public void Should_be_able_to_use_event_stream()
		{
			var store = GetEventStore();
			var aggregate = new Aggregate(Guid.NewGuid());
			EventStream eventStream;

			using (Timer.Time("get empty"))
			using (DatabaseContextFactory.Create(EventStoreConnectionStringName))
			{
				eventStream = store.Get(aggregate.Id);
			}

			var moveCommand = new MoveCommand();

			using (Timer.Time("adding initial events"))
				for (var i = 0; i < 100; i++)
				{
					moveCommand = new MoveCommand
					{
						Address = string.Format("Address-{0}", i),
						DateMoved = DateTime.Now
					};

					eventStream.AddEvent(aggregate.Move(moveCommand));
				}

			using (Timer.Time("saving event stream"))
			using (DatabaseContextFactory.Create(EventStoreConnectionStringName))
			{
				store.SaveEventStream(eventStream);
			}

			using (DatabaseContextFactory.Create(EventStoreConnectionStringName))
			{
				aggregate = new Aggregate(aggregate.Id);

				using (Timer.Time("reading event stream"))
				{
					eventStream = store.Get(aggregate.Id);
				}

				using (Timer.Time("apply events"))
				{
					eventStream.Apply(aggregate);
				}
			}

			Assert.AreEqual(moveCommand.Address, aggregate.State.Location.Address);
			Assert.AreEqual(moveCommand.DateMoved, aggregate.State.Location.DateMoved);

			using (Timer.Time("adding more events"))
				for (var i = 0; i < 100; i++)
				{
					moveCommand = new MoveCommand
					{
						Address = string.Format("Address-{0}", i),
						DateMoved = DateTime.Now
					};

					eventStream.AddEvent(aggregate.Move(moveCommand));
				}

			using (Timer.Time("saving event stream"))
			using (DatabaseContextFactory.Create(EventStoreConnectionStringName))
			{
				eventStream.AddSnapshot(aggregate.State);
				store.SaveEventStream(eventStream);
			}

			using (DatabaseContextFactory.Create(EventStoreConnectionStringName))
			{
				aggregate = new Aggregate(aggregate.Id);

				using (Timer.Time("reading event stream"))
				{
					eventStream = store.Get(aggregate.Id);
				}

				using (Timer.Time("apply events/snapshot"))
				{
					eventStream.Apply(aggregate);
				}
			}
		}

		private EventStore GetEventStore()
		{
			return new EventStore(new DefaultSerializer(), DatabaseGateway,
				new EventStoreQueryFactory());
		}

		[Test]
		public void Should_be_able_to_exercise_event_store()
		{
			using (DatabaseContextFactory.Create(EventStoreConnectionStringName))
			{
				ExerciseEventStore(GetEventStore());
			}
		}

		[Test]
		public void Should_be_able_to_exercise_event_store_ICanSnapshot()
		{
			using (DatabaseContextFactory.Create(EventStoreConnectionStringName))
			{
				ExerciseEventStoreCanSnapshot(GetEventStore());
			}
		}
	}
}