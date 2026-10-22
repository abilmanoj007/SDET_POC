using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDET_POC.API.Common.Messaging;
using SDET_POC.API.Services;

namespace SDET_POC.API.Features.CW1.Commands;

public sealed class CreateShipmentBookingController(IRequestHandler<CreateShipmentBookingCommand, string> handler) : ControllerBase
{
    [HttpPost]
    [Route("api/create-booking")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> PostAsync(CancellationToken cancellationToken)
    {
        var result= await handler.Handle(new CreateShipmentBookingCommand(), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateShipmentBookingCommand() : IRequest<string>;

public sealed class CreateShipmentBookingCommandHandler : IRequestHandler<CreateShipmentBookingCommand, string>
{
    private readonly ICW1Client _cw1Client;
    public CreateShipmentBookingCommandHandler(ICW1Client cw1Client)
    {
        _cw1Client = cw1Client; 
    }
    public async Task<string> Handle(CreateShipmentBookingCommand command, CancellationToken cancellationToken)
    {
        return await _cw1Client.CreateShipmentBookingInCW1Async(cancellationToken);
    }
}