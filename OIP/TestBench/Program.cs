using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TestBench
{
    internal class Program
    {
        public const string TEST_SIGNING_JWK = "eyJhbGciOiJFUzI1NksiLCJjcnYiOiJzZWNwMjU2azEiLCJkIjoieFI3ZXlEenhTVXpPYllnWmxFUjFqbmkybjE0MkhxTFZHRDlhVF9qcXhJMCIsImt0eSI6IkVDIiwidXNlIjoic2lnIn0";
        public const string TEST_SIGNING_XPUB = "xpub6EVyMwjbyrQEwL5Xzt9ffaWdphw5AjUH2Y8dgZsnuvVQ8Bk6t62or1uTudxPYp99Zj9eao1vMopSXQUt7rR18fem1DbT5daw69RrruPJWnv";

        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddTransient<TestSampleCreator>();
                        services.AddTransient<TestSamplePost>();
                    })
                    .Build();

            Console.WriteLine("\r\n\r\n------- Sample Creator ---------- \r\n");
            host.Services.GetRequiredService<TestSampleCreator>().Run();

            Console.WriteLine("\r\n\r\n------- Sample Post ---------- \r\n");
            host.Services.GetRequiredService<TestSamplePost>().Run();
        }
    }
}
