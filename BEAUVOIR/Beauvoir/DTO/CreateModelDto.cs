using System.ComponentModel.DataAnnotations;

namespace Beauvoir.DTO
{
    public class CreateModelDto
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "File is required")]
        public string FilePath { get; set; }

        [Required(ErrorMessage = "Tags are required")]
        public List<int> TagsId { get; set; }
        [Required(ErrorMessage = "Visibility is required")]
        public bool IsPublic { get; set; }
    }
}
