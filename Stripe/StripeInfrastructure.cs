
using Stripe;

namespace latest.Stripe;

public static class StripeInfrastructure {
    public static IServiceCollection AddStripeInfrastructure(this IServiceCollection services, IConfiguration configuration) {
        StripeConfiguration.ApiKey = configuration.GetValue<string>("StripeSettings:SecretKey");
        return services.AddScoped<IStripeAppService, StripeAppService>();   
    }
}