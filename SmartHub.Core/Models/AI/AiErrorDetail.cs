namespace ServiceHub.Core.Models.AI
{
    public class AiErrorDetail
    {
        public string Message { get; set; } = string.Empty;
        public string SuggestedCorrection { get; set; } = string.Empty;
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string Category { get; set; } = string.Empty;
    }
}