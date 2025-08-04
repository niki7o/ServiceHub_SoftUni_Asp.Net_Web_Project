using ServiceHub.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.Tools
{
    public class ContractGenerateRequestModel: BaseServiceRequest
    {
        [Required(ErrorMessage = "Типът на договора е задължителен.", AllowEmptyStrings = false)]
        [StringLength(100, ErrorMessage = "Типът на договора не може да надвишава 100 символа.")]
        public string ContractType { get; set; } = null!;

        [Required(ErrorMessage = "Име на страна А е задължително.", AllowEmptyStrings = false)]
        [StringLength(200, ErrorMessage = "Името на страна А не може да надвишава 200 символа.")]
        public string PartyA { get; set; } = null!;

        [Required(ErrorMessage = "Име на страна Б е задължително.", AllowEmptyStrings = false)]
        [StringLength(200, ErrorMessage = "Името на страна Б не може да надвишава 200 символа.")]
        public string PartyB { get; set; } = null!;

        [Required(ErrorMessage = "Дата на договора е задължителна.", AllowEmptyStrings = false)]
        public DateTime ContractDate { get; set; }

        [Required(ErrorMessage = "Условията на договора са задължителни.", AllowEmptyStrings = false)]
        [StringLength(8000, ErrorMessage = "Условията на договора не могат да надвишават 8000 символа.")]
        public string ContractTerms { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Допълнителна информация не може да надвишава 500 символа.")]
        public string? AdditionalInfo { get; set; }
    }
}
