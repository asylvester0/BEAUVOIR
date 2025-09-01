namespace Beauvoir.DTO
{
    public class RegisterModelDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }
        public string OriginalFilename { get; set; }
        public string Extension { get; set; }
        public string ObjectName { get; set; }
        public List<int> TagsId { get; set; } // Add this

    }
}