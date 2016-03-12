using Core.Contracts;
using System;
using System.Threading;
using System.Web.Http;

namespace Core.Tests.Controllers
{
    [RoutePrefix("api/test")]
    public class TestController : ApiController
    {
        [Route("")]
        public IHttpActionResult Get()
        {
            return Ok(new ServiceInfo { Name = "service-test" });
        }

        [Route("broken")]
        public IHttpActionResult GetBroken()
        {
            throw new ApplicationException("It's broke sucka!");
        }

        [Route("timeout")]
        public IHttpActionResult GetTimeout()
        {
            Thread.Sleep(60 * 1000);
            return Ok();
        }
    }
}
