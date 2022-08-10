using ConsoleAPIClient;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;

Console.WriteLine($"Making call.....");
RunAsync().GetAwaiter().GetResult();
Console.ReadLine();



static async Task RunAsync()
{
    AuthConfig config = AuthConfig.ReadJsonFromFile("appsettings.json");

    IConfidentialClientApplication app;

    app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
        .WithClientSecret(config.ClientSecret)
        .WithAuthority(new Uri(config.Authority))
        .Build();

    string[] resourceIds = new string[] { config.ResourceId };

    AuthenticationResult authResult = null;

    try
    {
        authResult = await app.AcquireTokenForClient(resourceIds).ExecuteAsync();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Token recieved...");
        Console.WriteLine($"Token is: {authResult.AccessToken}");
        Console.ResetColor();
    }
    catch (MsalClientException ex)
    {

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.Message);
        Console.ResetColor();

    }
    if (!string.IsNullOrEmpty(authResult.AccessToken))
    {
        var client = new HttpClient();
        var defRequestHeader = client.DefaultRequestHeaders;

        if(defRequestHeader.Accept == null || defRequestHeader.Accept.Any(x=>x.MediaType == "application/json"))
        {
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
        defRequestHeader.Authorization = new AuthenticationHeaderValue("bearer", authResult.AccessToken);

        HttpResponseMessage response = await client.GetAsync(config.BaseAddress);

        if(response.IsSuccessStatusCode)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            string json = await response.Content.ReadAsStringAsync();
            Console.WriteLine(json);
        }
    }
}