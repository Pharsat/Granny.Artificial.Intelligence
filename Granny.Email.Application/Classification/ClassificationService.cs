using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace Granny.Email.Application.Classification
{
    public class ClassificationService : IClassificationService
    {
        public ClassificationValue CalculateReceivedDateDistance(IEnumerable<DateTimeOffset> receivedDates)
        {
            var receivedDatesAsNumbers = receivedDates.Select(date => date.LocalDateTime.ToOADate()).ToList();
            var maxDateNumber = receivedDatesAsNumbers.Max();
            var incrementalValues = receivedDatesAsNumbers.Select(number => (maxDateNumber - number) / number).ToList();
            var maximumIncrementalValue = incrementalValues.Max();
            if (maximumIncrementalValue < 0.00001)
            {
                return ClassificationValue.LowRisk;
            }

            if (maximumIncrementalValue < 0.00100)
            {
                return ClassificationValue.Neutral;
            }

            return ClassificationValue.HighRisk;
        }

        public ClassificationValue CalculateReceivedQuantity(int receivedQuantity)
        {
            if (receivedQuantity <= 1)
            {
                return ClassificationValue.LowRisk;
            }

            if (receivedQuantity <= 2)
            {
                return ClassificationValue.Neutral;
            }

            return ClassificationValue.HighRisk;
        }

        public ClassificationValue CalculateSenderPolicyFrameworkProtocolAssessment(IEnumerable<string> results)
        {
            /* https://en.wikipedia.org/wiki/Sender_Policy_Framework#:~:text=Sender%20Policy%20Framework%20(SPF)%20is,the%20delivery%20of%20the%20email.&text=The%20list%20of%20authorized%20sending,DNS%20records%20for%20that%20domain.
             * pass
             * neutral
             * softfail
             * fail
             */

            if (results.Any(result => result == "fail") ||
                results.Any(result => result == "softfail"))
            {
                return ClassificationValue.HighRisk;
            }

            if (results.Any(result => result == "neutral"))
            {
                return ClassificationValue.Neutral;
            }

            return ClassificationValue.LowRisk;
        }

        public ClassificationValue CalculateDomainKeysIdentifiedMailProtocolAssessment(IEnumerable<string> results)
        {
            /* https://www.emailonacid.com/blog/article/email-development/what_is_dkim_everything_you_need_to_know_about_digital_signatures/
             * pass
             * fail
             * none
             * policy
             * neutral 
             * temperror
             * permerror 
             */

            if (results.Any(result => result == "fail") ||
                results.Any(result => result == "policy") ||
                results.Any(result => result == "neutral") ||
                results.Any(result => result == "permerror"))
            {
                return ClassificationValue.HighRisk;
            }

            if (results.Any(result => result == "temperror") ||
                results.Any(result => result == "none"))
            {
                return ClassificationValue.HighRisk;
            }

            return ClassificationValue.LowRisk;
        }

        public ClassificationValue CalculateDomainBasedMessageAuthenticationReportingConformanceProtocolsAssessment(IEnumerable<string> results)
        {
            /* https://docs.microsoft.com/en-us/microsoft-365/security/office-365-security/anti-spam-message-headers?view=o365-worldwide
             * pass
             * fail
             * bestguesspass
             * none
             */

            if (results.Any(result => result == "fail"))
            {
                return ClassificationValue.HighRisk;
            }

            if (results.Any(result => result == "none") ||
                results.Any(result => result == "bestguesspass"))
            {
                return ClassificationValue.Neutral;
            }

            return ClassificationValue.LowRisk;
        }

        public ClassificationValue CalculateFileExtensionTypes(IEnumerable<string> results)
        {
            /*
             *
             */

            if (results.Any(result => result == "zip") ||
                results.Any(result => result == "rar"))
            {
                return ClassificationValue.HighRisk;
            }

            if (results.Any(result => result == "doc") ||
                results.Any(result => result == "xls") ||
                results.Any(result => result == "ppt"))
            {
                return ClassificationValue.Neutral;
            }

            return ClassificationValue.LowRisk;
        }

        public ClassificationValue CalculateFromAndMessageIdDomains(string messageId, IEnumerable<string> fromAddresses)
        {
            var messageIdEmailAddress = new MailAddress(messageId);
            var fromEmailAddresses = fromAddresses.Select(fromAddress => new MailAddress(fromAddress)).ToList();

            if (fromEmailAddresses.All(fromEmailAddress => fromEmailAddress.Host == messageIdEmailAddress.Host))
            {
                return ClassificationValue.LowRisk;
            }

            if (fromEmailAddresses.Any(fromEmailAddress => fromEmailAddress.Host != messageIdEmailAddress.Host))
            {
                return ClassificationValue.HighRisk;
            }

            return ClassificationValue.Neutral;
        }
    }
}