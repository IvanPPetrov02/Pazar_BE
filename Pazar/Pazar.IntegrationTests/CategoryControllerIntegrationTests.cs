using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using BLL.Category_related;
using DAL.DbContexts;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Pazar; // This should match the namespace where your Program class is located

namespace Pazar.IntegrationTests
{
    public class CategoryControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public CategoryControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetCategoryById_ExistingId_ReturnsOk()
        {
            // Arrange
            var categoryId = 1; // Ensure this category ID exists in the test database

            // Act
            var response = await _client.GetAsync($"/api/Category/{categoryId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var category = await response.Content.ReadFromJsonAsync<Category>();
            category.Id.Should().Be(categoryId);
        }

        [Fact]
        public async Task GetCategoryById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var categoryId = 999; // Ensure this category ID does not exist

            // Act
            var response = await _client.GetAsync($"/api/Category/{categoryId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // Additional tests for other endpoints
    }
}