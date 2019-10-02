﻿using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.DTOs;
using Tanka.GraphQL.Introspection;
using Tanka.GraphQL.SchemaBuilding;

namespace Tanka.GraphQL.Server.Links
{
    public static class LinkingSchemaBuilderExtensions
    {
        /// <summary>
        ///     Execute <see cref="Introspect.DefaultQuery" /> on link
        ///     and import the schema
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="link">Execution link</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<SchemaBuilder> ImportIntrospectedSchema(
            this SchemaBuilder builder,
            ExecutionResultLink link,
            CancellationToken cancellationToken = default)
        {
            if (link == null) throw new ArgumentNullException(nameof(link));

            var channel = await link(
                Parser.ParseDocument(Introspect.DefaultQuery),
                null,
                cancellationToken);

            if (channel == null)
                throw new InvalidOperationException(
                    "Failed to execute introspection query. Link returned a null channel.");

            var result = await channel.ReadAsync(cancellationToken);

            if (result == null)
                throw new InvalidOperationException(
                    "Failed to execute introspection query. Link channel read result is null");

            if (result.Errors != null && result.Errors.Any())
                throw new InvalidOperationException(
                    "Failed to execute introspection query. " +
                    $"Errors: {string.Join(", ", result.Errors.Select(e => e.Message))}");

            var json = DefaultJsonSerializer.Serializer.Serialize(result);
            return builder.ImportIntrospectedSchema(Encoding.UTF8.GetString(json));
        }
    }
}