using System.Collections.Generic;
using System.ComponentModel;

namespace Granny.Email.WebApp.Models.Train
{
    public class TrainedEmailAddedResultViewModel
    {
        [DisplayName("Subject words")]
        public IEnumerable<string> SubjectSentence { get; set; }

        [DisplayName("Header words")]
        public IEnumerable<string> HeaderSentence { get; set; }

        [DisplayName("Body words")]
        public IEnumerable<string> BodySentence { get; set; }

        [DisplayName("Obtained uris")]
        public IEnumerable<string> Uris { get; set; }
    }
}
