﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Tanka.GraphQL.Introspection;
using Tanka.GraphQL.Linking;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.Tests.Data;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem.ValueSerialization;
using Xunit;

namespace Tanka.GraphQL.Tests.Tools
{
    public class MakeRemoteExecutableDigitransitFacts
    {
        private static string GetDigitransitIntrospection()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream("Tanka.GraphQL.Tests.digitransit.introspection");
            using var reader =
                new StreamReader(resourceStream ?? throw new InvalidOperationException(), Encoding.UTF8);
            return reader.ReadToEnd();
        }

        [Fact]
        public async Task ExecuteRemotely()
        {
            /* Given */
            var builder = new SchemaBuilder()
                .Scalar("Long", out _, new StringConverter())
                .Scalar("Lat", out _, new StringConverter())
                .Scalar("Polyline", out _, new StringConverter())
                .ImportIntrospectedSchema(GetDigitransitIntrospection());

            /* When */
            var schema = RemoteSchemaTools.MakeRemoteExecutable(
                builder,
                RemoteLinks.Http(
                    "https://api.digitransit.fi/routing/v1/routers/next-hsl/index/graphql"
                )
            );

            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = schema,
                Document = Parser.ParseDocument(@"
                    {
                        feeds {
                            feedId
                        }
                    }
                    ")
            });

            /* Then */
            result.ShouldMatchJson(@"
              {
              ""data"": {
                ""feeds"": [
                  {
                    ""feedId"": ""HSL""
                  },
                  {
                    ""feedId"": ""HSLlautta""
                  }
                ]
              }
            }
            ");
        }

        [Fact]
        public async Task ExecuteRemotely_with_link_graphql_error()
        {
            /* Given */
            var builder = new SchemaBuilder()
                .Scalar("Long", out _, new StringConverter())
                .Scalar("Lat", out _, new StringConverter())
                .Scalar("Polyline", out _, new StringConverter())
                .ImportIntrospectedSchema(GetDigitransitIntrospection());

            /* When */
            var schema = RemoteSchemaTools.MakeRemoteExecutable(
                builder,
                async (document, variables, cancellationToken) =>
                {
                    var channel = Channel.CreateBounded<ExecutionResult>(1);
                    var executionResult = new ExecutionResult
                    {
                        Errors = new[]
                        {
                            new ExecutionError("failed to find...")
                        }
                    };
                    await channel.Writer.WriteAsync(executionResult, cancellationToken);
                    channel.Writer.TryComplete();

                    return channel;
                });

            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = schema,
                Document = Parser.ParseDocument(@"
                    {
                        feeds {
                            feedId
                        }
                    }
                    ")
            });

            /* Then */
            result.ShouldMatchJson(@"
                {
                  ""data"": {
                    ""feeds"": null
                  },
                  ""errors"": [
                    {
                      ""message"": ""failed to find..."",
                      ""locations"": [
                        {
                          ""end"": 137,
                          ""start"": 47
                        }
                      ],
                      ""path"": [
                        ""feeds""
                      ],
                      ""extensions"": {
                        ""code"": ""QUERYEXECUTION"",
                        ""remoteError"": {
                          ""data"": null,
                          ""errors"": [
                            {
                              ""message"": ""failed to find...""
                            }
                          ],
                          ""extensions"": null
                        }
                      }
                    }
                  ]
                }
            ");
        }

        [Fact]
        public async Task ExecuteWithStaticDataLink()
        {
            /* Given */
            var builder = new SchemaBuilder()
                .Scalar("Long", out _, new StringConverter())
                .Scalar("Lat", out _, new StringConverter())
                .Scalar("Polyline", out _, new StringConverter())
                .ImportIntrospectedSchema(GetDigitransitIntrospection());

            /* When */
            var schema = RemoteSchemaTools.MakeRemoteExecutable(
                builder,
                RemoteLinks.Static(new ExecutionResult
                {
                    Data = new Dictionary<string, object>
                    {
                        ["feeds"] = new Dictionary<string, object>
                        {
                            ["feedId"] = "123"
                        }
                    }
                }));

            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = schema,
                Document = Parser.ParseDocument(@"
                    {
                        feeds {
                            feedId
                        }
                    }
                    ")
            });

            /* Then */
            result.ShouldMatchJson(
                @"
                {
                  ""data"": {
                    ""feeds"": [
                      {
                        ""feedId"": ""123""
                      }
                    ]
                  }
                }
            ");
        }

        [Fact]
        public async Task RemoteExecute_with_link_exception()
        {
            /* Given */
            var builder = new SchemaBuilder()
                .Scalar("Long", out _, new StringConverter())
                .Scalar("Lat", out _, new StringConverter())
                .Scalar("Polyline", out _, new StringConverter())
                .ImportIntrospectedSchema(GetDigitransitIntrospection());

            /* When */
            var schema = RemoteSchemaTools.MakeRemoteExecutable(
                builder,
                (document, variables, cancellationToken) =>
                    throw new InvalidOperationException("error"));

            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = schema,
                Document = Parser.ParseDocument(@"
                    {
                        feeds {
                            feedId
                        }
                    }
                    ")
            });

            /* Then */
            result.ShouldMatchJson(@"
                {
                  ""data"": {
                    ""feeds"": null
                  },
                  ""errors"": [
                    {
                      ""message"": ""error"",
                      ""locations"": [
                        {
                          ""end"": 137,
                          ""start"": 47
                        }
                      ],
                      ""path"": [
                        ""feeds""
                      ],
                      ""extensions"": {
                        ""code"": ""INVALIDOPERATION""
                      }
                    }
                  ]
                }
            ");
        }
    }
}