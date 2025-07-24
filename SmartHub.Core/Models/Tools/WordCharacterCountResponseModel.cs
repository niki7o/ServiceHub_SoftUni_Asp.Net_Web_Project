using System.ComponentModel.DataAnnotations;

namespace ServiceHub.Core.Models.Tools
{
    public class WordCharacterCountResponseModel
    {

        public int WordCount { get; set; }
        public int CharCount { get; set; } 
        public int LineCount { get; set; }
        public string? Message { get; set; }
    }
}