using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Granny.Email.Application.EmailInformationExtractor
{
    public class InformationExtractorService : IInformationExtractorService
    {
        public IEnumerable<string> GetSenderPolicyFrameworkResults(IEnumerable<string> headers)
        {
            var senderPolicyFrameworkRegularExpressionPattern = "spf=[\\w]+";
            var senderPolicyFrameworkProtocolsResults = new List<string>();

            foreach (var header in headers)
            {
                foreach (Match match in Regex.Matches(header, senderPolicyFrameworkRegularExpressionPattern, RegexOptions.IgnoreCase))
                {
                    senderPolicyFrameworkProtocolsResults.Add(match.Value.Split("=")[1].ToLower());
                }
            }

            return senderPolicyFrameworkProtocolsResults;
        }

        public IEnumerable<string> GetDomainKeysIdentifiedMailResults(IEnumerable<string> headers)
        {
            var domainKeysIdentifiedMailExpressionPattern = "dkim=[\\w]+";
            var domainKeysIdentifiedMailProtocolsResults = new List<string>();

            foreach (var header in headers)
            {
                foreach (Match match in Regex.Matches(header, domainKeysIdentifiedMailExpressionPattern, RegexOptions.IgnoreCase))
                {
                    domainKeysIdentifiedMailProtocolsResults.Add(match.Value.Split("=")[1].ToLower());
                }
            }

            return domainKeysIdentifiedMailProtocolsResults;
        }

        public IEnumerable<string> GetDomainBasedMessageAuthenticationReportingConformanceResults(IEnumerable<string> headers)
        {
            var domainBasedMessageAuthenticationReportingConformanceRegularExpressionPattern = "dmarc=[\\w]+";
            var domainBasedMessageAuthenticationReportingConformanceProtocolsResults = new List<string>();

            foreach (var header in headers)
            {
                foreach (Match match in Regex.Matches(header, domainBasedMessageAuthenticationReportingConformanceRegularExpressionPattern, RegexOptions.IgnoreCase))
                {
                    domainBasedMessageAuthenticationReportingConformanceProtocolsResults.Add(match.Value.Split("=")[1].ToLower());
                }
            }

            return domainBasedMessageAuthenticationReportingConformanceProtocolsResults;
        }
    }
}