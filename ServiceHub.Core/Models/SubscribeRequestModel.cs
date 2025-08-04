using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models
{
    public class SubscribeRequestModel
    {
        [Required(ErrorMessage = "Потвърждението е задължително.")]
        public bool ConfirmSubscription { get; set; } 
    }
}
