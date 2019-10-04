using System;
using System.Collections.Generic;

namespace Tanka.GraphQL.Server.WebSockets.DTOs
{
    public class OperationMessageQueryPayload : IEquatable<OperationMessageQueryPayload>
    {
        /// <summary>
        ///     Query, mutation or subscription document
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        ///     Variables
        /// </summary>
        public Dictionary<string, object> Variables { get; set; }

        /// <summary>
        ///     Operation name
        /// </summary>
        public string OperationName { get; set; }

        public Dictionary<string, object> Extensions { get; set; }

        public bool Equals(OperationMessageQueryPayload other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Query, other.Query) && Equals(Variables, other.Variables) &&
                   string.Equals(OperationName, other.OperationName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((OperationMessageQueryPayload) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Query != null ? Query.GetHashCode() : 0;
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