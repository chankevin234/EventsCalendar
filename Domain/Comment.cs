using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain
{
    public class Comment
    {
        public int Id { get; set; }
        public string Body { get; set; } //text posted by user
        public AppUser Author { get; set; } //author of comment
        public Activity Activity { get; set; } //activity being commneted on
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; //time that comment was posted

    }
}