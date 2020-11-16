using System.Collections.Generic;
using System.Threading.Tasks;

namespace Granny.Email.Application.Repository
{
    public interface IGrannyRepository
    {
        Task AddSubjectArray(IEnumerable<string> wordsArray, int label);
        Task AddHeaderArray(IEnumerable<string> wordsArray, int label);
        Task AddBodyArray(IEnumerable<string> wordsArray, int label);
        Task AddUri(string uri, int secure);
        Task<(IEnumerable<Sentence> Sentences, IEnumerable<Label> Labels)> GetHeaderTrainData();
        Task<(IEnumerable<Sentence> Sentences, IEnumerable<Label> Labels)> GetSubjectTrainData();
        Task<(IEnumerable<Sentence> Sentences, IEnumerable<Label> Labels)> GetBodyTrainData();
    }
}