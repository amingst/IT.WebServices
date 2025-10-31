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
    internal class TestSampleCreator
    {
        public void Run()
        {
            var creatorId = Guid.NewGuid();
            var imageId = Guid.NewGuid();

            var basicCreator = new BasicRecordTemplate()
            {
                Name = "test user",
                Description = "test description",
                Date = DateTimeOffset.UtcNow,
                Language = 37,
                NSFW = false,
                Avatar = "#" + imageId,
                WebUrl = "https://invertedtech.org/test",
            };

            var creator = new CreatorRegistrationRecordTemplate()
            {
                Handle = "test",
                Surname = "McTester",
                SigningXpub = Program.TEST_SIGNING_XPUB,
            };

            var basicImage = new BasicRecordTemplate()
            {
                Name = "Unknown Person",
                Date = DateTimeOffset.UtcNow,
                NSFW = false,
            };

            var image = new ImageRecordTemplate()
            {
                ArweaveAddress = "did:arweave:YMokIpCziHygHQP67uyeIIsp5yL8BxYcBGzdq1Zu3Iw",
                Filename = "Unknown-person.gif",
                ContentType = "image/gif",
                Size = 1040,
                Width = 280,
                Length = 280,
                Creator = "#" + creatorId,
            };

            var dataToSign = new DataForSignature();
            dataToSign.Tags.Add(new() { Name = DataTagNvPair.CREATOR, Value = "self" });
            dataToSign.Fragments.Add(new()
            {
                Id = creatorId.ToString(),
                DataType = "Record",
                RecordType = "creatorRegistration",
                Records = [basicCreator, creator],
            });
            dataToSign.Fragments.Add(new()
            {
                Id = imageId.ToString(),
                DataType = "Record",
                RecordType = "image",
                Records = [basicImage, image],
            });

            SigningService.AddSignatureTag(dataToSign, Program.TEST_SIGNING_JWK);
            var json = JsonSerializer.Serialize(dataToSign, new JsonSerializerOptions() { WriteIndented = true });

            Console.WriteLine(json);
        }
    }
}
