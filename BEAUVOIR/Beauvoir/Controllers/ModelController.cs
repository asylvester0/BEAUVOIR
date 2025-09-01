using Beauvoir.DTO;
using Beauvoir.Mapping;
using Beauvoir.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Model = Beauvoir.Models.Model;
using Beauvoir.Services;
using Minio.DataModel.Args;
using Minio;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Beauvoir.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ModelController : ControllerBase
    {
        private readonly IMinioClient _minioClient;
        private readonly IConfiguration _config;
        private readonly AppDbContext _dbContext;
        private readonly string _bucketName;

        public ModelController(IMinioClient minioClient, IConfiguration config, AppDbContext dbContext)
        {
            _minioClient = minioClient;
            _config = config;
            _dbContext = dbContext;
            _bucketName = _config["Minio:BucketName"];
        }


        // GET: api/<ModelController>
        [HttpGet]
        public ActionResult<IEnumerable<ModelDto>> Get()
        {
            try
            {
                var dbModel = ModelInfo();
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    // Usuario no autenticado -> solo modelos públicos
                    dbModel = dbModel.Where(m => m.IsPublic);
                }

                else
                {
                    var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
                    if (user == null)
                        return Unauthorized();

                    var friendsIds = GetFriendIds(userId);

                    dbModel = dbModel.Where(m =>
                            m.IsPublic
                            || m.OwnerId == user.Id
                            || (!m.IsPublic && friendsIds.Contains(m.OwnerId))
                        );
                }


                // create restriccions if it is register or not 
                //WHere..
                var models = ModelMapping.MapToBL(dbModel);

                return Ok(models);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred while fetching projects.");
            }
        }

        // GET api/<ModelController>/5
        [HttpGet("{id}")]
        public ActionResult<ModelDto> Get(int id)
        {
            try
            {
                var dbmodel = ModelInfo()
                    .FirstOrDefault(x => x.Id == id);
                if (dbmodel == null)
                {
                    return NotFound($"Could not find project with id {id}");
                }
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    // Usuario no autenticado solo ve si es público
                    if (!dbmodel.IsPublic)
                        return Forbid();
                }
                else
                {
                    var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
                    if (user == null)
                        return Unauthorized();

                    var friendsIds = GetFriendIds(userId);


                    if (!dbmodel.IsPublic && dbmodel.OwnerId != user.Id && !friendsIds.Contains(dbmodel.OwnerId))
                        return Forbid();

                }

                var model = ModelMapping.MapToBL(dbmodel);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred while retrieving the project.");
            }
        }
        //GET : Search 
        [HttpGet("[action]")]
        [Authorize]
        public ActionResult<IEnumerable<ModelDto>> Search(string searchPart, int page = 1, int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchPart))
                {
                    return BadRequest("Search term must not be empty.");
                }
                // Validación de parámetros de paginación
                if (page < 1 || pageSize < 1 || pageSize > 100)
                    return BadRequest("Invalid pagination parameters.");

                var userId = GetUserId(); // We assume we have a method to get the current user's ID
                var friendsIds = GetFriendIds(userId);
                var dbModels = ModelInfo().Where(x =>
                    (x.Name.Contains(searchPart) || x.Description.Contains(searchPart)) &&
                    (x.IsPublic || x.OwnerId == userId || (friendsIds.Contains(x.OwnerId) && !x.IsPublic))
                );
                // Total count

                int totalCount = dbModels.Count();

                // Aplicar paginación
                var pagedModels = dbModels
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();


                var models = ModelMapping.MapToBL(pagedModels);


                return Ok(new
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    Data = models
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred during the search.");
            }
        }
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                throw new Exception("User ID claim is missing or invalid.");
            }
            return userId;
        }



        [HttpPost("presigned-upload")]
        public async Task<IActionResult> GetPresignedUploadUrl([FromBody] string extension)
        {
            if (string.IsNullOrEmpty(extension) || !(extension == ".fbx" || extension == ".obj"))
                return BadRequest("Only .obj and .fbx files are allowed.");

            var objectName = $"{Guid.NewGuid()}{extension}";

            var url = await _minioClient.PresignedPutObjectAsync(
                new PresignedPutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName)
                    .WithExpiry(60 * 10) // válido 10 min
            );

            return Ok(new { url, objectName });
        }

        // 2️⃣ Registrar metadata en la DB
        [HttpPost("register")]
        public ActionResult RegisterModel([FromBody] RegisterModelDto dto)
        {
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
                return Unauthorized("Invalid user.");
            // Check if the tags exist
            var tags = _dbContext.Tags.Where(t => dto.TagsId.Contains(t.Id)).ToList();
            if (tags.Count != dto.TagsId.Count)
            {
                return BadRequest("One or more tags are invalid.");
            }

            var model = new Model
            {
                Name = dto.Title,
                Description = dto.Description,
                IsPublic = dto.IsPublic,
                FileName = dto.OriginalFilename,
                FileExtension = dto.Extension,
                FilePath = dto.ObjectName, // aquí se guarda el nombre en MinIO
                OwnerId = user.Id,
                Owner = user,
                CreatedAt = DateTime.UtcNow
            };
            // Add the tags
            foreach (var tag in tags)
            {
                model.ModelTags.Add(new ModelTag { Tag = tag });
            }

            _dbContext.Models.Add(model);
            _dbContext.SaveChanges();

            return Ok(model);
        }

        // 3️⃣ Descargar con presigned URL
        [HttpGet("{id}/download")]
        public async Task<IActionResult> GetPresignedDownloadUrl(int id)
        {
            var model = await _dbContext.Models.FindAsync(id);
            if (model == null)
                return NotFound();
            // Check access
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                // Unauthenticated user: only public models
                if (!model.IsPublic)
                    return Forbid();
            }
            else
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                    return Unauthorized();
                var friendsIds = GetFriendIds(userId);
                if (!model.IsPublic && model.OwnerId != userId && !friendsIds.Contains(model.OwnerId))
                    return Forbid();
            }
            var url = await _minioClient.PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(model.FilePath)
                    .WithExpiry(60 * 10) // válido 10 min
            );
            return Ok(new { url });

        }

        // PUT api/<ModelController>/5 // change visibility 
        [HttpPut("{id}")]
        public ActionResult IsPublic(int id, bool boolean)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    // Usuario no autenticado solo ve si es público
                    return Forbid();
                }

                var dbmodel = ModelInfo()
                        .FirstOrDefault(x => x.Id == id);


                if (dbmodel == null)
                {
                    return NotFound($"Project with ID={id} not found.");
                }
                if (dbmodel.OwnerId != userId)
                    return Forbid("You are not the owner of this model.");

                dbmodel.IsPublic = boolean;
                _dbContext.SaveChanges();

                return Ok($"Model ID={id} visibility change.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "You can not modify this model");
            }
        }

        private IQueryable<Model> ModelInfo()
        {
            var query = _dbContext.Models
                     .Include("ModelTags")
                     .Include("ModelTags.Tag")
                     .Include("Owner");

            return query;
        }
        private List<int> GetFriendIds(int userId)
        {
            return _dbContext.Friendships
                .Where(f =>
                    (f.RequesterId == userId || f.ReceiverId == userId) &&
                    f.Status == "Accepted")
                .Select(f => f.RequesterId == userId ? f.ReceiverId : f.RequesterId)
                .ToList();
        }

    }
}