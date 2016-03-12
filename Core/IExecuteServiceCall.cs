using System.Net.Http;

namespace Core
{
    public interface IExecuteServiceCall
    {
        HttpResponseMessage Execute();
    }
}
