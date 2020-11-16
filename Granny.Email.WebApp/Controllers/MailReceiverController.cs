using System.Collections.Generic;
using System.Threading.Tasks;
using Granny.Email.Application.Repository;
using Granny.Email.Infrastructure;
using Granny.Email.WebApp.Models.MailReceiver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Granny.Email.WebApp.Controllers
{
    public class MailReceiverController : Controller
    {
        private readonly IMailRepository _mailKitRepository;

        public MailReceiverController(IMailRepository mailKitRepository)
        {
            _mailKitRepository = mailKitRepository;
        }

        // GET: MailReceiverController
        public async Task<IActionResult> Index()
        {
            var unreadEmails = await _mailKitRepository.GetUnreadMailsAsync().ConfigureAwait(false);
            var emails = new List<MailViewModel>();

            foreach (var email in unreadEmails)
            {
                emails.Add(new MailViewModel
                {
                    ReceivedDate = email.Date,
                    Subject = email.Subject,
                    MessageId = email.MessageId
                });

                //using var fileStream = System.IO.File.Create($@"C:\Users\user\source\repos\Granny.Artificial.Intelligence\Emails\{email.Date:yyyyMMddHHmmss}.eml");
                //email.WriteTo(fileStream);
            }

            var inboxViewModel = new InboxViewModel
            {
                EmailAddress = _mailKitRepository.GetEmailAddress(),
                EmailList = emails
            };

            return View(inboxViewModel);
        }
    }
}
