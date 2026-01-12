namespace FCG.Core.Messages.Integration;

public class OrderPlacedEvent : IntegrationEvent
{
    public Guid ClientId { get; set; }
    public Guid OrderId { get; set; }
    public int PaymentType { get; set; }
    public decimal Amount { get; set; }
    public string CardName { get; set; }
    public string CardNumber { get; set; }
    public string ExpirationDate { get; set; }
    public string Cvv { get; set; }

    public OrderPlacedEvent(Guid clientId, Guid orderId, int paymentType, decimal amount, string cardName,
    string cardNumber, string expirationDate, string cvv)
    {
        ClientId = clientId;
        OrderId = orderId;
        PaymentType = paymentType;
        Amount = amount;
        CardName = cardName;
        CardNumber = cardNumber;
        ExpirationDate = expirationDate;
        Cvv = cvv;
    }
}
