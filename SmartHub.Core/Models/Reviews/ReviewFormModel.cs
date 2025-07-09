using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.Reviews
{
    public class ReviewFormModel
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Comment must be between 10 and 500 characters.")]
        public string Comment { get; set; } = null!;
    }
}
