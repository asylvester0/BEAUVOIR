using Beauvoir.DTO;
using Beauvoir.Mapping;
using Beauvoir.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Model = Beauvoir.Models.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Beauvoir.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        
        public ModelController(AppDbContext dbContext )
        {
            _dbContext = dbContext;
            
        }
        // GET: api/<ModelController>
        [HttpGet]
        public ActionResult<IEnumerable<ModelDto>>Get()
        {
           try
            {
                var dbModel = ModelInfo().Where(p => p.IsPublic);
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
        [HttpGet("[action]/{id}")]
        [Authorize]
        public IActionResult Download(int id)
        {
            var model = _dbContext.Models.FirstOrDefault(m => m.Id == id);
            if (model == null || model.FileContent == null)
                return NotFound("Model not found.");

            return File(model.FileContent, "application/octet-stream", model.FileName);
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

            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return Unauthorized("Invalid user.");

            var dbTags = _dbContext.Tags.Where(t => tagsId.Contains(t.Id)).ToList();
            var missingTags = tagsId.Except(dbTags.Select(t => t.Id)).ToList();
            if (missingTags.Any())
                return BadRequest($"Missing tags: {string.Join(", ", missingTags)}");

            var model = new Model
            {
                Name = title,
                Description = description,
                IsPublic = isPublic,
                FileName = file.FileName,
                FileExtension = extension,
                FileContent = fileBytes,
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



        // PUT api/<ModelController>/5 // change visibility 
        [HttpPut("{id}")]
        public ActionResult IsPublic(int id,bool boolean)
        {
            var dbmodel = ModelInfo()
                    .FirstOrDefault(x => x.Id == id);

            if (dbmodel == null)
            {       
                return NotFound($"Project with ID={id} not found.");
            }

            dbmodel.IsPublic = boolean;
            _dbContext.SaveChanges();

            return Ok($"Model ID={id} visibility change.");
        }

        private IQueryable<Model> ModelInfo()
        {
            var query = _dbContext.Models
                     .Include("ModelTags")
                     .Include("ModelTags.Tag")
                     .Include("Owner"); 
                   
            return query;
        }
    }
}
