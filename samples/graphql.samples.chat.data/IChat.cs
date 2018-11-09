using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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

        Task<IDisposable> JoinAsync(ITargetBlock<Message> target);
    }

    public class Chat : IChat
    {
        private readonly Queue<Message> _messages = new Queue<Message>();
        private readonly BroadcastBlock<Message> _messageStream;

        private int _lastId;

        public Chat()
        {
            _messageStream = new BroadcastBlock<Message>(original => original);
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
            await _messageStream.SendAsync(message);
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

        public Task<IDisposable> JoinAsync(ITargetBlock<Message> target)
        {
            var sub = _messageStream.LinkTo(target, new DataflowLinkOptions()
            {
                PropagateCompletion = true
            });

            return Task.FromResult(sub);
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