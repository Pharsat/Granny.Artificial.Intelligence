using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Granny.Email.WebApp.Models.MailReceiver
{
    public class InboxViewModel
    {
        public string EmailAddress { get; set; }

        [UIHint("MailReceiver\\EmailList")]
        public IEnumerable<MailViewModel> EmailList { get; set; }
    }
}
