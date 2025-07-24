using ServiceHub.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.Tools
{
    public class CvGenerateRequestModel : BaseServiceRequest 
    {
     

        [Required(ErrorMessage = "Името е задължително.")]
        [StringLength(100, ErrorMessage = "Името не може да надвишава 100 символа.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Имейлът е задължителен.")]
        [EmailAddress(ErrorMessage = "Невалиден формат на имейл адрес.")]
        [StringLength(100, ErrorMessage = "Имейлът не може да надвишава 100 символа.")]
        public string Email { get; set; } = null!;

        [StringLength(50, ErrorMessage = "Телефонният номер не може да надвишава 50 символа.")]
        public string? Phone { get; set; }

        [StringLength(2000, ErrorMessage = "Образованието не може да надвишава 2000 символа.")]
        public string? Education { get; set; }

        [StringLength(4000, ErrorMessage = "Опитът не може да надвишава 4000 символа.")]
        public string? Experience { get; set; }

        [StringLength(2000, ErrorMessage = "Уменията не могат да надвишават 2000 символа.")]
        public string? Skills { get; set; }
    }
}
