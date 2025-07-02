namespace Beauvoir.Models
{
    public class Tag
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public virtual ICollection<ModelTag> ModelTags { get; } = new List<ModelTag>();
    }
}
