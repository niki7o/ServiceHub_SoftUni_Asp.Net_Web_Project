using System.ComponentModel.DataAnnotations;

namespace ServiceHub.Core.Models.Tools
{
    public class WordCharacterCountResponseModel
    {

        public int WordCount { get; set; }
        public int CharacterCount { get; set; }
        public int LineCount { get; set; }
    }
}