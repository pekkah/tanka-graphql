using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fugu.graphql.samples.chat.data.domain;

namespace fugu.graphql.samples.chat.data
{
    public interface IChat
    {
        Task<IEnumerable<Message>> GetMessagesAsync(int latest);

        Task<Message> AddMessageAsync(
            string fromId,
            string content);
    }

    public class Chat : IChat
    {
        private readonly Queue<Message> _messages = new Queue<Message>();

        private int lastId = 0;

        public Chat()
        {
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
                Id = $"{++lastId}",
                Content = content,
                Timestamp = DateTimeOffset.UtcNow,
                From = from
            };

            _messages.Enqueue(message);

            return message;
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