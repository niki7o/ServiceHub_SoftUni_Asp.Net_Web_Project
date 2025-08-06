using ServiceHub.Common;
using ServiceHub.Common.Enum;
using ServiceHub.Data.Models.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Data.Models
{
    public class Service : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(ValidationConstants.ServiceTitleMaxLength, MinimumLength = ValidationConstants.ServiceTitleMinLength)] 
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(ValidationConstants.ServiceDescriptionMaxLength, MinimumLength = ValidationConstants.ServiceDescriptionMinLength)] 
        public string Description { get; set; } = null!;

        public Guid CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; } = null!;

        public virtual ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new HashSet<Favorite>();

        public AccessType AccessType { get; set; }
        public int ViewsCount { get; set; } = 0;
        public string? ServiceConfigJson { get; set; }

        public bool IsTemplate { get; set; } = false;
        public bool IsApproved { get; set; } = true;

        [Required]
        public string CreatedByUserId { get; set; } = null!;
        [ForeignKey(nameof(CreatedByUserId))]
        public virtual ApplicationUser CreatedByUser { get; set; } = null!;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedOn { get; set; }

        public string? ApprovedByUserId { get; set; }
        [ForeignKey(nameof(ApprovedByUserId))]
        public virtual ApplicationUser? ApprovedByUser { get; set; }
        public DateTime? ApprovedOn { get; set; }

        [MaxLength(ValidationConstants.ServiceImageUrlMaxLength)]
        public string? ImageUrl { get; set; }
    }
}
