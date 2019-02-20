using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace tanka.graphql.tests
{
    public class EventsModel
    {
        public enum EventType
        {
            INSERT,
            UPDATE,
            DELETE
        }

        public EventsModel()
        {
            Events = new List<Event>();
        }

        public List<Event> Events { get; set; }

        public BroadcastBlock<Event> Broadcast { get; set; } =
            new BroadcastBlock<Event>(ev => ev);

        public int LastId { get; set; } = 0;

        public async Task<int> AddAsync(NewEvent newEvent)
        {
            LastId++;
            var ev = new Event
            {
                Id = LastId,
                Type = newEvent.Type,
                Payload = newEvent.Payload
            };
            Events.Add(ev);
            await Broadcast.SendAsync(ev);

            return LastId;
        }

        public ISourceBlock<object> Subscribe(CancellationToken unsubscribe)
        {
            var source = new BufferBlock<Event>();
            var unlink = Broadcast.LinkTo(source);

            unsubscribe.Register(() => unlink.Dispose());

            return source;
        }

        public class Success
        {
            public Success(int id)
            {
                Id = id;
            }

            public int Id { get; set; }
        }

        public class Failure
        {
            public Failure(string message)
            {
                Message = message;
            }

            public string Message { get; set; }
        }

        public class Event
        {
            public int Id { get; set; }

            public EventType Type { get; set; }

            public string Payload { get; set; }
        }

        public class NewEvent
        {
            public EventType Type { get; set; }

            public string Payload { get; set; }
        }
    }
}