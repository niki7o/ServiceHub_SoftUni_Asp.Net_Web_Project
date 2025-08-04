using ServiceHub.Core.Models.Reviews;
using ServiceHub.Core.Models.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.User
{
    public class UserProfileViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Потребителско име")]
        public string UserName { get; set; }

        [Display(Name = "Имейл")]
        public string Email { get; set; }

        [Display(Name = "Роли")]
        public IEnumerable<string> Roles { get; set; } = new List<string>();

        [Display(Name = "Бизнес потребител")]
        public bool IsBusiness { get; set; }

        [Display(Name = "Бизнес абонамент изтича на")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? BusinessExpiresOn { get; set; }

        [Display(Name = "Последно създаване на услуга")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? LastServiceCreationDate { get; set; }

        [Display(Name = "Одобрени създадени услуги")]
        public int ApprovedServicesCount { get; set; }

      
        public IEnumerable<ServiceViewModel> CreatedServices { get; set; } = new List<ServiceViewModel>();
        public IEnumerable<ServiceViewModel> FavoriteServices { get; set; } = new List<ServiceViewModel>();
        public IEnumerable<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();

       
        public int CreatedServicesCurrentPage { get; set; }
        public int CreatedServicesTotalPages { get; set; }
        public int CreatedServicesPageSize { get; set; } = 5; 
        public int TotalCreatedServicesCount { get; set; }

       
        public int ReviewsCurrentPage { get; set; }
        public int ReviewsTotalPages { get; set; }
        public int ReviewsPageSize { get; set; } = 5; // Default to 5 items per page
        public int TotalReviewsCount { get; set; }
    }
}
