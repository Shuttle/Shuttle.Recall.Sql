using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Sql
{
    public class EventProjectionRepository : IEventProjectionRepository
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IProjectionQueryFactory _queryFactory;

        public EventProjectionRepository(IDatabaseGateway databaseGateway, IProjectionQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseGateway, "databaseGateway");
            Guard.AgainstNull(queryFactory, "queryFactory");

            _databaseGateway = databaseGateway;
            _queryFactory = queryFactory;
        }

        public long GetSequenceNumber(string projectionName)
        {
            return _databaseGateway.GetScalarUsing<long>(_queryFactory.GetSequenceNumber(projectionName));
        }

        public void SetSequenceNumber(string projectionName, long sequenceNumber)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.SetSequenceNumber(projectionName, sequenceNumber));
        }
    }
}