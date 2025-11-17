using AutoMapper;
using CoreBanking.Application.Accounts.Commands.CloseMyAccount;
using CoreBanking.Application.Accounts.Commands.CreateAccount;
using CoreBanking.Application.Accounts.Commands.UpdateAccountPreferences;
using CoreBanking.Application.Accounts.DTOs;
using CoreBanking.Application.Accounts.Queries.GetAccountBalance;
using CoreBanking.Application.Accounts.Queries.GetAccountDetails;
using CoreBanking.Core.Models;
using CoreBanking.Core.Models.Requests;
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
    /// Get account balance information
    /// </summary>
    /// <param name="accountNumber">The account number</param>
    /// <returns>Current and available balances</returns>
    /// <response code="200">Returns account balance</response>
    /// <response code="404">Account not found or access denied</response>
    [HttpGet("{accountNumber}/balance")]
    [ProducesResponseType(typeof(ApiResponse<AccountBalanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AccountBalanceDto>>> GetAccountBalance(string accountNumber)
    {
        //var customerId = GetCurrentCustomerId();
        //_logger.LogInformation("Customer {CustomerId} checking balance for account {AccountNumber}",
        _logger.LogInformation("Customer checking balance for account {AccountNumber}", accountNumber);

        var query = new GetAccountBalanceQuery
        {
            AccountNumber = AccountNumber.Create(accountNumber)
            //CustomerId = customerId // For authorization
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse<AccountBalanceDto>.CreateSuccess(result.Data!));
    }

    /// <summary>
    /// Close customer's own account (requires zero balance)
    /// </summary>
    /// <param name="accountNumber">The account number to close</param>
    /// <param name="request">Close account request with reason</param>
    /// <returns>Operation result</returns>
    /// <response code="200">Account closed successfully</response>
    /// <response code="400">Cannot close account (e.g., non-zero balance)</response>
    /// <response code="404">Account not found or access denied</response>
    [HttpDelete("{accountNumber}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> CloseMyAccount(
        string accountNumber,
        [FromBody] CloseMyAccountRequest request)
    {
        //var customerId = GetCurrentCustomerId();
        //_logger.LogInformation("Customer {CustomerId} requesting to close account {AccountNumber}",
        _logger.LogInformation("Customer requesting to close account {AccountNumber}", accountNumber);

        var command = new CloseMyAccountCommand
        {
            AccountNumber = AccountNumber.Create(accountNumber),
            //CustomerId = customerId,
            Reason = request.Reason
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse.CreateSuccess("Account closed successfully"));
    }

    /// <summary>
    /// Update account preferences and settings
    /// </summary>
    /// <param name="accountNumber">The account number</param>
    /// <param name="request">Account preferences to update</param>
    /// <returns>Operation result</returns>
    /// <response code="200">Preferences updated successfully</response>
    /// <response code="404">Account not found or access denied</response>
    [HttpPatch("{accountNumber}/preferences")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateAccountPreferences(
        string accountNumber,
        [FromBody] UpdateAccountPreferencesRequest request)
    {
        // var customerId = GetCurrentCustomerId();
        // _logger.LogInformation("Customer {CustomerId} updating preferences for account {AccountNumber}",
        _logger.LogInformation("Customer updating preferences for account {AccountNumber}", accountNumber);

        var command = new UpdateAccountPreferencesCommand
        {
            AccountNumber = AccountNumber.Create(accountNumber),
            //CustomerId = customerId,
            EnableTransactionAlerts = request.EnableTransactionAlerts,
            EnableLowBalanceAlerts = request.EnableLowBalanceAlerts,
            LowBalanceThreshold = request.LowBalanceThresholdAmount.HasValue
        ? new Money(request.LowBalanceThresholdAmount.Value, request.LowBalanceThresholdCurrency ?? "NGN")
        : null,
            MonthlyStatementPreference = request.MonthlyStatementPreference // Direct assignment
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return NotFound(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse.CreateSuccess("Account preferences updated successfully"));
    }

    
}