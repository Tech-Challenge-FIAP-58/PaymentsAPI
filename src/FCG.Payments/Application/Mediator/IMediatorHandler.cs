using FCG.Payments.Domain.Entities.Mediatr;
using FluentValidation.Results;

namespace FCG.Payments.Application.Mediator
{
    public interface IMediatorHandler
    {
        Task PublishEvent<T> (T evento) where T : Event;
        Task<ValidationResult> SendCommand<T>(T comando) where T : Command;
    }
}
