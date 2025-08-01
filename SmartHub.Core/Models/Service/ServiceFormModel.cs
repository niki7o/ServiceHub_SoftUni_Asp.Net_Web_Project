using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceHub.Common.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.Service
{
    public class ServiceFormModel
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters.")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters.")]
        public string Description { get; set; } = null!;


        [Required(ErrorMessage = "Category is required.")]
        [Display(Name = "Category")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Access Type is required.")]
        [Display(Name = "Access Type")]
        public AccessType AccessType { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    }
}
