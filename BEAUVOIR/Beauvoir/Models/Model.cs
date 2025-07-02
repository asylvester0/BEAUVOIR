namespace Beauvoir.Models
{
    public class Model
    {
        public int Id { get; set; }
        
        public int OwnerId { get; set;  }

        public virtual User Owner { get; set; } 

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string FilePath { get; set; }

        public virtual ICollection<ModelTag> ModelTags { get; set; } = new List<ModelTag>();
    
        public bool IsPublic { get; set; }

        public DateTime CreatedAt { set; get;  }

      

     
    }
}

