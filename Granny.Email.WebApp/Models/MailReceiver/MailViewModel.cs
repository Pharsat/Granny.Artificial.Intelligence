using System;

namespace Granny.Email.WebApp.Models.MailReceiver
{
    public class MailViewModel
    {
        public DateTimeOffset ReceivedDate { get; set; }
        public string Subject { get; set; }
        public string MessageId { get; set; }
    }
}
