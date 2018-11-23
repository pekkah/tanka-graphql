using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using fugu.graphql.samples.chat.data.domain;

namespace fugu.graphql.samples.chat.data
{
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

        Task<ChannelReader<object>> JoinAsync(CancellationToken unsubscribe);
    }

    public class Chat : IChat
    {
        private readonly Queue<Message> _messages = new Queue<Message>();
        private readonly List<Channel<object>> _channels = new List<Channel<object>>();

        private int _lastId;

        public async Task<IEnumerable<Message>> GetMessagesAsync(int latest)
        {
            await Task.Delay(0);
            return _messages.Take(latest);
        }

        public async Task<Message> AddMessageAsync(
            string fromId,
            string content)
        {
            await Task.Delay(0);
            var from = await GetFromAsync(fromId);
            var message = new Message
            {
                Id = $"{++_lastId}",
                Content = content,
                Timestamp = DateTimeOffset.UtcNow,
                From = from
            };

            _messages.Enqueue(message);

            foreach (var channel in _channels)
            {
                await channel.Writer.WriteAsync(message);
            }

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

        public Task<ChannelReader<object>> JoinAsync(CancellationToken unsubscribe)
        {
            var channel = Channel.CreateUnbounded<object>();
            _channels.Add(channel);
            unsubscribe.Register(() => { Leave(channel); });

            return Task.FromResult(channel.Reader);
        }

        private void Leave(Channel<object> channel)
        {
            _channels.Remove(channel);
            channel.Writer.Complete();
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
}