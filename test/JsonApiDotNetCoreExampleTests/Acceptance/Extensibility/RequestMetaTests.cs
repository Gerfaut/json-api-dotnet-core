using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using JsonApiDotNetCoreExample.Models;
using DotNetCoreDocs;
using JsonApiDotNetCoreExample;
using DotNetCoreDocs.Writers;
using Newtonsoft.Json;
using JsonApiDotNetCore.Models;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using JsonApiDotNetCoreExampleTests.Startups;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Extensibility
{
    [Collection("WebHostCollection")]
    public class RequestMetaTests
    {
        private DocsFixture<Startup, JsonDocWriter> _fixture;

        public RequestMetaTests(DocsFixture<Startup, JsonDocWriter> fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Injecting_IRequestMeta_Adds_Meta_Data()
        {
            // arrange
            var person = new Person();
            var expectedMeta = person.GetMeta(null);
            var builder = new WebHostBuilder()
                .UseStartup<MetaStartup>();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/people";

            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var documents = JsonConvert.DeserializeObject<Documents>(await response.Content.ReadAsStringAsync());
            
            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(documents.Meta);
            Assert.NotNull(expectedMeta);
            Assert.NotEmpty(expectedMeta);
            
            foreach(var hash in expectedMeta)
            {
                if(hash.Value is IList)
                {
                    var listValue = (IList)hash.Value;
                    for(var i=0; i < listValue.Count; i++)
                        Assert.Equal(listValue[i].ToString(), ((IList)documents.Meta[hash.Key])[i].ToString());
                }
                else
                {
                    Assert.Equal(hash.Value, documents.Meta[hash.Key]);
                }
            }
            Assert.Equal("request-meta-value", documents.Meta["request-meta"]);
        }
    }
}
