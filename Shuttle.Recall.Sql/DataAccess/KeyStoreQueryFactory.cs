using System;
using System.Runtime.Remoting.Messaging;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql
{
    public class KeyStoreQueryFactory : IKeyStoreQueryFactory
    {
        public IQuery Get(string key)
        {
            return RawQuery.Create(@"select Id from dbo.KeyStore where [Key] = @Key")
                    .AddParameterValue(KeyStoreColumns.Key, key);
        }

        public IQuery Add(Guid id, string key)
        {
            return
                RawQuery.Create(@"
insert into dbo.KeyStore
	(
        [Key],
		[Id]
	)
values
	(
		@Key,
		@Id
)
")
                    .AddParameterValue(KeyStoreColumns.Key, key)
                    .AddParameterValue(KeyStoreColumns.Id, id);
        }

        public IQuery Remove(string key)
        {
            return RawQuery.Create(@"delete from dbo.KeyStore where [Key] = @Key")
                    .AddParameterValue(KeyStoreColumns.Key, key);
        }

        public IQuery Remove(Guid id)
        {
            return RawQuery.Create(@"delete from dbo.KeyStore where [Id] = @Id")
                    .AddParameterValue(KeyStoreColumns.Id, id);
        }

        public IQuery Contains(string key)
        {
            return RawQuery.Create(@"if exists (select null from dbo.KeyStore where [Key] = @Key) select 1 else select 0")
                    .AddParameterValue(KeyStoreColumns.Key, key);
        }
    }
}