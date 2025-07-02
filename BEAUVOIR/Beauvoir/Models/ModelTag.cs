namespace Beauvoir.Models
{
    public class ModelTag
    {
        public int Id { get; set; }

        public int ModelId { get; set; }

        public int TagId { get; set; }

        public virtual Tag Tag { get; set; } = null!;

        public virtual Model Model { get; set; } = null!;
    }
}
