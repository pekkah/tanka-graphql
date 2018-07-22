using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fugu.graphql.samples.chat.data.domain;
using fugu.graphql.server.subscriptions;
using fugu.graphql.server.utilities;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace fugu.graphql.server.tests.subscriptions.specs
{
    public class ChatSpec : ChatFixture
    {
        public ChatSpec()
        {
            var schema = GetSchemaAsync().Result;
            _transport = new TestableSubscriptionTransport();
            _subscriptions = new SubscriptionManager(
                new SchemaExecutor(schema), 
                new NullLoggerFactory());

            _server = new SubscriptionServer(
                _transport,
                _subscriptions,
                new[] {new ApolloProtocol(new NullLogger<ApolloProtocol>()), },
                new NullLogger<SubscriptionServer>()
            );
        }

        private readonly TestableSubscriptionTransport _transport;
        private readonly SubscriptionManager _subscriptions;
        private readonly SubscriptionServer _server;

        private void AssertReceivedData(List<OperationMessage> writtenMessages, Predicate<JObject> predicate)
        {
            var dataMessages = writtenMessages.Where(m => m.Type == MessageType.GQL_DATA);
            var results = dataMessages.Select(m => m.Payload["Data"] as JObject)
                .ToList();

            Assert.Contains(results, predicate);
        }

        [Fact]
        public async Task Mutate_messages()
        {
            /* Given */
            Chat.AddMember("general", new Member()
            {
                DisplayName = "test",
                Id = "1"
            });

            var id = "1";
            _transport.AddMessageToRead(new OperationMessage
            {
                Id = id,
                Type = MessageType.GQL_CONNECTION_INIT
            });
            _transport.AddMessageToRead(new OperationMessage
            {
                Id = id,
                Type = MessageType.GQL_START,
                Payload = JObject.FromObject(new OperationMessagePayload
                {
                    OperationName = "",
                    Query = @"mutation AddMessage($message: InputMessage!) {
  addMessage(message: $message) {
    from {
      id
      displayName
    }
    content
  }
}",
                    Variables = JObject.Parse(@"{
  ""message"": {
        ""content"": ""Message"",
        ""fromId"": ""1""
    }
}").ToVariableDictionary()
                })
            });
            _transport.AddMessageToRead(new OperationMessage
            {
                Id = id,
                Type = MessageType.GQL_CONNECTION_TERMINATE
            });
            _transport.Complete();

            /* When */
            await _server.OnConnect();
            await _transport.Completion;

            /* Then */
            Assert.Contains(_transport.WrittenMessages,
                message => message.Type == MessageType.GQL_CONNECTION_ACK);
            Assert.Contains(_transport.WrittenMessages, message => message.Type == MessageType.GQL_DATA);
            Assert.Contains(_transport.WrittenMessages, message => message.Type == MessageType.GQL_COMPLETE);
        }

        [Fact]
        public async Task Query_messages()
        {
            /* Given */
            Chat.AddMessage(new Message
            {
                Content = "test",
                From = new Member()
                {
                    DisplayName = "todo",
                    Id = "1"
                },
                SentAt = DateTime.Now.ToShortDateString()
            });

            var id = "1";
            _transport.AddMessageToRead(new OperationMessage
            {
                Id = id,
                Type = MessageType.GQL_CONNECTION_INIT
            });
            _transport.AddMessageToRead(new OperationMessage
            {
                Id = id,
                Type = MessageType.GQL_START,
                Payload = JObject.FromObject(new OperationMessagePayload
                {
                    OperationName = "",
                    Query = @"query AllMessages {
    channels {
        messages {
            content
            sentAt
            from {
                id
                displayName
            }
        }
    }
}"
                })
            });
            _transport.AddMessageToRead(new OperationMessage
            {
                Id = id,
                Type = MessageType.GQL_CONNECTION_TERMINATE
            });
            _transport.Complete();

            /* When */
            await _server.OnConnect();
            await _transport.Completion;

            /* Then */
            Assert.Contains(_transport.WrittenMessages,
                message => message.Type == MessageType.GQL_CONNECTION_ACK);
            Assert.Contains(_transport.WrittenMessages, message => message.Type == MessageType.GQL_DATA);
            Assert.Contains(_transport.WrittenMessages, message => message.Type == MessageType.GQL_COMPLETE);
        }

        [Fact]
        public async Task Subscribe_and_mutate_messages()
        {
            /* Given */
            // subscribe
            var id = "1";
            _transport.AddMessageToRead(new OperationMessage
            {
                Id = id,
                Type = MessageType.GQL_CONNECTION_INIT
            });
            _transport.AddMessageToRead(new OperationMessage
            {
                Id = id,
                Type = MessageType.GQL_START,
                Payload = JObject.FromObject(new OperationMessagePayload
                {
                    OperationName = "",
                    Query = @"subscription MessageAdded {
  messageAdded {
    from { id displayName }
    content
    sentAt
  }
}"
                })
            });

            _transport.AddMessageToRead(new OperationMessage
            {
                Id = id,
                Type = MessageType.GQL_CONNECTION_TERMINATE,
                Payload = null
            });

            /* When */
            var serverTask = _server.OnConnect();
            // post message
            Chat.AddMessage(new Message
            {
                From = new Member()
                {
                    DisplayName = "todo",
                    Id = "1"
                },
                Content = "content",
                SentAt = DateTime.Now.ToShortDateString()
            });

            await serverTask;

            /* Then */
            Assert.Single(_transport.WrittenMessages,
                message => message.Type == MessageType.GQL_CONNECTION_ACK);
            AssertReceivedData(_transport.WrittenMessages, data => data.ContainsKey("messageAdded"));
            Assert.Single(_transport.WrittenMessages, message => message.Type == MessageType.GQL_COMPLETE);
        }
    }
}