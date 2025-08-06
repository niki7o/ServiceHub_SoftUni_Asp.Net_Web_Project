using ServiceHub.Common;
using ServiceHub.Data.Models.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Data.Models
{
    public class Category :BaseEntity
    {
        [Required]
        [MaxLength(ValidationConstants.CategoryNameMaxLength)] 
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public virtual ICollection<Service> Services { get; set; } = new HashSet<Service>();
    }

}
