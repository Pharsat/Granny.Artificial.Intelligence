using System.Collections.Generic;
using System.Threading.Tasks;

namespace Granny.Email.Application.Integration
{
    public interface IGrannyModelAccessorService
    {
        Task GenerateBodyModel(
            IEnumerable<string> trainingSentences,
            IEnumerable<int> trainingLabels,
            IEnumerable<string> testSentences,
            IEnumerable<int> testLabels,
            int totalWords,
            int sentencesMaxLength);
        Task GenerateHeaderModel(
            IEnumerable<string> trainingSentences,
            IEnumerable<int> trainingLabels,
            IEnumerable<string> testSentences,
            IEnumerable<int> testLabels,
            int totalWords,
            int sentencesMaxLength);
        Task GenerateSubjectModel(
            IEnumerable<string> trainingSentences,
            IEnumerable<int> trainingLabels,
            IEnumerable<string> testSentences,
            IEnumerable<int> testLabels,
            int totalWords,
            int sentencesMaxLength);
        Task<IEnumerable<double>> PredictBody(string sentence);
        Task<IEnumerable<double>> PredictHeader(string sentence);
        Task<IEnumerable<double>> PredictSubject(string sentence);
    }
}