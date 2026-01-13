using IT.WebServices.OIP.Models;
using IT.WebServices.OIP.Models.RecordTemplates;
using IT.WebServices.OIP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TestBench
{
    internal class TestSamplePost
    {
        public void Run()
        {
            var basic = new BasicRecordTemplate()
            {
                Name = "test post",
                Description = "test description",
                Date = DateTimeOffset.UtcNow,
                Language = 37,
                NSFW = false,
                WebUrl = "https://invertedtech.org/test",
            };

            var post = new PostRecordTemplate()
            {
                BylineWriter = "Me",
                BylineWritersTitle = "The Editor in Chief",
            };

            var dataToSign = new DataForSignature();
            dataToSign.Tags.Add(new() { Name = DataTagNvPair.CREATOR, Value = "did:arweave:LNdPGMUKwLw8_SBfi324wQlnpgvzrFQpRhLyAlV4lGo" });
            dataToSign.Fragments.Add(new()
            {
                Id = Guid.NewGuid().ToString(),
                DataType = "Record",
                RecordType = "post",
                Records = [basic, post],
            });

            SigningService.AddSignatureTag(dataToSign, Program.TEST_SIGNING_JWK);
            var json = JsonSerializer.Serialize(dataToSign, new JsonSerializerOptions() { WriteIndented = true });

            Console.WriteLine(json);
        }
    }
}
