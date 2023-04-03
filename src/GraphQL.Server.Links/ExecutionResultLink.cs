using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Server.Links;

public delegate ValueTask<ChannelReader<ExecutionResult>> ExecutionResultLink(
    ExecutableDocument document,
    IReadOnlyDictionary<string, object> variables,
    CancellationToken cancellationToken);