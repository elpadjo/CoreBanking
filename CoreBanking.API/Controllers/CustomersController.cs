using AutoMapper;
using CoreBanking.API.Models;
using CoreBanking.Application.Customers.Commands.CreateCustomer;
using CoreBanking.Application.Customers.Commands.DeactivateCustomer;
using CoreBanking.Application.Customers.Commands.ReactivateCustomer;
using CoreBanking.Application.Customers.Commands.UpdateCreditScore;
using CoreBanking.Application.Customers.Commands.UpdateProfile;
using CoreBanking.Application.Customers.Queries.GetCustomerDetails;
using CoreBanking.Application.Customers.Queries.GetCustomers;
using CoreBanking.Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoreBanking.API.Controllers;

/// <summary>
/// Controller for managing customers, including creation, retrieval, activation, deactivation, 
/// credit score updates, and profile updates.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomersController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomersController"/> class.
    /// </summary>
    public CustomersController(IMediator mediator, IMapper mapper, ILogger<CustomersController> logger)
    {
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a list of all customers.
    /// </summary>
    /// <returns>A list of customers.</returns>
    /// <response code="200">Returns the list of customers.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<CustomerDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetCustomersQuery { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _mediator.Send(query);

        return Ok(result
            //ApiResponse<List<CustomerDto>>.CreateSuccess(result.Data!)
            );
    }

    /// <summary>
    /// Retrieves details of a specific customer by their unique ID.
    /// </summary>
    /// <param name="customerId">The GUID of the customer.</param>
    /// <returns>Customer details including profile and account information.</returns>
    /// <response code="200">Returns the customer details.</response>
    /// <response code="404">Customer not found.</response>
    [HttpGet("{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerDetailsDto>>> GetCustomer([FromRoute] Guid customerId)
    {
        var query = new GetCustomerDetailsQuery { CustomerId = CustomerId.Create(customerId) };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse<CustomerDetailsDto>.CreateSuccess(result.Data!));
    }

    /// <summary>
    /// Creates a new customer in the system.
    /// </summary>
    /// <param name="request">The customer creation request containing personal and contact details.</param>
    /// <returns>The ID of the newly created customer.</returns>
    /// <response code="201">Customer successfully created.</response>
    /// <response code="400">Invalid request data.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateCustomer([FromBody] CreateCustomerDto request)
    {
        var command = _mapper.Map<CreateCustomerCommand>(request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse.CreateFailure(result.Errors));

        return CreatedAtAction(
            nameof(GetCustomer),
            new { customerId = result.Data },
            ApiResponse<CustomerId>.CreateSuccess(result.Data!));
    }

    /// <summary>
    /// Reactivates a previously deactivated customer.
    /// </summary>
    /// <param name="customerId">The GUID of the customer.</param>
    /// <param name="reason">The reason for reactivation. Defaults to "Customer request".</param>
    /// <returns>Status of the reactivation operation.</returns>
    /// <response code="200">Customer successfully reactivated.</response>
    /// <response code="400">Reactivation failed.</response>
    [HttpPost("{customerId:guid}/reactivate")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> ReactivateCustomer(
        [FromRoute] Guid customerId,
        [FromQuery] string reason = "Customer request")
    {
        var command = new ReactivateCustomerCommand
        {
            CustomerId = CustomerId.Create(customerId),
            Reason = reason
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse.CreateSuccess("Customer reactivated successfully."));
    }

    /// <summary>
    /// Deactivates an active customer.
    /// </summary>
    /// <param name="customerId">The GUID of the customer.</param>
    /// <param name="reason">The reason for deactivation. Defaults to "Customer request".</param>
    /// <returns>Status of the deactivation operation.</returns>
    /// <response code="200">Customer successfully deactivated.</response>
    /// <response code="400">Deactivation failed.</response>
    [HttpPost("{customerId:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> DeactivateCustomer(
        [FromRoute] Guid customerId,
        [FromQuery] string reason = "Customer request")
    {
        var command = new DeactivateCustomerCommand
        {
            CustomerId = CustomerId.Create(customerId),
            Reason = reason
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse.CreateSuccess("Customer deactivated successfully."));
    }

    /// <summary>
    /// Updates the credit score of a customer.
    /// </summary>
    /// <param name="customerId">The GUID of the customer.</param>
    /// <param name="request">The credit score update request.</param>
    /// <returns>Status of the update operation.</returns>
    /// <response code="200">Credit score updated successfully.</response>
    /// <response code="400">Update failed due to invalid input.</response>
    [HttpPatch("{customerId:guid}/credit-score")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> UpdateCreditScore(
        [FromRoute] Guid customerId,
        [FromBody] UpdateCreditScoreDto request)
    {
        var command = new UpdateCreditScoreCommand
        {
            CustomerId = CustomerId.Create(customerId),
            NewCreditScore = request.NewCreditScore,
            Reason = request.Reason ?? "System update"
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse.CreateSuccess("Customer credit score updated successfully."));
    }

    /// <summary>
    /// Updates the profile information of a customer.
    /// </summary>
    /// <param name="customerId">The GUID of the customer.</param>
    /// <param name="request">The profile update request containing new contact and address information.</param>
    /// <returns>Status of the update operation.</returns>
    /// <response code="200">Profile updated successfully.</response>
    /// <response code="400">Update failed due to invalid input.</response>
    [HttpPatch("{customerId:guid}/profile")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> UpdateProfile(
        [FromRoute] Guid customerId,
        [FromBody] UpdateProfileDto request)
    {
        var command = new UpdateProfileCommand
        {
            CustomerId = CustomerId.Create(customerId),
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Street = request.Street,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            Country = request.Country
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse.CreateSuccess("Customer profile updated successfully."));
    }
}