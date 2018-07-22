using System;
using System.Collections.Generic;
using System.Linq;
using fugu.graphql.samples.chat.data.domain;

namespace fugu.graphql.samples.chat.data.domain
{
    public class Chat
    {
        private readonly List<Channel> _channels;

        public Chat()
        {
            _channels = new List<Channel>();

            AddChannel("general");
            AddChannel("random");
        }

        public IEnumerable<Channel> Channels => _channels;

        public void AddChannel(string name)
        {
            _channels.Add(new Channel
            {
                Name = name
            });
        }

        public void AddMessage(Message message)
        {
            var channelName = message.ChannelName ?? "general";
            var channel = GetChannel(channelName);

            channel?.AddMessage(message);
        }

        public Channel GetChannel(string name)
        {
            var channel = Channels.SingleOrDefault(c => c.Name == name);
            return channel;
        }

        public IEnumerable<Message> GetMessages(Channel channel)
        {
            return channel.Messages;
        }

        public Message AddReceivedMessage(InputMessage inputMessage)
        {
            var channelName = inputMessage.ChannelName ?? "general";
            var channel = GetChannel(channelName);

            var member = channel?.Members.SingleOrDefault(m => m.Id == inputMessage.FromId);

            if (member == null)
                return null;

            var message = new Message()
            {
                ChannelName = channelName,
                Content = inputMessage.Content,
                From = member,
                SentAt = inputMessage.SentAt ?? DateTime.UtcNow.ToShortDateString()
            };
            channel.AddMessage(message);
            return message;
        }

        public void AddMember(string channelName, Member member)
        {
            var channel = GetChannel(channelName);
            channel.Members.Add(member);
        }
    }
}