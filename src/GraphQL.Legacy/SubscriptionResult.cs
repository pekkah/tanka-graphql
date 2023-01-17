using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;

namespace Tanka.GraphQL;

/// <summary>
///     Result of executing a subscription
/// </summary>
public class SubscriptionResult : IExecutionResult
{
    private List<ExecutionError> _errors;
    private Dictionary<string, object> _extensions;

    public SubscriptionResult(Channel<ExecutionResult> source)
    {
        Source = source;
    }

    public SubscriptionResult()
    {
    }

    public Channel<ExecutionResult> Source { get; }


    public Dictionary<string, object> Extensions
    {
        get => _extensions;
        set
        {
            if (value != null && !value.Any())
            {
                _extensions = null;
                return;
            }

            _extensions = value;
        }
    }

    public List<ExecutionError> Errors
    {
        get => _errors;
        set
        {
            if (value != null)
                if (!value.Any())
                {
                    _errors = null;
                    return;
                }

            _errors = value;
        }
    }
}