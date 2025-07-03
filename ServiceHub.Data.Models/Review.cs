using ServiceHub.Data.Models.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Data.Models
{
    public class Review : BaseEntity

    {
        [Range(1, 5)]
        public int Rating { get; set; }

        public string Comment { get; set; } = null!;

        public Guid ServiceId { get; set; }
        public virtual Service Service { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;

    }
}
