
using ProfileService.Contracts.Messages;
using Rebus.Bus;
using Rebus.Handlers;

namespace ProfileService.Api.Handlers;

//For test. Fake logik der returnerer success lige meget hvad
public class PaymentCommandHandler :
    IHandleMessages<ChargeUserPayment>,
    IHandleMessages<RefundUserPayment>
{
    private readonly IBus _bus;

    public PaymentCommandHandler(IBus bus)
    {
        _bus = bus;
    }

    public async Task Handle(ChargeUserPayment message)
    {
        Console.WriteLine($"[Payment] Charging user {message.UserId} amount {message.Amount}...");

        
        var success = true;

        if (success)
        {
            await _bus.Send(new PaymentProcessedSuccessfully(message.UserId));
        }
        else
        {
            await _bus.Send(new PaymentFailed(message.UserId, "Payment error"));
        }
    }

    public async Task Handle(RefundUserPayment message)
    {
        Console.WriteLine($"[Payment] Refunding user {message.UserId}...");

        await Task.CompletedTask;
    }
}