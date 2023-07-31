using Stripe;
using Stripe.Checkout;

namespace latest.Stripe;

public interface IStripeAppService {
    Account CreateExpressAccount(string email);

    AccountLink CreateAccountLink(string stripeId, string userId, string token);

    Account GetAccount(string id);
    Session CreateAccountPayment(string userId, string token); 
    Session GetSession(string id);
    Session CreatePaymentRequest(int price, string destination, string id, int cmid);
}