using Beauvoir.Controllers;
using Beauvoir.DTO;
using Beauvoir.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.Collections.Generic;
using System;

namespace Beauvoir.Tests
{
    public class AuthControllerTest
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Asegura una base limpia por test
                .Options;

            return new AppDbContext(options);
        }

        private IConfiguration GetFakeConfig()
        {
            var config = new Dictionary<string, string>
            {
                { "JWT:SecureKey", "TEST_SECRET_KEY_12345678901234567890" }
            };
            return new ConfigurationBuilder().AddInMemoryCollection(config).Build();
        }

        [Fact]
        public void Register_ReturnsBadRequest_WhenUserDtoIsNull()
        {
            // Arrange
            var dbContext = GetDbContext();
            var controller = new AuthController(GetFakeConfig(), dbContext);

            // Act
            var result = controller.Register(null);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("User data is required.", badRequest.Value); // Ajusta mensaje si es necesario
        }

        [Fact]
        public void Register_CreatesUser_WhenValidDataProvided()
        {
            var dbContext = GetDbContext();
            var controller = new AuthController(GetFakeConfig(), dbContext);

            var dto = new RegisterDto
            {
                Username = "testuser",
                Password = "password123",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };

            var result = controller.Register(dto);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUser = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal("testuser", returnedUser.Username);
        }

        [Fact]
        public void Register_ReturnsBadRequest_WhenUsernameAlreadyExists()
        {
            var dbContext = GetDbContext();
            dbContext.Users.Add(new User
            {
                Username = "existinguser",
                Email = "already@exists.com",
                FirstName = "Test",
                LastName = "User",
                PwdHash = "hash",
                PwdSalt = "salt"
            });
            dbContext.SaveChanges();

            var controller = new AuthController(GetFakeConfig(), dbContext);

            var dto = new RegisterDto
            {
                Username = "existinguser",
                Password = "password123",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };

            var result = controller.Register(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Username existinguser already exists", badRequest.Value); // Ajusta mensaje si aplica
        }

    }
}
