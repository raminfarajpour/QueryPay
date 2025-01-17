using Billing.Infrastructure.ExternalServices.SeedWorks;

namespace Billing.Infrastructure.ExternalServices.WalletService;

public class WalletSetting:HttpClientConfig
{
    public string GetWallet  { get; set; }
    public string Withdraw  { get; set; }
    public string CreateWallet  { get; set; }
}