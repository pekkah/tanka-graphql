using System;
using System.Collections.Generic;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Samples.Chat.Data.Domain;

public class From
{
    public string Name { get; set; }
    public string UserId { get; set; }
}

public class Message
{
    public string Content { get; set; }

    public From From { get; set; }
    public string Id { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}

public class InputMessage : IReadFromObjectDictionary
{
    public string Content { get; set; }

    public void Read(IReadOnlyDictionary<string, object> source)
    {
        Content = source.GetValue<string>("content");
    }
}