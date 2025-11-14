using AutoMapper;
using CoreBanking.API.Models;
using CoreBanking.API.Models.Requests;
using CoreBanking.Application.Accounts.Commands.CreateAccount;
using CoreBanking.Application.Accounts.Commands.TransferMoney;
using CoreBanking.Application.Accounts.DTOs;
using CoreBanking.Application.Accounts.Queries.GetAccountDetails;
using CoreBanking.Application.Accounts.Queries.GetTransactionHistory;
using CoreBanking.Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoreBanking.API.Controllers;

/// <summary>
/// Banking accounts management API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IMediator mediator, IMapper mapper, ILogger<AccountsController> logger)
    {
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get account details by account number
    /// </summary>
    /// <param name="accountNumber">The 10-digit account number</param>
    /// <returns>Account details including balance and customer information</returns>
    /// <response code="200">Returns the account details</response>
    /// <response code="404">Account not found</response>
    /// <response code="400">Invalid account number format</response>
    [HttpGet("{accountNumber}")]
    [ProducesResponseType(typeof(ApiResponse<AccountDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AccountDetailsDto>>> GetAccountDetails(string accountNumber)
    {
        _logger.LogInformation("Retrieving account details for {AccountNumber}", accountNumber);

        var query = new GetAccountDetailsQuery { AccountNumber = AccountNumber.Create(accountNumber) };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse<AccountDetailsDto>.CreateSuccess(result.Data!));
    }

    /// <summary>
    /// Create a new bank account
    /// </summary>
    /// <param name="request">Account creation details</param>
    /// <returns>The newly created account ID</returns>
    /// <response code="201">Account created successfully</response>
    /// <response code="400">Invalid request data or business rule violation</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateAccount([FromBody] CreateAccountRequest request)
    {
        _logger.LogInformation("Creating new account for customer {CustomerId}", request.CustomerId);

        var command = _mapper.Map<CreateAccountCommand>(request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse.CreateFailure(result.Errors));

        return CreatedAtAction(
            nameof(GetAccountDetails),
            new { accountNumber = result.Data.AccountNumber },
            ApiResponse<CreateAccountResponse>.CreateSuccess(result.Data!));
    }

    /// <summary>
    /// Transfer money between accounts
    /// </summary>
    /// <param name="sourceAccountNumber">Source account number</param>
    /// <param name="destinationAccountNumber">Destination account number</param>
    /// <param name="request">Transfer details</param>
    /// <returns>Transfer operation result</returns>
    /// <response code="200">Transfer completed successfully</response>
    /// <response code="400">Invalid transfer request</response>
    /// <response code="404">One or both accounts not found</response>
    /// <response code="409">Business rule violation (e.g., insufficient funds)</response>
    [HttpPost("{sourceAccountNumber}/transfer/{destinationAccountNumber}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse>> TransferMoney(
        string sourceAccountNumber,
        string destinationAccountNumber,
        [FromBody] TransferMoneyRequest request)
    {
        _logger.LogInformation("Processing transfer from {SourceAccount} to {DestinationAccount}",
            sourceAccountNumber, destinationAccountNumber);

        var command = new TransferMoneyCommand
        {
            SourceAccountNumber = AccountNumber.Create(sourceAccountNumber),
            DestinationAccountNumber = AccountNumber.Create(destinationAccountNumber),
            Amount = new Money(request.Amount, request.Currency),
            Reference = request.Reference,
            Description = request.Description
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return result.Errors.Any(e => e.Contains("insufficient", StringComparison.OrdinalIgnoreCase) ||
                                         e.Contains("limit reached", StringComparison.OrdinalIgnoreCase))
                ? Conflict(ApiResponse.CreateFailure(result.Errors))
                : BadRequest(ApiResponse.CreateFailure(result.Errors));
        }

        return Ok(ApiResponse.CreateSuccess("Transfer completed successfully"));
    }

    /// <summary>
    /// Get transaction history for an account
    /// </summary>
    /// <param name="accountNumber">The account number</param>
    /// <param name="startDate">Start date for filtering transactions (optional)</param>
    /// <param name="endDate">End date for filtering transactions (optional)</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of transactions per page (default: 50)</param>
    /// <returns>Paginated transaction history</returns>
    /// <response code="200">Returns transaction history</response>
    /// <response code="404">Account not found</response>
    /// <response code="400">Invalid account number format</response>
    [HttpGet("{accountNumber}/transactions")]
    [ProducesResponseType(typeof(ApiResponse<TransactionHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<TransactionHistoryDto>>> GetTransactionHistory(
        string accountNumber,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        _logger.LogInformation("Retrieving transaction history for {AccountNumber}", accountNumber);

        var query = new GetTransactionHistoryQuery
        {
            AccountNumber = AccountNumber.Create(accountNumber),
            StartDate = startDate,
            EndDate = endDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse<TransactionHistoryDto>.CreateSuccess(result.Data!));
    }
}