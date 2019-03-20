using System;

namespace tanka.graphql.samples.chat.data.domain
{
    public class From
    {
        public string UserId { get; set; }

        public string Name { get; set; }
    }

    public class Message
    {
        public string Id { get; set; }

        public From From { get; set; }

        public string Content { get; set; }

        public DateTimeOffset Timestamp { get; set; }
    }

    public class InputMessage
    {
        public string Content { get; set; }
    }
}