using Inferra.Application.Models.News;

namespace Inferra.Application.Interfaces.Services
{
    public interface INewsSentimentService
    {
        Task<NewsSentimentResponse> GetSentimentAsync();
    }
}
