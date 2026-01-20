using FCG.Core.Integration;
using FCG.Payments.Domain.Entities;
using FCG.Payments.Domain.Entities.Enums;

namespace FCG.Payments.Application.Extensions;

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
