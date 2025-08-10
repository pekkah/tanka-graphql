using System;
#if !NET5_0_OR_GREATER
using System.Runtime.Serialization;
#endif

namespace Tanka.GraphQL.Language;

/// <summary>
/// Exception thrown when parsing GraphQL documents fails
/// </summary>
#if !NET5_0_OR_GREATER
[Serializable]
#endif
public class ParseException : Exception
{
    public ParseException(string message) : base(message)
    {
    }

    public ParseException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ParseException(string message, int line, int column) : base(message)
    {
        Line = line;
        Column = column;
    }

#if !NET5_0_OR_GREATER
    protected ParseException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
        Line = (int?)info.GetValue(nameof(Line), typeof(int?));
        Column = (int?)info.GetValue(nameof(Column), typeof(int?));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(Line), Line);
        info.AddValue(nameof(Column), Column);
    }
#endif

    /// <summary>
    /// Line number where the parsing error occurred
    /// </summary>
    public int? Line { get; }

    /// <summary>
    /// Column number where the parsing error occurred
    /// </summary>
    public int? Column { get; }
}