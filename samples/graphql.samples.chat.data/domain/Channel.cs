using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using fugu.graphql.samples.chat.data.domain;

namespace fugu.graphql.samples.chat.data.domain
{
    public class Channel
    {
        private List<Message> _messages;
        private BufferBlock<Message> _messageBuffer;

        public Channel()
        {
            _messages = new List<Message>();
            _messageBuffer = new BufferBlock<Message>();
        }

        public string Name { get; set; }

        public List<Member> Members { get; set; } = new List<Member>();

        public IEnumerable<Message> Messages => _messages;

        public ISourceBlock<Message> Join(Member member)
        {
            var buffer = new BufferBlock<Message>();
            _messageBuffer.LinkTo(buffer, new DataflowLinkOptions()
            {
                PropagateCompletion = true
            });

            Members.Add(member);
            return buffer;
        }

        public void AddMessage(Message message)
        {
            _messages.Add(message);
            _messageBuffer.Post(message);
        }

        public void Leave(Member member)
        {
            Members.Remove(member);
        }
    }
}