namespace FCG.Core.Messages.Integration;

public class OrderPlacedEvent : IntegrationEvent
{
    public int ClientId { get; set; }
    public int OrderId { get; set; }
    public int PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string CardName { get; set; }
    public string CardNumber { get; set; }
    public string ExpirationDate { get; set; }
    public string Cvv { get; set; }

    public OrderPlacedEvent(int clientId, int orderId, int paymentMethod, decimal amount, string cardName,
    string cardNumber, string expirationDate, string cvv)
    {
        ClientId = clientId;
        OrderId = orderId;
        PaymentMethod = paymentMethod;
        Amount = amount;
        CardName = cardName;
        CardNumber = cardNumber;
        ExpirationDate = expirationDate;
        Cvv = cvv;
    }
}
