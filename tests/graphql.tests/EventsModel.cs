using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;


namespace Tanka.GraphQL.Tests;

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

    public Channel<Event> Broadcast { get; set; } = Channel.CreateUnbounded<Event>();

    public List<Event> Events { get; set; }

    public int LastId { get; set; }

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
        await Broadcast.Writer.WriteAsync(ev);

        return LastId;
    }

    public IAsyncEnumerable<object?> Subscribe(CancellationToken unsubscribe)
    {
        return Broadcast.Reader.ReadAllAsync(unsubscribe);
    }

    public class Event
    {
        public int Id { get; set; }

        public string Payload { get; set; }

        public EventType Type { get; set; }
    }

    public class Failure
    {
        public Failure(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }

    public class NewEvent
    {
        public string Payload { get; set; }
        public EventType Type { get; set; }

    }

    public class Success
    {
        public Success(int id, Event ev)
        {
            Id = id;
            Event = ev;
        }

        public Event Event { get; }

        public int Id { get; set; }
    }
}