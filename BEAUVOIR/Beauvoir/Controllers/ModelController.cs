using Beauvoir.DTO;
using Beauvoir.Mapping;
using Beauvoir.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Amazon.S3;
using Amazon.S3.Transfer;
using Model = Beauvoir.Models.Model;
using Beauvoir.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Beauvoir.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ModelController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly MinioService _minioService;

        public ModelController(AppDbContext dbContext, MinioService minioService)
        {
            _dbContext = dbContext;
            _minioService = minioService;
        }

        // GET: api/<ModelController>
        [HttpGet]
        public ActionResult<IEnumerable<ModelDto>>Get()
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
            }catch(Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred while fetching projects.");
            }
        }

        // GET api/<ModelController>/5
        [HttpGet("{id}")]
        public ActionResult<ModelDto> Get(int id)
        {
            try {
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

                var dbModels = ModelInfo().Where(x => x.Name.Contains(searchPart) || x.Description.Contains(searchPart));
                {

                    dbModels = dbModels.Where(p => p.IsPublic);
                }
                
                // Total count (opcional, para frontend)
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
        

        // POST api/<ModelController>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadModel(
     [FromForm] string title,
     [FromForm] string description,
     [FromForm] bool isPublic,
     [FromForm] List<int> tagsId,
     IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var allowedExtensions = new[] { ".obj", ".fbx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Only .obj and .fbx files are allowed.");

            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return Unauthorized("Invalid user.");

            var dbTags = _dbContext.Tags.Where(t => tagsId.Contains(t.Id)).ToList();
            var missingTags = tagsId.Except(dbTags.Select(t => t.Id)).ToList();
            if (missingTags.Any())
                return BadRequest($"Missing tags: {string.Join(", ", missingTags)}");

            // Generar nombre único para el objeto en Minio
            var objectName = $"{Guid.NewGuid()}{extension}";

            // Subir a Minio
            using (var stream = file.OpenReadStream())
            {
                await _minioService.UploadFileAsync(
                    objectName,
                    stream,
                    "application/octet-stream");
            }

            // Crear modelo en base de datos
            var model = new Model
            {
                Name = title,
                Description = description,
                IsPublic = isPublic,
                FileName = file.FileName,
                FileExtension = extension,
                FilePath = objectName, // Guardar la referencia al objeto en Minio
                OwnerId = user.Id,
                Owner = user,
                ModelTags = dbTags.Select(t => new ModelTag { Tag = t }).ToList(),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Models.Add(model);
            await _dbContext.SaveChangesAsync();

            var modelDto = ModelMapping.MapToBL(model);
            return Ok(modelDto);
        }

        [HttpGet("[action]/{id}")]
        [Authorize]
        public async Task<IActionResult> Download(int id)
        {
            var model = _dbContext.Models.FirstOrDefault(m => m.Id == id);
            if (model == null || string.IsNullOrEmpty(model.FilePath))
                return NotFound("Model not found.");

            var stream = await _minioService.DownloadFileAsync(model.FilePath);
            return File(stream, "application/octet-stream", model.FileName);
        }

        // PUT api/<ModelController>/5 // change visibility 
        [HttpPut("{id}")]
        public ActionResult IsPublic(int id,bool boolean)
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
