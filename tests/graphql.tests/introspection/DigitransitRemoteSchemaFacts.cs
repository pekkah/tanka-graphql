using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Newtonsoft.Json;
using tanka.graphql.introspection;
using tanka.graphql.language;
using tanka.graphql.tests.data;
using tanka.graphql.tools;
using tanka.graphql.type;
using tanka.graphql.type.converters;
using Xunit;

namespace tanka.graphql.tests.introspection
{
    public class DigitransitRemoteSchemaFacts
    {
        private static string GetDigitransitIntrospection()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream("tanka.graphql.tests.digitransit.introspection");
            using (var reader =
                new StreamReader(resourceStream ?? throw new InvalidOperationException(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        [Fact]
        public async Task MakeRemoteExecutable()
        {
            /* Given */
            var builder = new SchemaBuilder()
                .Scalar("Long", out _, new StringConverter())
                .Scalar("Lat", out _, new StringConverter())
                .Scalar("Polyline", out _, new StringConverter())
                .ImportIntrospectedSchema(GetDigitransitIntrospection());

            var mockResult = new ExecutionResult()
            {
                Data = new Dictionary<string, object>()
                {
                    ["feeds"] = new Dictionary<string, object>()
                    {
                        ["feedId"] ="123"
                    }
                }
            };
            var resultChannel = Channel.CreateBounded<ExecutionResult>(1);
            await resultChannel.Writer.WriteAsync(mockResult);
            resultChannel.Writer.TryComplete();

            /* When */
            var schema = RemoteSchemaTools.MakeRemoteExecutable(
                builder,
                link: (document, variables, cancellationToken) => 
                    new ValueTask<ChannelReader<ExecutionResult>>(resultChannel));

            var result = await Executor.ExecuteAsync(new ExecutionOptions()
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
            result.ShouldMatchJson(@"{
              ""data"": {
                ""feeds"": {
                  ""feedId"": ""123""
                }
              }
            }
            ");
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
                link: async (document, variables, cancellationToken) =>
                {
                    var http = new HttpClient();
                    var url = "https://api.digitransit.fi/routing/v1/routers/next-hsl/index/graphql";
                    var query = document.ToGraphQL();
                    var request = new
                    {
                        query
                    };
                    var response = await http.PostAsync(
                        url,
                        new StringContent(
                            JsonConvert.SerializeObject(request), 
                            Encoding.UTF8, 
                            "application/json"), 
                        cancellationToken);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var executionResult = JsonConvert.DeserializeObject<ExecutionResult>(responseContent);
                    var channel = Channel.CreateBounded<ExecutionResult>(1);
                    await channel.Writer.WriteAsync(executionResult, cancellationToken);
                    channel.Writer.TryComplete();

                    return channel;
                });

            var result = await Executor.ExecuteAsync(new ExecutionOptions()
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
        public async Task MakeRemoteExecutable_on_link_error()
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
                link: (document, variables, cancellationToken) => 
                    throw new InvalidOperationException("error"));

            var result = await Executor.ExecuteAsync(new ExecutionOptions()
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
                      ""extensions"": {
                        ""code"": ""INVALIDOPERATION""
                      }
                    }
                  ]
                }
            ");
        }

        [Fact]
        public async Task MakeRemoteExecutable_on_link_graphql_error()
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
                link: async (document, variables, cancellationToken) =>
                {
                    var channel = Channel.CreateBounded<ExecutionResult>(1);
                    var executionResult = new ExecutionResult()
                    {
                        Errors = new[]
                        {
                            new Error("failed to find..."),
                        }
                    };
                    await channel.Writer.WriteAsync(executionResult, cancellationToken);
                    channel.Writer.TryComplete();

                    return channel;
                });

            var result = await Executor.ExecuteAsync(new ExecutionOptions()
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
                        ""code"": ""COMPLETEVALUE""
                      }
                    }
                  ]
                }
            ");
        }

        [Fact]
        public void Read_types()
        {
            /* Given */
            var builder = new SchemaBuilder();
            builder.Scalar("Long", out _, new StringConverter());
            builder.Scalar("Lat", out _, new StringConverter());
            builder.Scalar("Polyline", out _, new StringConverter());

            /* When */
            builder.ImportIntrospectedSchema(GetDigitransitIntrospection());

            /* Then */
            Assert.True(builder.TryGetType<ObjectType>("Query", out var query));
        }
    }
}