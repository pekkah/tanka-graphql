namespace fugu.graphql.samples.chat.data.domain
{
    public class InputMessage
    {
        public string Content { get; set; }

        public string FromId { get; set; }

        public string SentAt { get; set; }

        public string ChannelName { get; set; }
    }
}