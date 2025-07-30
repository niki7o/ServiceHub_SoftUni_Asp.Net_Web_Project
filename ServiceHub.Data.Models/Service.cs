using ServiceHub.Data.Models.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiceHub.Common.Enum;

namespace ServiceHub.Data.Models
{
    public class Service : BaseEntity
    {

        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;


        public Guid CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        public virtual ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
        public virtual ICollection<Favorite> Favorites { get; set; }
        public AccessType AccessType { get; set; }
        public int ViewsCount { get; set; } = 0;
        public string? ServiceConfigJson { get; set; }
    }
}
