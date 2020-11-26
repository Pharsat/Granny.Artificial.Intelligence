using System;
using System.Collections.Generic;
using System.Text;

namespace Granny.Email.Application.Repository
{
    public interface IRawEmailsRepository
    {
        IEnumerable<(int Label, string EmailText)> GetEmailsFromFile(string filePath);
    }
}
