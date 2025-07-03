using Beauvoir.DTO;
using Beauvoir.Mapping;
using Beauvoir.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Beauvoir.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public TagController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("[action]")]
        [Authorize]
        public ActionResult<IEnumerable<TagDto>> List()
        {
            try
            {
                var dbTags = _dbContext.Tags;

                var Tags = TagMapping.MapToBL(dbTags);

                return Ok(Tags);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "There has been a problem while fetching the data you requested");
            }
        }
        [HttpPost()]
        [Authorize]
        public ActionResult<TagDto> Add(TagDto Tagdto)
        {
            try
            {
                var dbTag = TagMapping.MapToDAL(Tagdto);

                _dbContext.Tags.Add(dbTag);

                _dbContext.SaveChanges();

                Tagdto = TagMapping.MapToBL(dbTag);

                return Ok(Tagdto);



            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "There has been a problem while fetching the data you requested");

            }



        }
        [HttpPut("{n}")]
        [Authorize]
        public ActionResult<TagDto> Edit(int n, TagDto Tagdto)
        {
            try
            {
                var Tag = _dbContext.Tags.FirstOrDefault(x => x.Id == n);
                if (Tag == null) return NotFound();

                Tag.Name = Tagdto.Name;

                _dbContext.SaveChanges();
                Tagdto = TagMapping.MapToBL(Tag);
                return Ok(Tagdto);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }


        }
        [HttpDelete("{n}")]
        [Authorize]
        public ActionResult<TagDto> Delete(int n)
        {
            try
            {
                var Tag = _dbContext.Tags.Include(x => x.ModelTags).FirstOrDefault(x => x.Id == n);
                if (Tag == null)
                    return NotFound();



                if (Tag.ModelTags != null && Tag.ModelTags.Any())
                {
                    return BadRequest("Tag can not be deleted if related to existing project .");
                }
                _dbContext.Tags.Remove(Tag); // Then remove "parent"
                _dbContext.SaveChanges();

                var result = TagMapping.MapToBL(Tag);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
