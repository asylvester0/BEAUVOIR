using Microsoft.AspNetCore.Mvc;

namespace Beauvoir.Models
{
    public class Friendship
    {
        public int Id { get; set; }

        public int RequesterId { get; set;  }
        
        public int ReceiverId { get; set;  }

        public string Status { get; set;  }

        public DateTime CreatedAt { get; set;  }
    }
}
