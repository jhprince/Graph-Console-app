using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Extensions.Configuration;
using Helpers;
using Microsoft.Kiota.Abstractions.Authentication;
using Azure.Identity;

namespace graphconsoleapp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = LoadAppSettings();
            if (config == null)
            {
                Console.WriteLine("Invalid appsettings.json file.");
                return;
            }
            var client = GetAuthenticatedGraphClient(config);
            var profileResponse = client.Users["6e157bb8-c39e-4743-94e2-5707e96c11a1"].GetAsync().GetAwaiter().GetResult();
            Console.WriteLine("welcometo" + profileResponse.DisplayName);
            // request 1 - get user's files
            var results = client.Users["6e157bb8-c39e-4743-94e2-5707e96c11a1"].Drive.GetAsync().GetAwaiter().GetResult().Root.Children;

            foreach (var file in results)
            {
                Console.WriteLine(file.Id + ": " + file.Name);
            }
        }

        private static IConfigurationRoot? LoadAppSettings()
        {
            try
            {
                var config = new ConfigurationBuilder()
                                  .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                                  .AddJsonFile("appsettings.json", false, true)
                                  .Build();

                if (string.IsNullOrEmpty(config["applicationId"]) ||
                    string.IsNullOrEmpty(config["tenantId"]))
                {
                    return null;
                }

                return config;
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
        }
        private static IAuthenticationProvider CreateAuthorizationProvider(IConfigurationRoot config)
        {
            var clientId = config["applicationId"];
            var authority = $"https://login.microsoftonline.com/{config["tenantId"]}/v2.0";

            List<string> scopes = new List<string>();
            scopes.Add("https://graph.microsoft.com/.default");

            var cca = PublicClientApplicationBuilder.Create(clientId)
                                                    .WithAuthority(authority)
                                                    .WithDefaultRedirectUri()
                                                    .Build();

            return MsalAuthenticationProvider.GetInstance(cca, scopes.ToArray());
        }
        private static GraphServiceClient GetAuthenticatedGraphClient(IConfigurationRoot config)
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var tenantId = config["tenantId"];
            var clientId = config["applicationId"];
            var clientSecret = config["clientSceret"];
            
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            };

            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);
            return graphClient;
        }
    }
}