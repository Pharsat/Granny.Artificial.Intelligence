using System.Collections.Generic;

namespace Granny.Email.Application.Preparation
{
    public interface IStopWordsServices
    {
        IEnumerable<string> GetSpanishStopWords();
        IEnumerable<string> GetHtmlStopWords();
    }
}
