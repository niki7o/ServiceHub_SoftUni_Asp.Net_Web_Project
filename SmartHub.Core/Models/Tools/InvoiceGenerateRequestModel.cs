using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.Tools
{
    public class InvoiceItem
    {
        [Required(ErrorMessage = "Името на артикула е задължително.")]
        [StringLength(200, ErrorMessage = "Името на артикула не може да надвишава 200 символа.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Количеството е задължително.")]
        [Range(0.01, 1000000, ErrorMessage = "Количеството трябва да бъде положително число.")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "Единичната цена е задължителна.")]
        [Range(0.01, 10000000, ErrorMessage = "Единичната цена трябва да бъде положително число.")]
        public decimal UnitPrice { get; set; }
    }

    
    public class InvoiceGenerateRequestModel
    {
        [Required(ErrorMessage = "Номерът на фактурата е задължителен.")]
        [StringLength(50, ErrorMessage = "Номерът на фактурата не може да надвишава 50 символа.")]
        public string InvoiceNumber { get; set; }

        [Required(ErrorMessage = "Датата на издаване е задължителна.")]
        [DataType(DataType.Date, ErrorMessage = "Невалиден формат на дата.")]
        public DateTime IssueDate { get; set; }

        [Required(ErrorMessage = "Името на продавача е задължително.")]
        [StringLength(200, ErrorMessage = "Името на продавача не може да надвишава 200 символа.")]
        public string SellerName { get; set; }

        [StringLength(500, ErrorMessage = "Адресът на продавача не може да надвишава 500 символа.")]
        public string SellerAddress { get; set; }

        [StringLength(50, ErrorMessage = "Булстатът на продавача не може да надвишава 50 символа.")]
        public string SellerEIK { get; set; }

        [StringLength(50, ErrorMessage = "МОЛ на продавача не може да надвишава 50 символа.")]
        public string SellerMOL { get; set; } 

        [Required(ErrorMessage = "Името на купувача е задължително.")]
        [StringLength(200, ErrorMessage = "Името на купувача не може да надвишава 200 символа.")]
        public string BuyerName { get; set; }

        [StringLength(500, ErrorMessage = "Адресът на купувача не може да надвишава 500 символа.")]
        public string BuyerAddress { get; set; }

        [StringLength(50, ErrorMessage = "Булстатът на купувача не може да надвишава 50 символа.")]
        public string BuyerEIK { get; set; }

        [StringLength(50, ErrorMessage = "МОЛ на купувача не може да надвишава 50 символа.")]
        public string BuyerMOL { get; set; }

        [Required(ErrorMessage = "Артикулите са задължителни.")]
        [MinLength(1, ErrorMessage = "Трябва да има поне един артикул във фактурата.")]
        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

        [Range(0, 100, ErrorMessage = "Отстъпката трябва да бъде между 0 и 100 процента.")]
        public decimal DiscountPercentage { get; set; } = 0; 

        [Range(0, 100, ErrorMessage = "Данъчната ставка трябва да бъде между 0 и 100 процента.")]
        public decimal TaxRatePercentage { get; set; } = 20; 

        [StringLength(500, ErrorMessage = "Начинът на плащане не може да надвишава 500 символа.")]
        public string PaymentMethod { get; set; }

        [StringLength(1000, ErrorMessage = "Допълнителните бележки не могат да надвишават 1000 символа.")]
        public string Notes { get; set; }
    }

    
    public class InvoiceGenerateResult
    {
        public bool IsSuccess { get; set; }
        public byte[] GeneratedFileContent { get; set; }
        public string GeneratedFileName { get; set; }
        public string ContentType { get; set; }
        public string ErrorMessage { get; set; }
    }
}
