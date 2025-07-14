using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Common
{
   public abstract class BaseServiceRequest
    {
        [Required(ErrorMessage = "Service ID is required.")]
        public Guid ServiceId { get; set; }
    }
}
