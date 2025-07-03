using System.ComponentModel.DataAnnotations;

namespace Beauvoir.DTO
{
    public class TagDto
    {
         public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
    
    }
}