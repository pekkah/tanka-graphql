using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Tanka.GraphQL.Samples.Chat.Data.Domain;
using Tanka.GraphQL.Subscriptions;

namespace Tanka.GraphQL.Samples.Chat.Data;

public interface IChat
{
    Task<IEnumerable<Message>> GetMessagesAsync(
        int latest);

    Task<Message> AddMessageAsync(
        string fromId,
        string content);

    Task<Message> EditMessageAsync(
        string id,
        string content);

    IAsyncEnumerable<Message> JoinAsync(CancellationToken unsubscribe);
}

public class Chat : IChat
{
    private readonly Queue<Message> _messages = new();
    private readonly BroadcastChannel<Message> _broadcast;

    private Channel<Message> _messageChannel = Channel.CreateUnbounded<Message>();
    private int _lastId;

    public Chat()
    {
        _broadcast = new BroadcastChannel<Message>(_messageChannel);
    }

    public async Task<IEnumerable<Message>> GetMessagesAsync(int latest)
    {
        await Task.Delay(0);
        return _messages.Take(latest);
    }

    public async Task<Message> AddMessageAsync(
        string fromId,
        string content)
    {
        var from = await GetFromAsync(fromId);
        var message = new Message
        {
            Id = $"{++_lastId}",
            Content = content,
            Timestamp = DateTimeOffset.UtcNow,
            From = from
        };

        _messages.Enqueue(message);
        await _messageChannel.Writer.WriteAsync(message);
        return message;
    }

    public async Task<Message> EditMessageAsync(string id, string content)
    {
        await Task.Delay(0);
        var originalMessage = _messages.SingleOrDefault(m => m.Id == id);

        if (originalMessage == null)
            return null;

        originalMessage.Content = content;
        return originalMessage;
    }

    public IAsyncEnumerable<Message> JoinAsync(CancellationToken unsubscribe)
    {
        return _broadcast.Subscribe(unsubscribe);
    }

    private async Task<From> GetFromAsync(string fromId)
    {
        await Task.Delay(0);
        return new From
        {
            UserId = fromId,
            Name = "From"
        };
    }
}