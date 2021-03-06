﻿using System.Collections.Generic;
using System.Linq;
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

        public async Task<(IEnumerable<Sentence> Sentences, IEnumerable<Label> Labels)> GetHeaderTrainData()
        {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase("granny");
            var sentencesCollection = database.GetCollection<Sentence>("header_sentences");
            var labelsCollection = database.GetCollection<Label>("header_labels");
            var sentences = await sentencesCollection.FindAsync(Builders<Sentence>.Filter.Empty).ConfigureAwait(false);
            var labels = await labelsCollection.FindAsync(Builders<Label>.Filter.Empty).ConfigureAwait(false);

            return (sentences.ToList(), labels.ToList());
        }

        public async Task<(IEnumerable<Sentence> Sentences, IEnumerable<Label> Labels)> GetSubjectTrainData()
        {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase("granny");
            var sentencesCollection = database.GetCollection<Sentence>("subject_sentences");
            var labelsCollection = database.GetCollection<Label>("subject_labels");
            var sentences = await sentencesCollection.FindAsync(Builders<Sentence>.Filter.Empty).ConfigureAwait(false);
            var labels = await labelsCollection.FindAsync(Builders<Label>.Filter.Empty).ConfigureAwait(false);

            return (sentences.ToList(), labels.ToList());
        }

        public async Task<(IEnumerable<Sentence> Sentences, IEnumerable<Label> Labels)> GetBodyTrainData()
        {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase("granny");
            var sentencesCollection = database.GetCollection<Sentence>("body_sentences");
            var labelsCollection = database.GetCollection<Label>("body_labels");
            var sentences = await sentencesCollection.FindAsync(Builders<Sentence>.Filter.Empty).ConfigureAwait(false);
            var labels = await labelsCollection.FindAsync(Builders<Label>.Filter.Empty).ConfigureAwait(false);

            return (sentences.ToList(), labels.ToList());
        }


        public async Task<(int OkEmailsCount, int BadEmailsCount)> GetEmailClassificationTypesCount()
        {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase("granny");
            var labelsCollection = database.GetCollection<Label>("body_labels");
            var badLabels = await labelsCollection.FindAsync(label => label.Value == 1).ConfigureAwait(false);
            var goodLabels = await labelsCollection.FindAsync(label => label.Value == 0).ConfigureAwait(false);
            return (goodLabels.ToList().Count, badLabels.ToList().Count);
        }

        public async Task AddRawEmail(List<List<string>> sentences, List<int> labels)
        {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase("granny");
            var headerCollection = database.GetCollection<Sentence>("raw_email_sentences");
            var headerLabelCollection = database.GetCollection<Label>("raw_email_labels");

            await headerCollection.InsertManyAsync(sentences.Select(sentence => new Sentence {Words = sentence})).ConfigureAwait(false);
            await headerLabelCollection.InsertManyAsync(labels.Select(label=> new Label { Value = label })).ConfigureAwait(false);
        }

        public async Task<(IEnumerable<Sentence> Sentences, IEnumerable<Label> Labels)> GetRawEmailTrainData()
        {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase("granny");
            var sentencesCollection = database.GetCollection<Sentence>("raw_email_sentences");
            var labelsCollection = database.GetCollection<Label>("raw_email_labels");
            var sentences = await sentencesCollection.FindAsync(Builders<Sentence>.Filter.Empty).ConfigureAwait(false);
            var labels = await labelsCollection.FindAsync(Builders<Label>.Filter.Empty).ConfigureAwait(false);

            return (sentences.ToList(), labels.ToList());
        }
    }
}
