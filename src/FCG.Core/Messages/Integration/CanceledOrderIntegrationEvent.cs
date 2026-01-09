using System;

namespace FCG.Core.Messages.Integration
{
    public class CanceledOrderIntegrationEvent : IntegrationEvent
    {
        public Guid ClienteId { get; private set; }
        public Guid PedidoId { get; private set; }

        public CanceledOrderIntegrationEvent(Guid clienteId, Guid pedidoId)
        {
            ClienteId = clienteId;
            PedidoId = pedidoId;
        }
    }
}