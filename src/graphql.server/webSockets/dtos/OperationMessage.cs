using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tanka.graphql.requests;

namespace tanka.graphql.server.webSockets.dtos
{
    [DataContract]
    public class OperationMessage : IEquatable<OperationMessage>
    {
        /// <summary>
        ///     Nullable Id
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        ///     Type <see cref="MessageType" />
        /// </summary>
        [DataMember(Name = "type")]
        public string Type { get; set; }

        /// <summary>
        ///     Nullable payload
        /// </summary>
        [DataMember(Name = "payload")]
        public JObject Payload { get; set; }


        /// <inheritdoc />
        public override string ToString()
        {
            return $"Type: {Type} Id: {Id} Payload: {Payload}";
        }

        public bool Equals(OperationMessage other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Id, other.Id) && string.Equals(Type, other.Type) && Equals(Payload, other.Payload);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OperationMessage) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Id != null ? Id.GetHashCode() : 0);
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

    public class OperationMessageQueryPayload: IEquatable<OperationMessageQueryPayload>
    {
        /// <summary>
        ///     Query, mutation or subscription document
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        ///     Variables
        /// </summary>
        [JsonConverter(typeof(NestedDictionaryConverter))]
        public Dictionary<string, object> Variables { get; set; }

        /// <summary>
        ///     Operation name
        /// </summary>
        public string OperationName { get; set; }

        public bool Equals(OperationMessageQueryPayload other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Query, other.Query) && Equals(Variables, other.Variables) && string.Equals(OperationName, other.OperationName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OperationMessageQueryPayload) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Query != null ? Query.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Variables != null ? Variables.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OperationName != null ? OperationName.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(OperationMessageQueryPayload left, OperationMessageQueryPayload right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(OperationMessageQueryPayload left, OperationMessageQueryPayload right)
        {
            return !Equals(left, right);
        }
    }
}