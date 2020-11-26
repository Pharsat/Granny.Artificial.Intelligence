using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Granny.Email.Application.Classification;
using Granny.Email.Application.EmailInformationExtractor;
using Granny.Email.Application.Integration;
using Granny.Email.Application.Preparation;
using Granny.Email.Application.Repository;
using Granny.Email.WebApp.Models.Predict;
using Granny.Email.WebApp.Models.Train;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace Granny.Email.WebApp.Controllers
{
    public class AnalyzerController : Controller
    {
        private readonly IMailRepository _mailKitRepository;
        private readonly IClassificationService _classificationService;
        private readonly IInformationExtractorService _informationExtractorService;
        private readonly ITextPreparerService _textPreparerService;
        private readonly IGrannyRepository _grannyRepository;
        private readonly IGrannyModelAccessorService _grannyModelAccessorService;
        private readonly IRawEmailsRepository _rawEmailsRepository;

        public AnalyzerController(
            IMailRepository mailKitRepository,
            IClassificationService classificationService,
            IInformationExtractorService informationExtractorService,
            ITextPreparerService textPreparerService,
            IGrannyRepository grannyRepository,
            IGrannyModelAccessorService grannyModelAccessorService,
            IRawEmailsRepository rawEmailsRepository)
        {
            _mailKitRepository = mailKitRepository;
            _classificationService = classificationService;
            _informationExtractorService = informationExtractorService;
            _textPreparerService = textPreparerService;
            _grannyRepository = grannyRepository;
            _grannyModelAccessorService = grannyModelAccessorService;
            _rawEmailsRepository = rawEmailsRepository;
        }

        public async Task<IActionResult> Analyze(string messageId)
        {
            var (mail, uniqueId) = await _mailKitRepository.GetUnreadEmailByIdAsync(messageId).ConfigureAwait(false);

            var receivedHeaders = mail.Headers.Where(header => header.Field == "Received").ToList();
            var receivedDatesAsString = receivedHeaders.Select(header => header.Value.Split(";")[1].Trim().Substring(0, 31)).ToList();
            var receivedDates = receivedDatesAsString.Select(date => DateTimeOffset.Parse(date, CultureInfo.CurrentCulture)).OrderBy(date => date).ToList();

            var authorizationResultsHeaders = mail.Headers.Where(header => header.Field == "Authentication-Results");
            var authorizationResultsHeadersValues = authorizationResultsHeaders.Select(authorizationResultsHeader => authorizationResultsHeader.Value).ToList();

            var senderPolicyFrameworkProtocolsResults = _informationExtractorService.GetSenderPolicyFrameworkResults(authorizationResultsHeadersValues);
            var domainKeysIdentifiedMailProtocolsResults = _informationExtractorService.GetDomainKeysIdentifiedMailResults(authorizationResultsHeadersValues);
            var domainBasedMessageAuthenticationReportingConformanceProtocolsResults = _informationExtractorService.GetDomainBasedMessageAuthenticationReportingConformanceResults(authorizationResultsHeadersValues);

            // https://developers.virustotal.com/v3.0/reference#file-info see how to analize files
            var attachmentsNames = mail.Attachments.Select(attachment => attachment.ContentDisposition.FileName);
            var fileExtensions = attachmentsNames.Select(attachmentsName => Path.GetExtension(attachmentsName).ToLower());

            var vector = new int[8];
            var receivedDatesDistance = _classificationService.CalculateReceivedDateDistance(receivedDates);
            var receivedQuantity = _classificationService.CalculateReceivedQuantity(receivedHeaders.Count);
            var domainCorrespondsToMessageId = _classificationService.CalculateFromAndMessageIdDomains(messageId, mail.From.Select(from => ((MailboxAddress)from).Address));
            var byAndFromChainMatches = 0;
            var spfProtocol = _classificationService.CalculateSenderPolicyFrameworkProtocolAssessment(senderPolicyFrameworkProtocolsResults);
            var dkimProtocol = _classificationService.CalculateDomainKeysIdentifiedMailProtocolAssessment(domainKeysIdentifiedMailProtocolsResults);
            var dmarkProtocol = _classificationService.CalculateDomainBasedMessageAuthenticationReportingConformanceProtocolsAssessment(domainBasedMessageAuthenticationReportingConformanceProtocolsResults);
            var attachmentsExtensions = _classificationService.CalculateFileExtensionTypes(fileExtensions);
            var linksNumber = 0;
            var anchorLinksMatches = 0;

            return View();
        }

        public async Task<IActionResult> GenerateEmailSentence(string messageId, bool isSpam)
        {
            var (mail, uniqueId) = await _mailKitRepository.GetUnreadEmailByIdAsync(messageId).ConfigureAwait(false);

            var headerEmailSentence = new StringBuilder();

            var authorizationResultsHeaders = mail.Headers.Where(header => header.Field == "Authentication-Results");
            var authorizationResultsHeadersValues = authorizationResultsHeaders.Select(authorizationResultsHeader => authorizationResultsHeader.Value).ToList();

            var senderPolicyFrameworkProtocolsResults = _informationExtractorService.GetSenderPolicyFrameworkResults(authorizationResultsHeadersValues);
            var domainKeysIdentifiedMailProtocolsResults = _informationExtractorService.GetDomainKeysIdentifiedMailResults(authorizationResultsHeadersValues);
            var domainBasedMessageAuthenticationReportingConformanceProtocolsResults = _informationExtractorService.GetDomainBasedMessageAuthenticationReportingConformanceResults(authorizationResultsHeadersValues);

            //Add attachments info
            foreach (var attachment in mail.Attachments)
            {
                if (attachment.ContentDisposition.FileName != null)
                {
                    var file = attachment.ContentDisposition.FileName;
                    var fileSize = attachment.ContentDisposition.Size;
                    var fileName = Path.GetFileName(file).ToLower();
                    var fileExtension = Path.GetExtension(file).ToLower();
                    headerEmailSentence.Append($"attachment filename {fileName} extension {fileExtension} size {fileSize} ");
                }
            }

            //Add protocol result info
            foreach (var protocolResult in senderPolicyFrameworkProtocolsResults)
            {
                headerEmailSentence.Append($"spfProtocol result {protocolResult} ");
            }

            foreach (var protocolResult in domainKeysIdentifiedMailProtocolsResults)
            {
                headerEmailSentence.Append($"dkimProtocol result {protocolResult} ");
            }

            foreach (var protocolResult in domainBasedMessageAuthenticationReportingConformanceProtocolsResults)
            {
                headerEmailSentence.Append($"dmarcProtocol result {protocolResult} ");
            }

            //Add body info
            var bodyHtmlText = mail.HtmlBody;

            var uris = Regex
                .Matches(bodyHtmlText, @"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)", RegexOptions.IgnoreCase)
                .Select(match => match.Value)
                .ToList();

            var subjectSentence = _textPreparerService.Prepare(mail.Subject).ToList();
            var subjectLabel = Convert.ToInt32(isSpam);

            var headerSentence = _textPreparerService.Prepare(headerEmailSentence.ToString()).ToList();
            var headerLabel = Convert.ToInt32(isSpam);

            var bodySentence = _textPreparerService.Prepare(GetBodyText(bodyHtmlText)).ToList();
            var bodyLabel = Convert.ToInt32(isSpam);

            await _grannyRepository.AddSubjectArray(subjectSentence, subjectLabel).ConfigureAwait(false);
            await _grannyRepository.AddHeaderArray(headerSentence, headerLabel).ConfigureAwait(false);
            await _grannyRepository.AddBodyArray(bodySentence, bodyLabel).ConfigureAwait(false);

            foreach (var uri in uris)
            {
                await _grannyRepository.AddUri(uri, (int)UriEvaluationCategory.NotEvaluated).ConfigureAwait(false);
            }

            await _mailKitRepository.MarkEmailAsSeen(uniqueId).ConfigureAwait(false);

            var trainedEmailAddedResultViewModel = new TrainedEmailAddedResultViewModel()
            {
                SubjectSentence = subjectSentence,
                HeaderSentence = headerSentence,
                BodySentence = bodySentence,
                Uris = uris
            };

            return View(trainedEmailAddedResultViewModel);
        }

        public async Task<IActionResult> GenerateModel()
        {
            //var (bodySentences, bodyLabels) = await _grannyRepository.GetBodyTrainData().ConfigureAwait(false);
            //var (headerSentences, headerLabels) = await _grannyRepository.GetHeaderTrainData().ConfigureAwait(false);
            //var (subjectSentences, subjectLabels) = await _grannyRepository.GetSubjectTrainData().ConfigureAwait(false);
            var (subjectSentences, subjectLabels) = await _grannyRepository.GetRawEmailTrainData().ConfigureAwait(false);

            //var bodyTotalWords = bodySentences
            //    .SelectMany(sentence => sentence.Words)
            //    .Distinct()
            //    .Count();
            //var headerTotalWords = headerSentences
            //    .SelectMany(sentence => sentence.Words)
            //    .Distinct()
            //    .Count();
            //var subjectTotalWords = subjectSentences
            //    .SelectMany(sentence => sentence.Words)
            //    .Distinct()
            //    .Count();
            var subjectTotalWords = subjectSentences
                .SelectMany(sentence => sentence.Words)
                .Distinct()
                .Count();

            //var bodyMaxLengthSize = bodySentences
            //    .Select(sentence => sentence.Words.Count())
            //    .Max();
            //var headerMaxLengthSize = headerSentences
            //    .Select(sentence => sentence.Words.Count())
            //    .Max();
            //var subjectMaxLengthSize = subjectSentences
            //    .Select(sentence => sentence.Words.Count())
            //    .Max();
            var subjectMaxLengthSize = subjectSentences
                .Select(sentence => sentence.Words.Count())
                .Max();

            //var bodyFullSentences = bodySentences.Select(sentence => string.Join(" ", sentence.Words));
            //var headerFullSentences = headerSentences.Select(sentence => string.Join(" ", sentence.Words));
            //var subjectFullSentences = subjectSentences.Select(sentence => string.Join(" ", sentence.Words));
            var subjectFullSentences = subjectSentences.Select(sentence => string.Join(" ", sentence.Words));

            //var bodyFullLabels = bodyLabels.Select(label => label.Value);
            //var headerFullLabels = headerLabels.Select(label => label.Value);
            //var subjectFullLabels = subjectLabels.Select(label => label.Value);
            var subjectFullLabels = subjectLabels.Select(label => label.Value);

            //_grannyModelAccessorService.GenerateBodyModel(
            //    bodyFullSentences,
            //    bodyFullLabels,
            //    bodyFullSentences,
            //    bodyFullLabels,
            //    bodyTotalWords,
            //    bodyMaxLengthSize).ConfigureAwait(false);
            //_grannyModelAccessorService.GenerateHeaderModel(
            //    headerFullSentences,
            //    headerFullLabels,
            //    headerFullSentences,
            //    headerFullLabels,
            //    headerTotalWords,
            //    headerMaxLengthSize).ConfigureAwait(false);
            //_grannyModelAccessorService.GenerateSubjectModel(
            //    subjectFullSentences,
            //    subjectFullLabels,
            //    subjectFullSentences,
            //    subjectFullLabels,
            //    subjectTotalWords,
            //    subjectMaxLengthSize).ConfigureAwait(false);

            _grannyModelAccessorService.GenerateRawEmailModel(
                subjectFullSentences,
                subjectFullLabels,
                subjectFullSentences,
                subjectFullLabels,
                subjectTotalWords,
                subjectMaxLengthSize).ConfigureAwait(false);

            return View();
        }

        public async Task<IActionResult> PredictEmail(string messageId)
        {
            var (mail, uniqueId) = await _mailKitRepository.GetUnreadEmailByIdAsync(messageId).ConfigureAwait(false);

            var headerEmailSentence = new StringBuilder();

            var authorizationResultsHeaders = mail.Headers.Where(header => header.Field == "Authentication-Results");
            var authorizationResultsHeadersValues = authorizationResultsHeaders.Select(authorizationResultsHeader => authorizationResultsHeader.Value).ToList();

            var senderPolicyFrameworkProtocolsResults = _informationExtractorService.GetSenderPolicyFrameworkResults(authorizationResultsHeadersValues);
            var domainKeysIdentifiedMailProtocolsResults = _informationExtractorService.GetDomainKeysIdentifiedMailResults(authorizationResultsHeadersValues);
            var domainBasedMessageAuthenticationReportingConformanceProtocolsResults = _informationExtractorService.GetDomainBasedMessageAuthenticationReportingConformanceResults(authorizationResultsHeadersValues);

            //Add attachments info
            foreach (var attachment in mail.Attachments)
            {
                var file = attachment.ContentDisposition.FileName;
                var fileSize = attachment.ContentDisposition.Size;
                var fileName = Path.GetFileName(file).ToLower();
                var fileExtension = Path.GetExtension(file).ToLower();
                headerEmailSentence.Append($"attachment filename {fileName} extension {fileExtension} size {fileSize} ");
            }

            //Add protocol result info
            foreach (var protocolResult in senderPolicyFrameworkProtocolsResults)
            {
                headerEmailSentence.Append($"spfProtocol result {protocolResult} ");
            }

            foreach (var protocolResult in domainKeysIdentifiedMailProtocolsResults)
            {
                headerEmailSentence.Append($"dkimProtocol result {protocolResult} ");
            }

            foreach (var protocolResult in domainBasedMessageAuthenticationReportingConformanceProtocolsResults)
            {
                headerEmailSentence.Append($"dmarcProtocol result {protocolResult} ");
            }

            //Add body info
            var bodyHtmlText = mail.HtmlBody;

            var bodySentence = _textPreparerService.Prepare(GetBodyText(bodyHtmlText)).ToList();

            var bodyFullSentence = string.Join(" ", bodySentence);

            var bodyPrediction = await _grannyModelAccessorService.PredictRawEmail(bodyFullSentence).ConfigureAwait(false);

            var predictionResultViewModel = new PredictionResultViewModel()
            {
                BodyPredictionResult = bodyPrediction.First(),
                HeaderPredictionResult = 0,
                SubjectPredictionResult = 0
            };

            await _mailKitRepository.MarkEmailAsSeen(uniqueId).ConfigureAwait(false);

            return View(predictionResultViewModel);
        }

        public async Task<IActionResult> GetLabelsCount()
        {
            var (ok, bad) = await _grannyRepository.GetEmailClassificationTypesCount().ConfigureAwait(false);
            return View();
        }

        public async Task<IActionResult> LoadExcelFileData()
        {
            var results = _rawEmailsRepository.GetEmailsFromFile(@"C:\Users\user\Downloads\SMSSpamCollection.xlsx");

            await _grannyRepository.AddRawEmail(
                results.Select(result => _textPreparerService.Prepare(result.EmailText).ToList()).ToList(),
                results.Select(result => result.Label).ToList()).ConfigureAwait(false);

            return View();
        }

        private string GetBodyText(string html)
        {
            var bodyEmailSentence = new StringBuilder();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Remove script & style nodes
            doc.DocumentNode.Descendants().Where(n => n.Name == "script" || n.Name == "style").ToList().ForEach(n => n.Remove());

            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//text()[normalize-space(.) != '']"))
            {
                bodyEmailSentence.Append($"{node.InnerText.Trim()} ");
            }

            return bodyEmailSentence.ToString();
        }
    }
}
