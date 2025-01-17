using System.Runtime.InteropServices;
using Billing.Infrastructure.ExternalServices.SeedWorks;
using Billing.Infrastructure.ExternalServices.WalletService.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Billing.Infrastructure.ExternalServices.WalletService;

public class WalletProviderService(
    IHttpClientFactory httpClientFactory,
    ILogger<WalletProviderService> logger,
    IOptions<ExternalServicesSetting> externalServicesOptions) : IWalletProviderService
{
    private readonly WalletSetting _walletSetting =
        externalServicesOptions?.Value.Wallet ??
        throw new ArgumentNullException(nameof(WalletSetting));

    public async Task<CreateWalletResponseModel> CreateUserWallet(CreateWalletRequestModel createRequest, CancellationToken cancellationToken)
    {
        var callApiRequest = CallApiRequest.Create(_walletSetting.CreateWallet, HttpMethod.Post);
        
        var client = httpClientFactory.CreateClient(_walletSetting.Name);
        
        var response =  await client.SendAsync<CreateWalletResponseModel>(callApiRequest,
            logger: logger,
            cancellationToken: cancellationToken);
       
        
        if (response is not { DeserializationSucceed: true, RequestSucceed: true } ||
            response.Data is null)
        {
            throw new ExternalException($"Error in calling service CreateUserWallet");
        }
       
        return response.Data;
    }

    public async Task<WalletResponseModel> GetUserWalletAsync(Guid walletId, CancellationToken cancellationToken)
    {
        var action = string.Format(_walletSetting.GetWallet, walletId);
        var callApiRequest = CallApiRequest.Create(action, HttpMethod.Get);
        
        var client = httpClientFactory.CreateClient(_walletSetting.Name);
        
        var response =  await client.SendAsync<WalletResponseModel>(callApiRequest,
            logger: logger,
            cancellationToken: cancellationToken);
       
        
        if (response is not { DeserializationSucceed: true, RequestSucceed: true } ||
            response.Data is null)
        {
            throw new ExternalException($"Error in calling service GetUserWallet with id {walletId}");
        }
       
        return response.Data;
    }

    public async Task<WithdrawResponseModel> WithdrawAsync(Guid walletId,WithdrawRequestModel withdrawRequest, CancellationToken cancellationToken)
    {
        var action = string.Format(_walletSetting.Withdraw,walletId);
        var callApiRequest = CallApiRequest.Create(action, HttpMethod.Put);
        
        var client = httpClientFactory.CreateClient(_walletSetting.Name);
        
        var response =  await client.SendAsync<WithdrawResponseModel>(callApiRequest,
            logger: logger,
            cancellationToken: cancellationToken);
       
        
        if (response is not { DeserializationSucceed: true, RequestSucceed: true } ||
            response.Data is null)
        {
            throw new ExternalException($"Error in calling service Withdraw with id {walletId}");
        }
       
        return response.Data;
    }
}