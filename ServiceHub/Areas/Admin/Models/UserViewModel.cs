using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Areas.Admin.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Потребителско име")]
        public string UserName { get; set; }

        [Display(Name = "Имейл")]
        public string Email { get; set; }

        [Display(Name = "Роли")]
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }
}
