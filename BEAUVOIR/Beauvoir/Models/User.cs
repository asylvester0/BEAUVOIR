using System.Data;

namespace Beauvoir.Models
{
    public class User
    {
        public int  Id{ get; set; }

        public string Username { get; set; } = null!;

        public string PwdHash { get; set; } = null!;

        public string PwdSalt { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public virtual ICollection<Friendship> Friends { get; } = new List<Friendship>();

        public virtual ICollection<Friendship> FriendRequest { get; } = new List<Friendship>();

        public virtual ICollection<Model> Models { get; } = new List<Model>();

       
    }
}

