using System.Collections.Generic;
using System.Threading.Tasks;
using Granny.Email.Application.Repository;
using MongoDB.Driver;

namespace Granny.Email.Infrastructure.Repository.Mongo
{
    public class GrannyMongoRepository : IGrannyRepository
    {
        private readonly string _connectionString;

        public GrannyMongoRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task AddSubjectArray(IEnumerable<string> array, int label)
        {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase("granny");
            var subjectCollection = database.GetCollection<Sentence>("subject_sentences");
            var subjectLabelCollection = database.GetCollection<Label>("subject_labels");

            await subjectCollection.InsertOneAsync(new Sentence { Words = array }).ConfigureAwait(false);
            await subjectLabelCollection.InsertOneAsync(new Label { Value = label }).ConfigureAwait(false);
        }

        public async Task AddHeaderArray(IEnumerable<string> array, int label)
        {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase("granny");
            var headerCollection = database.GetCollection<Sentence>("header_sentences");
            var headerLabelCollection = database.GetCollection<Label>("header_labels");

            await headerCollection.InsertOneAsync(new Sentence { Words = array }).ConfigureAwait(false);
            await headerLabelCollection.InsertOneAsync(new Label { Value = label }).ConfigureAwait(false);
        }

        public async Task AddBodyArray(IEnumerable<string> array, int label)
        {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase("granny");
            var bodyCollection = database.GetCollection<Sentence>("body_sentences");
            var bodyLabelCollection = database.GetCollection<Label>("body_labels");

            await bodyCollection.InsertOneAsync(new Sentence { Words = array }).ConfigureAwait(false);
            await bodyLabelCollection.InsertOneAsync(new Label { Value = label }).ConfigureAwait(false);
        }

        public async Task AddUri(string uri, int secure)
        {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase("granny");
            var collection = database.GetCollection<EmailInUri>("uris");
            var existingUri = await collection.FindAsync(x => x.Uri.Equals(uri)).ConfigureAwait(false);

            if (!await existingUri.AnyAsync())
            {
                await collection.InsertOneAsync(new EmailInUri { Secured = secure, Uri = uri }).ConfigureAwait(false);
            }
        }
    }
}
