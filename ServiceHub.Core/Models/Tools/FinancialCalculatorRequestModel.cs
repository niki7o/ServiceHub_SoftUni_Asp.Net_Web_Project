using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.Tools
{
    public class RevenueItem
    {
        [Required(ErrorMessage = "Описанието на прихода е задължително.")]
        [StringLength(200, ErrorMessage = "Описанието на прихода не може да надвишава 200 символа.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Сумата на прихода е задължителна.")]
        [Range(0.01, 1000000000, ErrorMessage = "Сумата на прихода трябва да бъде положително число.")]
        public decimal Amount { get; set; }
    }

  
    public class ExpenseItem
    {
        [Required(ErrorMessage = "Описанието на разхода е задължително.")]
        [StringLength(200, ErrorMessage = "Описанието на разхода не може да надвишава 200 символа.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Сумата на разхода е задължителна.")]
        [Range(0.01, 1000000000, ErrorMessage = "Сумата на разхода трябва да бъде положително число.")]
        public decimal Amount { get; set; }
    }

   
    public class FinancialCalculatorRequestModel
    {
      
        [Range(0, 1000000000, ErrorMessage = "Нетната печалба трябва да бъде валидно число.")]
        public decimal NetProfitForROI { get; set; } = 0;

        [Range(0.01, 1000000000, ErrorMessage = "Цената на инвестицията трябва да бъде положително число.")]
        public decimal CostOfInvestment { get; set; } = 0;

       
        public List<RevenueItem> Revenues { get; set; } = new List<RevenueItem>();
        public List<ExpenseItem> Expenses { get; set; } = new List<ExpenseItem>();

       
        [Range(0, 100, ErrorMessage = "Процентът на растеж трябва да бъде между 0 и 100.")]
        public decimal GrowthRatePercentage { get; set; } = 0; 

        [Range(1, 120, ErrorMessage = "Периодът на прогноза трябва да бъде между 1 и 120 месеца.")]
        public int ForecastPeriodMonths { get; set; } = 12; 

        [StringLength(1000, ErrorMessage = "Допълнителните бележки не могат да надвишават 1000 символа.")]
        public string Notes { get; set; }
    }


    public class FinancialCalculationResult
    {
        public bool IsSuccess { get; set; }
        public byte[] GeneratedFileContent { get; set; }
        public string GeneratedFileName { get; set; }
        public string ContentType { get; set; }
        public string ErrorMessage { get; set; }

        
        public decimal CalculatedROI { get; set; }
        public decimal TotalRevenues { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfitLoss { get; set; }
        public decimal ForecastedRevenues { get; set; }
        public decimal ForecastedExpenses { get; set; }
        public decimal ForecastedNetProfitLoss { get; set; }
    }
}