using JsonApiDotNetCore.Serialization;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NoEntityFrameworkExample;
using System.Net.Http;
using JsonApiDotNetCoreExampleTests.Helpers.Extensions;
using System.Threading.Tasks;
using System.Net;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCoreExample.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace JsonApiDotNetCoreExampleTests.Acceptance.Extensibility
{
    public class NoEntityFrameworkTests
    {
        private readonly TestServer _server;
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public NoEntityFrameworkTests()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            _server = new TestServer(builder);
            _config = _server.GetService<IConfiguration>();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(_config.GetValue<string>("Data:DefaultConnection"));
            _context = new AppDbContext(optionsBuilder.Options);
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task Can_Implement_Custom_IResourceService_Without_EFAsync()
        {
            // arrange
            _context.TodoItems.Add(new TodoItem());
            _context.SaveChanges();

            var client = _server.CreateClient();

            var httpMethod = new HttpMethod("GET");
            var route = $"/api/v1/custom-todo-items";

            var request = new HttpRequestMessage(httpMethod, route);

            // act
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var deserializedBody = _server.GetService<IJsonApiDeSerializer>()
                .DeserializeList<TodoItem>(responseBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(deserializedBody);
            Assert.NotEmpty(deserializedBody);
        }
    }
}