using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using BLL.Category_related;
using DAL.DbContexts;
using Pazar;

namespace Pazar.IntegrationTests
{
    internal class CategoryControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public CategoryControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetCategoryById_ExistingId_ReturnsOk()
        {
            var categoryId = 1;

            var response = await _client.GetAsync($"/api/Category/{categoryId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var category = await response.Content.ReadFromJsonAsync<Category>();
            category.Id.Should().Be(categoryId);
        }

        [Fact]
        public async Task GetCategoryById_NonExistingId_ReturnsNotFound()
        {
            var categoryId = 999;

            var response = await _client.GetAsync($"/api/Category/{categoryId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}