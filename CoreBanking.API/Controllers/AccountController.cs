using AutoMapper;
using CoreBanking.API.Models;
using CoreBanking.API.Models.Requests;
using CoreBanking.Application.Accounts.Commands.CreateAccount;
using CoreBanking.Application.Accounts.Commands.CreateTransactions;
using CoreBanking.Application.Accounts.Commands.TransferMoney;
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
    /// Creates a new transaction for the specified account.
    /// </summary>
    /// <description>
    /// Records a new debit or credit transaction (e.g., deposit, withdrawal, or system adjustment)
    /// against a valid bank account. Returns the unique transaction ID upon success.
    /// </description>
    /// <remarks>
    /// This endpoint records a new debit or credit transaction on an account.  
    /// Use this to simulate deposits, withdrawals, or system-generated adjustments.
    ///
    /// **Supported Transaction Types:**
    /// - `Credit` — funds added to the account (e.g., deposit, interest payment)
    /// - `Debit` — funds removed from the account (e.g., withdrawal, service charge)
    /// - `Reversal` — system correction or refund of a previous transaction
    ///
    /// **Supported Currency:**
    /// - `NGN` — Nigerian Naira (only supported currency)
    ///
    /// **Example request:**
    /// ```json
    /// {
    ///   "accountNumber": "1234567890",
    ///   "amount": 5000.00,
    ///   "currency": "NGN",
    ///   "type": "Credit",
    ///   "description": "Initial deposit"
    /// }
    /// ```
    ///
    /// **Example successful response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Transaction created successfully",
    ///   "data": {
    ///     "value": "f1a9b2c5-2b7a-49d5-9a90-5fbe15ccdf3e"
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">
    /// Transaction creation details including account number, amount, currency (only NGN), and type (Credit, Debit, or Reversal).
    /// </param>
    /// <response code="201">Transaction successfully created and persisted.</response>
    /// <response code="400">Invalid input or business rule violation (e.g., negative amount, closed account).</response>
    [HttpPost("transaction")]
    [ProducesResponseType(typeof(ApiResponse<TransactionId>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<TransactionId>>> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        _logger.LogInformation("Creating new transaction for account {AccountNumber}", request.AccountNumber);

        var command = _mapper.Map<CreateTransactionCommand>(request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse<TransactionId>.CreateSuccess(result.Data!));
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
        _logger.LogInformation("Creating new account for customer {CustomerId}", CustomerId.Create(request.CustomerId) );

        var command = _mapper.Map<CreateAccountCommand>(request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse.CreateFailure(result.Errors));

        return CreatedAtAction(
            nameof(GetAccountDetails),
            new { accountNumber = "TEMPORARY" }, // Would need account number here
            ApiResponse<Guid>.CreateSuccess(result.Data!));
    }


    /// <summary>
    /// Transfer money between accounts
    /// </summary>
    /// <param name="sourceaccountNumber">Source account number</param>
    /// <param name="destinationaccountNumber">Destination account number</param>
    /// <param name="request">Transfer details</param>
    /// <returns>Transfer operation result</returns>
    /// <response code="200">Transfer completed successfully</response>
    /// <response code="400">Invalid transfer request</response>
    /// <response code="409">Business rule violation (e.g., insufficient funds)</response>
    [HttpPost("{accountNumber}/transfer")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse>> TransferMoney(
        string sourceaccountNumber, string destinationaccountNumber,
        [FromBody] TransferMoneyRequest request)
    {
        _logger.LogInformation("Processing transfer from {AccountNumber}", sourceaccountNumber);

        var command = new TransferMoneyCommand
        {
            SourceAccountNumber = AccountNumber.Create(sourceaccountNumber),
            DestinationAccountNumber = AccountNumber.Create(destinationaccountNumber),
            Amount = new Money(request.Amount, request.Currency),
            Reference = request.Reference,
            Description = request.Description
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return result.Errors.Any(e => e.Contains("insufficient", StringComparison.OrdinalIgnoreCase))
                ? Conflict(ApiResponse.CreateFailure(result.Errors))
                : BadRequest(ApiResponse.CreateFailure(result.Errors));
        }

        return Ok(ApiResponse.CreateSuccess("Transfer completed successfully"));
    }


    /// <summary>
    /// Retrieves the transaction history for a specific account.
    /// </summary> 
    /// <description>
    /// Returns a paginated list of transactions for the provided account number.
    /// Supports optional filtering by start and end dates.
    /// </description>
    /// <remarks>
    /// This endpoint returns a paginated list of all transactions performed on a given account.  
    /// You can optionally filter results by date range.
    ///
    /// **Example request:**
    /// ```http
    /// GET /api/accounts/1234567890/transactions?startDate=2025-01-01&endDate=2025-02-01&page=1&pageSize=20
    /// ```
    ///
    /// **Example response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": {
    ///     "transactions": [
    ///       {
    ///         "transactionId": "7bda492a-5c3f-4b7e-b0c9-d3ad8a3175b1",
    ///         "type": "Credit",
    ///         "amount": 5000.00,
    ///         "currency": "NGN",
    ///         "description": "Deposit",
    ///         "timestamp": "2025-02-10T13:22:45Z"
    ///       }
    ///     ],
    ///     "pagination": {
    ///       "currentPage": 1,
    ///       "pageSize": 20,
    ///       "totalPages": 3,
    ///       "totalRecords": 60
    ///     }
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="accountNumber">The 10-digit account number to retrieve transaction history for.</param>
    /// <param name="startDate">Optional start date to filter transactions.</param>
    /// <param name="endDate">Optional end date to filter transactions.</param>
    /// <param name="page">Pagination parameter for the current page (default = 1).</param>
    /// <param name="pageSize">Number of records per page (default = 50).</param>
    /// <response code="200">Transaction history retrieved successfully.</response>
    /// <response code="404">Account not found or no transactions in the given period.</response>
    [HttpGet("{accountNumber}/transactions")]
    [ProducesResponseType(typeof(ApiResponse<TransactionHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TransactionHistoryDto>>> GetTransactionHistory(
        string accountNumber,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
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


    /// <summary>
    /// Retrieves a single transaction detail by transaction ID.
    /// </summary>
    /// <remarks>
    /// <description>
    /// Returns the complete transaction details including amount, currency, timestamp, type,
    /// and description for the specified account number and transaction ID.
    /// </description>
    /// Use this endpoint to fetch detailed information about a specific transaction.  
    /// It’s typically used for auditing or viewing transaction details in customer dashboards.
    ///
    /// **Example request:**
    /// ```http
    /// GET /api/accounts/transactions/f1a9b2c5-2b7a-49d5-9a90-5fbe15ccdf3e
    /// ```
    ///
    /// **Example response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": {
    ///     "transactionId": "f1a9b2c5-2b7a-49d5-9a90-5fbe15ccdf3e",
    ///     "type": "Credit",
    ///     "amount": 5000.00,
    ///     "currency": "NGN",
    ///     "description": "Deposit",
    ///     "timestamp": "2025-02-10T13:22:45Z",
    ///     "status": "Completed"
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="transactionId">The unique identifier of the transaction.</param>
    /// <response code="200">Transaction retrieved successfully.</response>
    /// <response code="404">Transaction not found for the provided account number.</response>
    [HttpGet("transactions/{transactionId}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> GetSingleTransactionHistory(Guid transactionId)
    {
        var query = new GetSingleTransactionHistoryQuery
        {
            TransactionId = TransactionId.Create(transactionId)
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse<TransactionDto>.CreateSuccess(result.Data!));
    }

    
}