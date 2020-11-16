using System.Collections.Generic;
using MongoDB.Bson;

namespace Granny.Email.Application.Repository
{
    public class Sentence
    {
        public ObjectId Id { get; set; }
        public IEnumerable<string> Words { get; set; }
    }
}