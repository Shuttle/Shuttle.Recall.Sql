using System;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql
{
	public interface IKeyStoreQueryFactory
	{
		IQuery Get(string key);
		IQuery Add(Guid id, string key);
		IQuery Remove(string key);
	    IQuery Remove(Guid id);
	    IQuery Contains(string key);
	}
}