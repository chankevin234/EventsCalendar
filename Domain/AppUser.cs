using Microsoft.AspNetCore.Identity;

namespace Domain
{
    public class AppUser : IdentityUser //1 to many relationship
    {
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public ICollection<ActivityAttendee> Activities { get; set; } // use the join entity as your Icollection type
        public ICollection<Photo> Photos { get; set; } // relates to Photo class (key)
        public ICollection<UserFollowing> Followings { get; set; } //who is the user following
        public ICollection<UserFollowing> Followers { get; set; }
    }
}