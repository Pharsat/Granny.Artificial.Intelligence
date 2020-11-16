using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Granny.Email.Application.Integration;

namespace Granny.Email.Infrastructure.Integration
{
    public class GrannyPythonModelAccessorService : IGrannyModelAccessorService
    {
        private const string BodyModelBaseUrl = "http://127.0.0.1:8082";
        private const string HeaderModelBaseUrl = "http://127.0.0.1:8082";
        private const string SubjectModelBaseUrl = "http://127.0.0.1:8082";

        public async Task GenerateBodyModel(
            IEnumerable<string> trainingSentences,
            IEnumerable<int> trainingLabels,
            IEnumerable<string> testSentences,
            IEnumerable<int> testLabels,
            int totalWords,
            int sentencesMaxLength)
        {
            var body = new
            {
                training_sentences = trainingSentences,
                training_labels = trainingLabels,
                test_sentences = testSentences,
                test_labels = testLabels,
                total_words = totalWords,
                sentence_max_lenght = sentencesMaxLength
            };

            await BodyModelBaseUrl
                   .AppendPathSegment("api/model/generate")
                   .PostJsonAsync(body)
                   .ConfigureAwait(false);
        }

        public async Task GenerateHeaderModel(
            IEnumerable<string> trainingSentences,
            IEnumerable<int> trainingLabels,
            IEnumerable<string> testSentences,
            IEnumerable<int> testLabels,
            int totalWords,
            int sentencesMaxLength)
        {
            var body = new
            {
                training_sentences = trainingSentences,
                training_labels = trainingLabels,
                test_sentences = testSentences,
                test_labels = testLabels,
                total_words = totalWords,
                sentence_max_lenght = sentencesMaxLength
            };

            await HeaderModelBaseUrl
                .AppendPathSegment("api/model/generate")
                .PostJsonAsync(body)
                .ConfigureAwait(false);
        }

        public async Task GenerateSubjectModel(
            IEnumerable<string> trainingSentences,
            IEnumerable<int> trainingLabels,
            IEnumerable<string> testSentences,
            IEnumerable<int> testLabels,
            int totalWords,
            int sentencesMaxLength)
        {
            var body = new
            {
                training_sentences = trainingSentences,
                training_labels = trainingLabels,
                test_sentences = testSentences,
                test_labels = testLabels,
                total_words = totalWords,
                sentence_max_lenght = sentencesMaxLength
            };

            await SubjectModelBaseUrl
                .AppendPathSegment("api/model/generate")
                .PostJsonAsync(body)
                .ConfigureAwait(false);
        }
    }
}
