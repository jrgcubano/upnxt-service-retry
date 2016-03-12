using Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace Core
{
    /// <summary>
    /// Manager for calling services with retry strategies
    /// </summary>
    public interface IServiceManager : IRetryStrategy, IExecuteServiceCall
    {
        IServiceManager Call(Func<HttpResponseMessage> service);
        object GetServerFaultResult();
    }

    public class ServiceManager : IServiceManager
    {
        private double _maxRetries;
        private int _maxRetryMinutes;
        private RetryUnit _retryUnit;

        private Func<HttpResponseMessage> _serviceCall;
        private Func<object> _serverFaultHandler;
        private object _serverFaultResult;

        public IServiceManager Call(Func<HttpResponseMessage> service)
        {
            _serviceCall = service;
            return this;
        }

        public object GetServerFaultResult()
        {
            return _serverFaultResult;
        }

        public IRetryStrategy OnFailRetryFor(int maxRetries)
        {
            _maxRetries = maxRetries;
            return this;
        }

        public IRetryStrategy OnServerFault(Func<object> handler)
        {
            _serverFaultHandler = handler;
            return this;
        }

        public IRetryUnit Times()
        {
            _retryUnit = RetryUnit.Consecutive;
            return this;
        }

        public IRetryUnit Seconds()
        {
            _retryUnit = RetryUnit.Delay;
            return this;
        }

        public IRetryUnit Minutes()
        {
            _retryUnit = RetryUnit.Delay;
            _maxRetryMinutes = (int)_maxRetries;
            return this;
        }

        public IRetryUnit BackOff()
        {
            _retryUnit = RetryUnit.BackOff;
            return this;
        }

        public HttpResponseMessage Execute()
        {
            var exeptions = new List<Exception>();

            for (var retry = 0d; retry < GetRetrySeconds(); retry++)
            {
                try
                {
                    if (retry > 0)
                    {
                        switch (_retryUnit)
                        {
                            case RetryUnit.Consecutive:
                                retry++;
                                break;

                            case RetryUnit.Delay:
                                Thread.Sleep(TimeSpan.FromSeconds(GetRetrySeconds()));
                                break;

                            case RetryUnit.BackOff:
                                retry = Math.Min(GetRetrySeconds(), retry * 2);
                                Thread.Sleep(TimeSpan.FromSeconds(retry));
                                break;
                        }
                    }

                    var result = _serviceCall();
                    if (IsServerFault(result))
                        _serverFaultResult = _serverFaultHandler();

                    return result;
                }
                catch (Exception ex)
                {
                    exeptions.Add(ex);

                    if (IsServerFault(ex))
                        _serverFaultResult = _serverFaultHandler();
                }
            }

            throw new AggregateException(exeptions);
        }

        private static bool IsServerFault(HttpResponseMessage result)
        {
            return new[] { HttpStatusCode.ServiceUnavailable, HttpStatusCode.GatewayTimeout, HttpStatusCode.BadGateway, HttpStatusCode.RequestTimeout }.Any(m => m == result.StatusCode);
        }

        private static bool IsServerFault(Exception ex)
        {
            return false;
        }

        private double GetRetrySeconds()
        {
            if (_maxRetryMinutes > 0)
                return _maxRetries * 60;

            if (_maxRetries <= 0)
                return 1;

            return _maxRetries;
        }
    }
}
