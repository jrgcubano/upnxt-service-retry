
namespace Core
{
    public interface IRetryUnit : IExecuteServiceCall
    {
        IRetryUnit Times();
        IRetryUnit Seconds();
        IRetryUnit Minutes();

        /// <summary>
        /// Exponentially delayed retry for a maximum of x seconds set
        /// </summary>
        /// <returns></returns>
        IRetryUnit BackOff();
    }
}
