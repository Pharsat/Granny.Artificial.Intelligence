using MongoDB.Bson;

namespace Granny.Email.Application.Repository
{
    public class EmailInUri
    {
        public ObjectId Id { get; set; }
        public string Uri { get; set; }
        public int Secured { get; set; }
    }
}