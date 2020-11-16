using System.Collections.Generic;
using System.Threading.Tasks;
using MailKit;
using MimeKit;

namespace Granny.Email.Application.Repository
{
    public interface IMailRepository
    {
        Task<(MimeMessage Message, UniqueId Id)> GetUnreadEmailByIdAsync(string messageId);
        Task<IEnumerable<MimeMessage>> GetUnreadMailsAsync();
        Task<IEnumerable<MimeMessage>> GetAllMailsAsync();
        Task MarkEmailAsSeen(UniqueId uid);
        string GetEmailAddress();
    }
}