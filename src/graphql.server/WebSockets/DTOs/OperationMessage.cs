using System;

namespace Tanka.GraphQL.Server.WebSockets.DTOs;

public class OperationMessage : IEquatable<OperationMessage>
{
    /// <summary>
    ///     Nullable Id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     Nullable payload
    /// </summary>
    public object Payload { get; set; }

    /// <summary>
    ///     Type <see cref="MessageType" />
    /// </summary>
    public string Type { get; set; }

    public bool Equals(OperationMessage other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(Id, other.Id) && string.Equals(Type, other.Type) && Equals(Payload, other.Payload);
    }


    /// <inheritdoc />
    public override string ToString()
    {
        return $"Type: {Type} Id: {Id} Payload: {Payload}";
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((OperationMessage)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Id != null ? Id.GetHashCode() : 0;
            hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Payload != null ? Payload.GetHashCode() : 0);
            return hashCode;
        }
    }

    public static bool operator ==(OperationMessage left, OperationMessage right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(OperationMessage left, OperationMessage right)
    {
        return !Equals(left, right);
    }
}