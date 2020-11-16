namespace Granny.Email.Infrastructure
{
    public class EmailReceiverConfiguration
    {
        public string MailServer { get; set; }
        public int Port { get; set; }
        public bool Ssl { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }
}