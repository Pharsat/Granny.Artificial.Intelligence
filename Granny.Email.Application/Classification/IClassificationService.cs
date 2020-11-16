using System;
using System.Collections.Generic;

namespace Granny.Email.Application.Classification
{
    public interface IClassificationService
    {
        ClassificationValue CalculateReceivedDateDistance(IEnumerable<DateTimeOffset> receivedDates);
        ClassificationValue CalculateReceivedQuantity(int receivedQuantity);
        ClassificationValue CalculateSenderPolicyFrameworkProtocolAssessment(IEnumerable<string> results);
        ClassificationValue CalculateDomainKeysIdentifiedMailProtocolAssessment(IEnumerable<string> results);
        ClassificationValue CalculateDomainBasedMessageAuthenticationReportingConformanceProtocolsAssessment(IEnumerable<string> results);
        ClassificationValue CalculateFileExtensionTypes(IEnumerable<string> results);
        ClassificationValue CalculateFromAndMessageIdDomains(string messageId, IEnumerable<string> fromAddresses);
    }
}