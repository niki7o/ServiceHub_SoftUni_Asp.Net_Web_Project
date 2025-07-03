using ServiceHub.Data.Models.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Data.Models
{
    public class Service : BaseEntity
    {
        [Required]
        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public bool IsBusinessOnly { get; set; }

        public Guid CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        public virtual ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
    }
}
