using AutoMapper;
using CoreBanking.API.Models;
using CoreBanking.API.Models.Requests;
using CoreBanking.Application.Common.Models;
using CoreBanking.Application.Holds.Commands.CreateHold;
using CoreBanking.Application.Holds.Commands.DeleteHold;
using CoreBanking.Application.Holds.Commands.UpdateHold;
using CoreBanking.Application.Holds.Queries.GetHolds;
using CoreBanking.Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class HoldsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<HoldsController> _logger;

    public HoldsController(IMediator mediator, IMapper mapper, ILogger<HoldsController> logger)
    {
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all holds (paginated)
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>All holds between pagesize * page number</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<HoldDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllHolds(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
    {
        var query = new GetAllHoldsQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query);

        return Ok(ApiResponse<PaginatedResult<HoldDto>>.CreateSuccess(result.Data!));
    }

    /// <summary>
    /// Get Holds by account number
    /// </summary>
    /// <param name="accountNumber">The 10-digit account number</param>
    /// <returns>Hold details</returns>
    /// <response code="200">Returns the hold details</response>
    /// <response code="404">Account not found</response>
    /// <response code="400">Invalid account number format</response>
    [HttpGet("{accountNumber}")]
    [ProducesResponseType(typeof(ApiResponse<List<HoldDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHoldsByAccount(string accountNumber)
    {
        _logger.LogInformation("Retrieving account details for {AccountNumber}", accountNumber);

        var query = new GetHoldsByAccountQuery { AccountNumber = AccountNumber.Create(accountNumber) };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse<List<HoldDto>>.CreateSuccess(result.Data!));
    }

    /// <summary>
    /// Create a new hold on an account
    /// </summary>
    /// <param name="request">Hold creation details</param>
    /// <returns>The newly created Hold ID</returns>
    /// <response code="201">Hold created successfully</response>
    /// <response code="400">Invalid request data or business rule violation</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateHold([FromBody] CreateHoldRequest request)
    {
        _logger.LogInformation("Creating new hold on account {AccountId}", request.AccountId);

        var command = _mapper.Map<CreateHoldCommand>(request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse.CreateFailure(result.Errors));

        return Created(
            $"api/holds/{result.Data}",
            ApiResponse<Guid>.CreateSuccess(result.Data!));
    }

    //[HttpPut("{holdId:guid}")]
    //[ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    //public async Task<IActionResult> UpdateHold(Guid holdId, [FromBody] UpdateHoldRequest request)
    //{

    //    var command = new UpdateHoldCommand
    //    {
    //        HoldId = HoldId.Create(holdId), 
    //        Amount = request.Amount,
    //        Description = request.Description,
    //        DurationInDays = request.DurationInDays
    //    };

    //    _logger.LogInformation("Updating hold {HoldId}", holdId);

    //    var result = await _mediator.Send(command);

    //    if (!result.IsSuccess)
    //        return BadRequest(ApiResponse.CreateFailure(result.Errors));

    //    return Ok(ApiResponse<Guid>.CreateSuccess(result.Data));
    //}

    //[HttpDelete("{id:guid}")]
    //public async Task<IActionResult> DeleteHold(Guid id)
    //{
    //    var result = await _mediator.Send(new DeleteHoldCommand(id));
    //    return result.IsSuccess ? Ok(result) : BadRequest(result);
    //}


}
