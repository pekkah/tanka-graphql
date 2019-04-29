using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace tanka.graphql.server.webSockets
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class GraphQLWSProtocolOptions
    {
        private static readonly Task<bool> True = Task.FromResult(true);

        /// <summary>
        ///     Method called when initialize message is received from client to validate
        ///     the connectionParams
        /// </summary>
        /// <returns>true if connection accepted; otherwise false</returns>
        public Func<MessageContext, Task<bool>> Initialize { get; set; } = context => True;
    }
}