using System;
using System.Collections.Generic;
using System.Text;
using Tanka.GraphQL.DTOs;
using Xunit;

namespace Tanka.GraphQL.Tests.DTOs
{
    public class SerializerFacts
    {
        private Serializer _sut;

        public SerializerFacts()
        {
            _sut = new Serializer();
        }

        [Fact]
        public void Serialize_QueryRequest()
        {
            /* Given */
            var request = new QueryRequest()
            {
                Query = "{ name }",
                OperationName = "operation",
                Variables = new Dictionary<string, object>()
                {
                    ["id"] = 0,
                    ["filter"] = new Dictionary<string, object>()
                    {
                        ["property"] = "test",
                        ["direction"] = "ASC"
                    }
                }
            };

            /* When */
            var bytes = _sut.Serialize(request);
            var json = Encoding.UTF8.GetString(bytes);

            /* Then */
            Assert.Equal(
                @"{""query"":""{ name }"",""variables"":{""id"":0,""filter"":{""property"":""test"",""direction"":""ASC""}},""operationName"":""operation"",""extensions"":null}",
                json);
        }

        [Fact]
        public void Deserialize_QueryRequest()
        {
           
            /* Given */
            var json = @"{""query"":""{ name }"",""variables"":{""id"":0,""filter"":{""property"":""test"",""direction"":""ASC""}},""operationName"":""operation"",""extensions"":null}";
            /*var request = new QueryRequest()
            {
                Query = "{ name }",
                OperationName = "operation",
                Variables = new Dictionary<string, object>()
                {
                    ["id"] = 0,
                    ["filter"] = new Dictionary<string, object>()
                    {
                        ["property"] = "test",
                        ["direction"] = "ASC"
                    }
                }
            };*/

            /* When */
            var bytes = Encoding.UTF8.GetBytes(json);
            var request = _sut.Deserialize<QueryRequest>(bytes);

            /* Then */
            Assert.Equal("{ name }", request.Query);
            var filter = (KeyValuePair<string, object>)request.Variables["filter"];
            var filterDict = filter.Value as Dictionary<string, object>;
            Assert.Equal("test", filterDict["property"]);
        }
    }
}
