using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Common
{
    public  static class ValidationConstants
    {
        public const int CategoryNameMaxLength = 100;

      
        public const int ReviewRatingMin = 1;
        public const int ReviewRatingMax = 5;

       
        public const int ServiceTitleMinLength = 3;
        public const int ServiceTitleMaxLength = 100;
        public const int ServiceDescriptionMinLength = 10;
        public const int ServiceDescriptionMaxLength = 1000;
        public const int ServiceImageUrlMaxLength = 2048;
    }
}
