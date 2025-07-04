using Beauvoir.Controllers;
using Beauvoir.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Xunit;
using System;
using System.Linq;

namespace Beauvoir.Tests
{
    public class FriendshipControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private ClaimsPrincipal GetMockUser(int userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));
        }

        [Fact]
        public void SendRequest_CreatesFriendRequest_WhenValid()
        {
            var dbContext = GetInMemoryDbContext();

            var currentUser = new User
            {
                Id = 1,
                Username = "user1",
                Email = "user1@example.com",
                FirstName = "First1",
                LastName = "Last1",
                PwdHash = "testhash",
                PwdSalt = "testsalt"
            };

            var targetUser = new User
            {
                Id = 2,
                Username = "user2",
                Email = "user2@example.com",
                FirstName = "First2",
                LastName = "Last2",
                PwdHash = "testhash",
                PwdSalt = "testsalt"
            };

            dbContext.Users.AddRange(currentUser, targetUser);
            dbContext.SaveChanges();

            var controller = new FriendshipController(dbContext)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = GetMockUser(currentUser.Id) }
                }
            };

            var result = controller.SendRequest(targetUser.Id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Solicitud de amistad enviada.", okResult.Value);

            var friendship = dbContext.Friendships
                .FirstOrDefault(f => f.RequesterId == currentUser.Id && f.ReceiverId == targetUser.Id);

            Assert.NotNull(friendship);
            Assert.Equal("Pending", friendship.Status);
        }

    }
}