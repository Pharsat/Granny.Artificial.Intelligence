using MongoDB.Bson;

namespace Granny.Email.Application.Repository
{
    public class Label
    {
        public ObjectId Id { get; set; }
        public int Value { get; set; }
    }
}