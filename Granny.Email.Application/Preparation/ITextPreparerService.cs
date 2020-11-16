using System.Collections.Generic;

namespace Granny.Email.Application.Preparation
{
    public interface ITextPreparerService
    {
        IEnumerable<string> Prepare(string text);
    }
}