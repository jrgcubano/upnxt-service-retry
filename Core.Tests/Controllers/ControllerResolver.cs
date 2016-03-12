using System.Collections.Generic;
using System.Reflection;
using System.Web.Http.Dispatcher;

namespace Core.Tests.Controllers
{
    public class ControllerResolver : DefaultAssembliesResolver
    {
        public override ICollection<Assembly> GetAssemblies()
        {
            return new List<Assembly> { typeof(Controllers.TestController).Assembly };
        }
    }
}
