using FCG.Core.Messages.Integration;
using FCG.Payments.Models;
using FCG.Payments.Models.Enums;

namespace FCG.Payments.Domain.Extensions;

public static class OrderEventExtensions
{
    public static Payment ToPayment(this OrderPlacedEvent message)
    {
        return new Payment(
                        message.OrderId,
                        (PaymentMethod)message.PaymentMethod,
                        message.Amount,
                        new CreditCard(
                            message.CardName,
                            message.CardNumber,
                            message.ExpirationDate,
                            message.Cvv)
                    );
    }
}
