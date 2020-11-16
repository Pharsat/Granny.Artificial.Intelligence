using System.Collections.Generic;

namespace Granny.Email.Application.EmailInformationExtractor
{
    public interface IInformationExtractorService
    {
        IEnumerable<string> GetSenderPolicyFrameworkResults(IEnumerable<string> headers);
        IEnumerable<string> GetDomainKeysIdentifiedMailResults(IEnumerable<string> headers);
        IEnumerable<string> GetDomainBasedMessageAuthenticationReportingConformanceResults(IEnumerable<string> headers);
    }
}