using PayPal.Api;

namespace TechXpress_DepiGraduation.Data.Services;

public class PayPalConfiguration 
{
    public static APIContext GetAPIContext(IConfiguration configuration)
    {
        var paypalConfig = new Dictionary<string, string>
        {
            { "mode", configuration["PayPal:mode"] },
            { "connectionTimeout", configuration["PayPal:connectionTimeout"] },
            { "requestRetries", configuration["PayPal:requestRetries"] },
            { "clientId", configuration["PayPal:clientId"] },
            { "clientSecret", configuration["PayPal:clientSecret"] }
        };

        var accessToken = new OAuthTokenCredential(paypalConfig["clientId"], paypalConfig["clientSecret"], paypalConfig).GetAccessToken();
        Console.WriteLine($"PayPal Access Token: {accessToken}");
        return new APIContext(accessToken) { Config = paypalConfig };
    }
    
}