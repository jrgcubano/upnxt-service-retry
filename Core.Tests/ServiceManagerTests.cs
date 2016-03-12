using Core.Contracts;
using Core.Tests.Controllers;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;

namespace Core.Tests
{
    [TestClass]
    public class ServiceManagerTests
    {
        class OwinConfig
        {
            public void Configuration(IAppBuilder app)
            {
                HttpConfiguration config = new HttpConfiguration();
                config.Services.Replace(typeof(IAssembliesResolver), new ControllerResolver());
                config.MapHttpAttributeRoutes();
                app.UseWebApi(config);
            }
        }

        [TestMethod]
        public async Task service_get_service_200()
        {
            var service = SendRequest((client) =>
            {
                return new ServiceManager().Call(() =>
                {
                    var response = client.GetAsync("http://testserver/api/test").GetAwaiter().GetResult();
                    return response;
                }).Execute();
            });

            Assert.AreEqual("service-test", service.Content.ReadAsAsync<ServiceInfo>().GetAwaiter().GetResult().Name);
        }

        [TestMethod]
        public async Task service_get_service_404()
        {
            var service = SendRequest((client) =>
            {
                return new ServiceManager().Call(() =>
                {
                    var response = client.GetAsync("http://testserver/api/test-404").GetAwaiter().GetResult();
                    return response;
                })
                .OnFailRetryFor(1).Minutes()
                .Execute();
            });

            Assert.AreEqual(HttpStatusCode.NotFound, service.StatusCode);
        }

        [TestMethod]
        public async Task service_get_service_500()
        {
            var service = SendRequest((client) =>
            {
                return new ServiceManager().Call(() =>
                {
                    var response = client.GetAsync("http://testserver/api/test/broken").GetAwaiter().GetResult();
                    return response;
                })
                .OnFailRetryFor(3).Times()
                .Execute();
            });

            Assert.AreEqual(HttpStatusCode.InternalServerError, service.StatusCode);
        }

        [TestMethod]
        public async Task service_get_service_timeout()
        {
            try
            {
                var service = SendRequest((client) =>
                {
                    client.Timeout = TimeSpan.FromSeconds(1);

                    return new ServiceManager().Call(() =>
                    {
                        var response = client.GetAsync("http://testserver/api/test/timeout").GetAwaiter().GetResult();
                        return response;
                    })
                    .OnFailRetryFor(2).Times()
                    .Execute();
                });
            }
            catch (Exception ex)
            {
                Assert.AreEqual(2, ((AggregateException)ex).InnerExceptions.Count);
            }
        }

        private HttpResponseMessage SendRequest(Func<HttpClient, HttpResponseMessage> endpoint)
        {
            using (var server = TestServer.Create<OwinConfig>())
            using (var client = new HttpClient(server.Handler))
                return endpoint(client);
        }
    }
}
