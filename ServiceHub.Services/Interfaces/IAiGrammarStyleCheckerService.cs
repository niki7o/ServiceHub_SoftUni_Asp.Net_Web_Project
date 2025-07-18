using ServiceHub.Core.Models.AI;

namespace ServiceHub.Services.Interfaces
{
    public interface IAiGrammarStyleCheckerService
    {
        Task<AiCheckResultModel> CheckGrammarAndStyleAsync(string text, string language);
    }
}