using System.Threading.Tasks;

namespace fugu.graphql.server.subscriptions
{
    /// <summary>
    ///     QueryOperation message listener
    /// </summary>
    public interface IOperationMessageListener
    {
        Task BeforeHandleAsync(MessageHandlingContext context);

        /// <summary>
        ///     Called to handle message
        /// </summary>
        /// <returns></returns>
        Task HandleAsync(MessageHandlingContext context);

        /// <summary>
        ///     Called after message has been handled
        /// </summary>
        /// <returns></returns>
        Task AfterHandleAsync(MessageHandlingContext context);
    }
}