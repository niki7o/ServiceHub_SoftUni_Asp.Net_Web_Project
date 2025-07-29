using ServiceHub.Common.Enum;
using ServiceHub.Core.Models.Reviews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.Service
{

    public class ServiceViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public AccessType AccessType { get; set; } 
 
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }
        public bool IsFavorite { get; set; }
        public int ViewsCount { get; set; } = 0;
        public IEnumerable<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>(); 
    }
}
