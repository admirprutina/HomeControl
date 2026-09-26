using HomeControl.Api.Contracts.Devices;
using HomeControl.Application.Features.Devices.Commands.RegisterDevice;
using HomeControl.Application.Features.Devices.Commands.TurnOffLight;
using HomeControl.Application.Features.Devices.Commands.TurnOnLight;
using HomeControl.Application.Features.Devices.Queries.GetDeviceById;
using HomeControl.Application.Features.Devices.Queries.TestDeviceConnection;
using HomeControl.Application.Messaging;
using HomeControl.Domain.Devices;
using Microsoft.AspNetCore.Mvc;

namespace HomeControl.Api.Controllers;

[ApiController]
[Route("api/devices")]
public sealed class DevicesController : ControllerBase
{
    private readonly ISender _sender;

    public DevicesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetDeviceByIdResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetDeviceByIdQueryRequest(id);
        var result = await _sender.Send(query, cancellationToken);
        var response = new GetDeviceByIdResponse(result.DeviceId, result.Name, result.Type.ToString(), result.IsOn);

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<RegisterDeviceResponse>> RegisterAsync(
        [FromBody] RegisterDeviceRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<DeviceType>(request.Type, ignoreCase: true, out var deviceType)
            || !Enum.IsDefined(deviceType))
        {
            return BadRequest();
        }

        var command = new RegisterDeviceCommandRequest(request.Name, deviceType);
        var result = await _sender.Send(command, cancellationToken);
        var response = new RegisterDeviceResponse(result.DeviceId, result.Name, result.Type.ToString());

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("{id:guid}/turn-on")]
    public async Task<ActionResult<TurnOnLightResponse>> TurnOnAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new TurnOnLightCommandRequest(id);
        var result = await _sender.Send(command, cancellationToken);
        var response = new TurnOnLightResponse(result.DeviceId, result.IsOn);

        return Ok(response);
    }

    [HttpPost("{id:guid}/test-connection")]
    public async Task<ActionResult<TestDeviceConnectionResponse>> TestConnectionAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new TestDeviceConnectionQueryRequest(id);
        var result = await _sender.Send(query, cancellationToken);
        var response = new TestDeviceConnectionResponse(result.DeviceId, result.Protocol, result.IsSuccessful);

        return Ok(response);
    }

    [HttpPost("{id:guid}/turn-off")]
    public async Task<ActionResult<TurnOffLightResponse>> TurnOffAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new TurnOffLightCommandRequest(id);
        var result = await _sender.Send(command, cancellationToken);
        var response = new TurnOffLightResponse(result.DeviceId, result.IsOn);

        return Ok(response);
    }
}
