using Beauvoir.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace Beauvoir.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FriendshipController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public FriendshipController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/friendship/list
        [HttpGet("list")]
        public ActionResult List()
        {
            var userId = GetUserId();

            // Obtener todos los friendships aceptados donde soy requester o receiver
            var friendships = _dbContext.Friendships
                .Where(f => (f.RequesterId == userId || f.ReceiverId == userId) && f.Status == "Accepted")
                .ToList();

            // Obtener IDs de los amigos (el que no soy yo)
            var friendIds = friendships.Select(f => f.RequesterId == userId ? f.ReceiverId : f.RequesterId).ToList();

            // Obtener datos básicos de esos amigos (nombre, email, etc) - según User model
            var friends = _dbContext.Users
                .Where(u => friendIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.FirstName,
                    u.LastName,
                    u.Email
                })
                .ToList();

            return Ok(friends);
        }
        [HttpGet("requests")]
        public ActionResult GetPendingRequests()
        {
            var userId = GetUserId();

            var requests = _dbContext.Friendships
                .Where(f => f.ReceiverId == userId && f.Status == "Pending")
                .Join(_dbContext.Users,
                      f => f.RequesterId,
                      u => u.Id,
                      (f, u) => new
                      {
                          u.Id,
                          u.Username,
                          u.FirstName,
                          u.LastName,
                          u.Email,
                          f.CreatedAt
                      })
                .ToList();

            return Ok(requests);
        }


        // POST: api/friendship/request/{userId}
        [HttpPost("request/{userId}")]
        public ActionResult SendRequest(int userId)
        {
            var currentUserId = GetUserId();

            if (userId == currentUserId)
                return BadRequest("No puedes enviarte una solicitud a ti mismo.");

            var exists = _dbContext.Friendships.Any(f =>
                (f.RequesterId == currentUserId && f.ReceiverId == userId) ||
                (f.RequesterId == userId && f.ReceiverId == currentUserId)
            );

            if (exists)
                return BadRequest("Ya existe una solicitud o amistad entre estos usuarios.");

            var friendship = new Friendship
            {
                RequesterId = currentUserId,
                ReceiverId = userId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Friendships.Add(friendship);
            _dbContext.SaveChanges();

            return Ok("Solicitud de amistad enviada.");
        }

        // POST: api/friendship/accept/{userId}
        [HttpPost("accept/{userId}")]
        public ActionResult AcceptRequest(int userId)
        {
            var currentUserId = GetUserId();

            var friendship = _dbContext.Friendships.FirstOrDefault(f =>
                f.RequesterId == userId && f.ReceiverId == currentUserId && f.Status == "Pending");

            if (friendship == null)
                return NotFound("Solicitud de amistad no encontrada.");

            friendship.Status = "Accepted";
            _dbContext.SaveChanges();

            return Ok("Solicitud de amistad aceptada.");
        }
        [HttpDelete("remove/{friendId}")]
        public ActionResult RemoveFriend(int friendId)
        {
            var currentUserId = GetUserId();
            var friendship = _dbContext.Friendships.FirstOrDefault(f =>
                (f.RequesterId == currentUserId && f.ReceiverId == friendId && f.Status == "Accepted") ||
                (f.RequesterId == friendId && f.ReceiverId == currentUserId && f.Status == "Accepted"));
            if (friendship == null)
                return NotFound("Friendship not found.");
            _dbContext.Friendships.Remove(friendship);
            _dbContext.SaveChanges();
            return Ok("Friend removed.");
        }

        // POST: api/friendship/reject/{userId}
        [HttpPost("reject/{userId}")]
        public ActionResult RejectRequest(int userId)
        {
            var currentUserId = GetUserId();

            var friendship = _dbContext.Friendships.FirstOrDefault(f =>
                f.RequesterId == userId && f.ReceiverId == currentUserId && f.Status == "Pending");

            if (friendship == null)
                return NotFound("Solicitud de amistad no encontrada.");

            _dbContext.Friendships.Remove(friendship);
            _dbContext.SaveChanges();

            return Ok("Solicitud de amistad rechazada.");
        }

        private int GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
            {
                throw new Exception("User ID claim is missing or invalid.");
            }
            return userId;
        }

    }
}