﻿

using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.TypeSystem.ValueSerialization
{
    /// <summary>
    ///     See http://facebook.github.io/graphql/#sec-Scalars
    /// </summary>
    /// <typeparam name="I"></typeparam>
    /// <typeparam name="O"></typeparam>
    public interface IValueConverter
    {
        /// <summary>
        ///     Serialize input value into actual value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        object? Serialize(object? value);

        /// <summary>
        ///     Serialize input value into printable value node
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        ValueBase SerializeLiteral(object? value);

        /// <summary>
        ///     Parse query variable into actual value
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        object? ParseValue(object? input);

        /// <summary>
        ///     Parse input value into actual value
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        object? ParseLiteral(ValueBase input);
    }
}