using Beauvoir.DTO;
using Beauvoir.Models;

namespace Beauvoir.Mapping
{
    public class ModelMapping
    {
        public static IEnumerable<ModelDto> MapToBL(IEnumerable<Model> models) =>
        models.Select(p => MapToBL(p));
        public static ModelDto MapToBL(Model model) =>
            new ModelDto
            {
                Id = model.Id,
                Title = model.Name,
                Description = model.Description,
                Tags = model.ModelTags.Select(mt => mt.Tag.Name).ToList(),
                FilePath = model.FilePath,
                Owner = model.Owner.Username,
                IsPublic = model.IsPublic,
                CreatedAt = model.CreatedAt
            };


        public static IEnumerable<Model> MapToDAL(IEnumerable<CreateModelDto> blProjects) =>
            blProjects.Select(x => MapToDAL(x));

        public static Model MapToDAL(CreateModelDto model) =>
            new Model
            {
                Name = model.Title,
                Description = model.Description,
                IsPublic = model.IsPublic,
                //FilePath = model.FilePath
            };
    }
}

