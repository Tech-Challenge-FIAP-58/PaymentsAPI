using System;

namespace FCG.Core.Messages.Integration
{
    public class OrderPayedIntegrationEvent : IntegrationEvent
    {
        public Guid ClienteId { get; private set; }
        public Guid PedidoId { get; private set; }

        public OrderPayedIntegrationEvent(Guid clienteId, Guid pedidoId)
        {
            ClienteId = clienteId;
            PedidoId = pedidoId;
        }
    }
}