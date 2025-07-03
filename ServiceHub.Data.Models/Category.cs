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
        public string Name { get; set; } = null!;

        public virtual ICollection<Service> Services { get; set; } = new HashSet<Service>();
    }
}
