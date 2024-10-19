using Braintree;

namespace Store.Infrastructure;

public class BraintreeService : IBraintreeService
{
    public IConfiguration configuration;

    public BraintreeService(IConfiguration conf)
    {
        configuration = conf;
    }

    public IBraintreeGateway CreateGateway()
    {
        BraintreeGateway gateway = new BraintreeGateway()
        {
            Environment = Braintree.Environment.SANDBOX,
            MerchantId = configuration["BraintreeGateway:MerchantId"],
            PublicKey = configuration["BraintreeGateway:PublicKey"],
            PrivateKey = configuration["BraintreeGateway:PrivateKey"]
        };
        return gateway;
    }
}
