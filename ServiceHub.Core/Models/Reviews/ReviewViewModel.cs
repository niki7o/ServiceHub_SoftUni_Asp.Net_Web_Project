using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.Reviews
{
    public class ReviewViewModel
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string UserName { get; set; } = null!;
        public string? UserRoleCssClass { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = null!;
        public DateTime CreatedOn { get; set; }
        public string ServiceName { get; set; } = null!;
        public bool IsAuthor { get; set; }
    }
}
