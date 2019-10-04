using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Channels;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Tests
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

        public EventChannel<Event> Broadcast { get; set; } =
            new EventChannel<Event>();

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
            await Broadcast.WriteAsync(ev);

            return LastId;
        }

        public ISubscribeResult Subscribe(CancellationToken unsubscribe)
        {
            return Broadcast.Subscribe(unsubscribe);
        }

        public class Success
        {
            public Success(int id, Event ev)
            {
                Id = id;
                Event = ev;
            }

            public int Id { get; set; }
            public Event Event { get; }
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

        public class NewEvent : IReadFromObjectDictionary
        {
            public EventType Type { get; set; }

            public string Payload { get; set; }

            public void Read(IReadOnlyDictionary<string, object> source)
            {
                Type = (EventType)Enum.Parse(typeof(EventType), source.GetValue<string>("type"));
                Payload = source.GetValue<string>("payload");
            }
        }
    }
}