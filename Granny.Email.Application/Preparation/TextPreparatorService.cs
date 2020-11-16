using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Granny.Email.Application.Preparation
{
    public class TextPreparerService : ITextPreparerService
    {
        private readonly IEnumerable<string> _stopWords;
        private readonly IEnumerable<string> _htmlStopWords;

        public TextPreparerService(IStopWordsServices stopWordsServices)
        {
            _stopWords = stopWordsServices.GetSpanishStopWords();
            _htmlStopWords = stopWordsServices.GetHtmlStopWords();
        }

        public IEnumerable<string> Prepare(string text)
        {
            var loweredText = ConvertToLower(text);
            var cleanedText = RemoveSpecialCharacters(loweredText);
            var textArray = ConvertToArray(cleanedText);
            var textArrayWithoutNumbers = RemoveNumbers(textArray);
            var textArrayWithoutStopWords = RemoveStopWords(textArrayWithoutNumbers);
            var textArrayWithoutEmpties = RemoveBlanks(textArrayWithoutStopWords);
            var textArrayWithoutHtmlTags = RemoveHtmlStopWords(textArrayWithoutEmpties);

            return textArrayWithoutHtmlTags;
        }

        private string ConvertToLower(string text)
        {
            return text.ToLower();
        }

        private string RemoveSpecialCharacters(string text)
        {
            var onlyAlphanumericCharactersRegex = new Regex("[^a-z0-9 áéíóúüñ]");

            return onlyAlphanumericCharactersRegex.Replace(text, string.Empty);
        }

        private IEnumerable<string> ConvertToArray(string text)
        {
            return text.Split(" ");
        }

        private IEnumerable<string> RemoveNumbers(IEnumerable<string> texts)
        {
            return texts
                .Where(text => text.All(character => !char.IsDigit(character)));
        }

        private IEnumerable<string> RemoveStopWords(IEnumerable<string> texts)
        {
            return texts.Where(text => !_stopWords.Contains(text));
        }

        private IEnumerable<string> RemoveHtmlStopWords(IEnumerable<string> texts)
        {
            return texts.Where(text => !_htmlStopWords.Contains(text));
        }

        private IEnumerable<string> RemoveBlanks(IEnumerable<string> texts)
        {
            return texts.Where(text => !string.IsNullOrEmpty(text));
        }
    }
}
