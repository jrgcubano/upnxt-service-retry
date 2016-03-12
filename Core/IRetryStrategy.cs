using System;

namespace Core
{
    public interface IRetryStrategy : IRetryUnit, IExecuteServiceCall
    {
        IRetryStrategy OnFailRetryFor(int maxRetries);
        IRetryStrategy OnServerFault(Func<object> handler);
    }
}
