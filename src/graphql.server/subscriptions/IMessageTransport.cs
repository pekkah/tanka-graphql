using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace fugu.graphql.server.subscriptions
{
    /// <summary>
    ///     Transport defining the source of the data Reader
    ///     and target of the data Writer
    /// </summary>
    public interface IMessageTransport
    {
        /// <summary>
        ///     Pipeline from which the messages are read
        /// </summary>
        ISourceBlock<OperationMessage> Reader { get; }

        /// <summary>
        ///     Pipeline to which the messages are written
        /// </summary>
        ITargetBlock<OperationMessage> Writer { get; }

        void Complete();

        Task Completion { get; }
    }
}