namespace AzureMasterclass.Domain.Options;

public class AzureAdOptions
{
    public const string AzureAd = "AzureAd";

    public string Instance { get; set; } = "";
    public string TenantId { get; set; } = "";
    public string ClientId { get; set; } = "";
}