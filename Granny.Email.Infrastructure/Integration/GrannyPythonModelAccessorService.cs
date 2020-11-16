using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
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

        private const string BodyPredictBaseUrl = "";
        private const string HeaderPredictBaseUrl = "";
        private const string SubjectPredictBaseUrl = "";

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

        public async Task<IEnumerable<double>> PredictBody(string sentence)
        {
            var body = new { sentence };
            var result = await BodyPredictBaseUrl
                 .AppendPathSegment("api/model/predict")
                 .PostJsonAsync(body)
                 .ConfigureAwait(false);

            if (result.StatusCode != (int) HttpStatusCode.OK) throw new Exception("Response is not ok.");

            var json = await result.GetJsonAsync().ConfigureAwait(false);

            return JsonSerializer.Deserialize<IEnumerable<double>>(json);
        }

        public async Task<IEnumerable<double>> PredictHeader(string sentence)
        {
            var body = new { sentence };
            var result = await HeaderPredictBaseUrl
                .AppendPathSegment("api/model/predict")
                .PostJsonAsync(body)
                .ConfigureAwait(false);

            if (result.StatusCode != (int)HttpStatusCode.OK) throw new Exception("Response is not ok.");

            var json = await result.GetJsonAsync().ConfigureAwait(false);

            return JsonSerializer.Deserialize<IEnumerable<double>>(json);
        }

        public async Task<IEnumerable<double>> PredictSubject(string sentence)
        {
            var body = new { sentence };
            var result = await SubjectPredictBaseUrl
                .AppendPathSegment("api/model/predict")
                .PostJsonAsync(body)
                .ConfigureAwait(false);

            if (result.StatusCode != (int)HttpStatusCode.OK) throw new Exception("Response is not ok.");

            var json = await result.GetJsonAsync().ConfigureAwait(false);

            return JsonSerializer.Deserialize<IEnumerable<double>>(json);
        }
    }
}
