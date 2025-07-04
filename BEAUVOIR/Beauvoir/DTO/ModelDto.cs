namespace Beauvoir.DTO
{
    public class ModelDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public List<string> Tags { get; set; }
        public string Owner { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; } 

    }
}
