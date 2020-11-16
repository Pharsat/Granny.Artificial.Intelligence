using System.Collections.Generic;
using System.Threading.Tasks;
using Granny.Email.Application.Repository;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Granny.Email.Infrastructure
{
    public class MailKitMailRepository : IMailRepository
    {
        private readonly string _mailServer, _login, _password;
        private readonly int _port;
        private readonly bool _ssl;

        public MailKitMailRepository(IOptions<EmailReceiverConfiguration> emailReceiverConfiguration)
        {
            _mailServer = emailReceiverConfiguration.Value.MailServer;
            _port = emailReceiverConfiguration.Value.Port;
            _ssl = emailReceiverConfiguration.Value.Ssl;
            _login = emailReceiverConfiguration.Value.Login;
            _password = emailReceiverConfiguration.Value.Password;
        }

        public async Task<(MimeMessage Message, UniqueId Id)> GetUnreadEmailByIdAsync(string messageId)
        {
            using var client = new ImapClient();

            await client.ConnectAsync(_mailServer, _port, _ssl).ConfigureAwait(false);

            client.AuthenticationMechanisms.Remove("XOAUTH2");

            await client.AuthenticateAsync(_login, _password).ConfigureAwait(false);
            
            var inbox = client.Inbox;
           
            await inbox.OpenAsync(FolderAccess.ReadOnly).ConfigureAwait(false);

            var results = await inbox.SearchAsync(SearchQuery.HeaderContains("Message-Id", messageId)).ConfigureAwait(false);
            
            MimeMessage message = null;
            var uniqueId = new UniqueId();

            foreach (var id in results)
            {
                message = await inbox.GetMessageAsync(id).ConfigureAwait(false);
                uniqueId = id;
            }

            await client.DisconnectAsync(true).ConfigureAwait(false);

            return (message, uniqueId);
        }

        public async Task<IEnumerable<MimeMessage>> GetUnreadMailsAsync()
        {
            var messages = new List<MimeMessage>();

            using var client = new ImapClient();

            await client.ConnectAsync(_mailServer, _port, _ssl).ConfigureAwait(false);

            // Note: since we don't have an OAuth2 token, disable
            // the XOAUTH2 authentication mechanism.
            client.AuthenticationMechanisms.Remove("XOAUTH2");

            await client.AuthenticateAsync(_login, _password).ConfigureAwait(false);

            // The Inbox folder is always available on all IMAP servers...
            var inbox = client.Inbox;

            await inbox.OpenAsync(FolderAccess.ReadOnly).ConfigureAwait(false);

            var results = await inbox.SearchAsync(SearchQuery.NotSeen).ConfigureAwait(false);

            foreach (var uniqueId in results)
            {
                var message = await inbox.GetMessageAsync(uniqueId).ConfigureAwait(false);

                messages.Add(message);

                //Mark message as read
                //inbox.AddFlags(uniqueId, MessageFlags.Seen, true);
            }

            await client.DisconnectAsync(true).ConfigureAwait(false);

            return messages;
        }

        public async Task<IEnumerable<MimeMessage>> GetAllMailsAsync()
        {
            var messages = new List<MimeMessage>();

            using var client = new ImapClient();

            await client.ConnectAsync(_mailServer, _port, _ssl).ConfigureAwait(false);

            // Note: since we don't have an OAuth2 token, disable
            // the XOAUTH2 authentication mechanism.
            client.AuthenticationMechanisms.Remove("XOAUTH2");

            await client.AuthenticateAsync(_login, _password).ConfigureAwait(false);

            // The Inbox folder is always available on all IMAP servers...
            var inbox = client.Inbox;
                
            await inbox.OpenAsync(FolderAccess.ReadOnly).ConfigureAwait(false);
                
            var results = await inbox.SearchAsync(SearchOptions.All, SearchQuery.NotSeen);
                
            foreach (var uniqueId in results.UniqueIds)
            {
                var message = await inbox.GetMessageAsync(uniqueId).ConfigureAwait(false);

                messages.Add(message);

                //Mark message as read
                //inbox.AddFlags(uniqueId, MessageFlags.Seen, true);
            }

            await client.DisconnectAsync(true).ConfigureAwait(false);

            return messages;
        }

        public async Task MarkEmailAsSeen(UniqueId uid)
        {
            using var client = new ImapClient();

            await client.ConnectAsync(_mailServer, _port, _ssl).ConfigureAwait(false);

            client.AuthenticationMechanisms.Remove("XOAUTH2");

            await client.AuthenticateAsync(_login, _password).ConfigureAwait(false);

            var inbox = client.Inbox;

            await inbox.AddFlagsAsync(new List<UniqueId>{ uid }, MessageFlags.Seen, true);

            await client.DisconnectAsync(true).ConfigureAwait(false);
        }

        public string GetEmailAddress()
        {
            return _login;
        }
    }
}
