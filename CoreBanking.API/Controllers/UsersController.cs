using AutoMapper;
using CoreBanking.API.Models;
using CoreBanking.API.Models.Requests;
using CoreBanking.Application.Common.Models;
using CoreBanking.Application.User.Commands.ActivateUser;
using CoreBanking.Application.User.Commands.CreateUser;
using CoreBanking.Application.User.Commands.DeactivateUser;
using CoreBanking.Application.User.Commands.DeleteUser;
using CoreBanking.Application.User.Commands.LoginUser;
using CoreBanking.Application.User.Commands.RestoreUser;
using CoreBanking.Application.User.Commands.UpdatePassword;
using CoreBanking.Application.User.Commands.UpdateRole;
using CoreBanking.Application.User.Commands.UpdateUsername;
using CoreBanking.Application.User.Queries.Common;
using CoreBanking.Application.User.Queries.GetUserByEmail;
using CoreBanking.Application.User.Queries.GetUserById;
using CoreBanking.Application.User.Queries.GetUserByUsername;
using CoreBanking.Application.User.Queries.GetUsersByRole;
using CoreBanking.Core.Models;
using CoreBanking.Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoreBanking.API.Controllers;

/// <summary>
/// User Management API - Provides comprehensive user administration endpoints
/// </summary>
/// <remarks>
/// This controller handles all user-related operations including:
/// - User creation and authentication
/// - Profile management (username, password, role updates)
/// - Account lifecycle (activation, deactivation, deletion)
/// - User queries and filtering
///
/// All endpoints return standardized ApiResponse objects with success/failure indicators.
/// Pagination is supported on list endpoints with configurable page size (max 100).
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;
    private readonly IMapper _mapper;

    public UsersController(IMediator mediator, ILogger<UsersController> logger, IMapper mapper)
    {
        _mediator = mediator;
        _logger = logger;
        _mapper = mapper;
    }

    #region Commands

    /// <summary>
    /// Creates a new user account in the system
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/users/create
    ///     {
    ///        "username": "john.doe",
    ///        "password": "SecureP@ssw0rd",
    ///        "email": "john.doe@example.com",
    ///        "role": "Teller"
    ///     }
    ///
    /// Valid roles: Admin, Manager, Teller
    ///
    /// Business Rules:
    /// - Username must be unique
    /// - Email must be unique and valid format
    /// - Password must meet security requirements
    /// - New users are created in active state by default
    /// </remarks>
    /// <param name="request">User creation details including username, password, email, and role</param>
    /// <returns>The newly created user's GUID identifier</returns>
    /// <response code="201">User created successfully - returns user ID</response>
    /// <response code="400">Validation failed (duplicate username/email, invalid role, weak password)</response>
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateUser([FromBody] CreateUserRequest request)
    {
        _logger.LogInformation("Creating new user with username {Username}", request.Username);

        var command = _mapper.Map<CreateUserCommand>(request);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse.CreateFailure(result.Errors));

        return CreatedAtAction(
            nameof(GetUserById),
            new { userId = result.Data!.Value },
            ApiResponse<Guid>.CreateSuccess(result.Data!.Value, "User created successfully"));

 
    }
    /// <summary>
    /// Authenticates a user and returns their profile information
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/users/login
    ///     {
    ///        "username": "john.doe",
    ///        "password": "SecureP@ssw0rd"
    ///     }
    ///
    /// Security Features:
    /// - Account lockout after 5 failed attempts (30-minute lockout)
    /// - Password hashing verification
    /// - Login timestamp tracking
    /// - Failed attempt counter reset on successful login
    ///
    /// Response includes: UserId, Username, Email, Role, IsActive status
    /// </remarks>
    /// <param name="request">Login credentials (username and password)</param>
    /// <returns>User profile information including role and active status</returns>
    /// <response code="200">Login successful - returns user profile</response>
    /// <response code="400">Invalid credentials, account locked, or user inactive</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LoginUserResponse>>> Login([FromBody] LoginUserRequest request)
    {
        _logger.LogInformation("Login attempt for username {Username}", request.Username);

        var command = _mapper.Map<LoginUserCommand>(request);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse<LoginUserResponse>.CreateSuccess(result.Data!, "Login successful"));
    }

    /// <summary>
    /// Updates the username for an existing user account
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/users/{userId}/username
    ///     {
    ///        "newUsername": "johnny.doe"
    ///     }
    ///
    /// Business Rules:
    /// - New username must be unique across all users
    /// - Username cannot be empty or whitespace
    /// - User must exist and not be deleted
    ///
    /// Use Cases:
    /// - User requests username change
    /// - Administrator corrects typo in username
    /// - Rebranding or name change requirements
    /// </remarks>
    /// <param name="userId">The GUID identifier of the user to update</param>
    /// <param name="request">Username update request containing the new username</param>
    /// <returns>Success confirmation or error details</returns>
    /// <response code="200">Username updated successfully</response>
    /// <response code="400">Validation failed (username already taken, invalid format, or empty)</response>
    /// <response code="404">User not found or has been deleted</response>
    [HttpPut("{userId}/username")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateUsername(Guid userId, [FromBody] UpdateUsernameRequest request)
    {
        _logger.LogInformation("Updating username for user {UserId}", userId);

        var command = _mapper.Map<UpdateUsernameCommand>(request);
        command  = command with { UserId = UserId.Create(userId) };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase))
                ? NotFound(ApiResponse.CreateFailure(result.Errors))
                : BadRequest(ApiResponse.CreateFailure(result.Errors));
        }

        return Ok(ApiResponse.CreateSuccess("Username updated successfully"));
    }

    /// <summary>
    /// Updates the password for an existing user account
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/users/{userId}/password
    ///     {
    ///        "currentPassword": "OldP@ssw0rd123",
    ///        "newPassword": "NewSecureP@ss456"
    ///     }
    ///
    /// Security Requirements:
    /// - Current password must be provided and match the stored hash
    /// - New password must meet complexity requirements:
    ///   * Minimum length (typically 8+ characters)
    ///   * Mix of uppercase, lowercase, numbers, special characters
    /// - Cannot reuse the current password as new password
    ///
    /// Business Rules:
    /// - User must be active (not deleted)
    /// - Password change is logged for audit purposes
    /// - Failed attempts do not trigger account lockout
    ///
    /// Use Cases:
    /// - User-initiated password change for security
    /// - Periodic password rotation policy compliance
    /// - Recovery after suspected credential compromise
    /// </remarks>
    /// <param name="userId">The GUID identifier of the user whose password will be updated</param>
    /// <param name="request">Password update request with current and new password</param>
    /// <returns>Success confirmation or error details</returns>
    /// <response code="200">Password updated successfully</response>
    /// <response code="400">Validation failed (incorrect current password, weak new password, or passwords match)</response>
    /// <response code="404">User not found or has been deleted</response>
    [HttpPut("{userId}/password")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdatePassword(Guid userId, [FromBody] UpdatePasswordRequest request)
    {
        _logger.LogInformation("Updating password for user {UserId}", userId);

        var command = _mapper.Map<UpdatePasswordCommand>(request);

        command = command with
        {
            UserId = UserId.Create(userId)
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase))
                ? NotFound(ApiResponse.CreateFailure(result.Errors))
                : BadRequest(ApiResponse.CreateFailure(result.Errors));
        }

        return Ok(ApiResponse.CreateSuccess("Password updated successfully"));
    }

    /// <summary>
    /// Updates the role/permission level for an existing user
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/users/{userId}/role
    ///     {
    ///        "newRole": "Manager"
    ///     }
    ///
    /// Valid Roles:
    /// - **Admin**: Full system access, user management, configuration
    /// - **Manager**: Supervisory functions, reporting, approval workflows
    /// - **Teller**: Basic operations, customer transactions, limited access
    ///
    /// Business Rules:
    /// - Only administrators should be able to change user roles
    /// - Role must be one of the predefined valid roles
    /// - Role changes are audited for compliance
    /// - User permissions take effect immediately
    ///
    /// Use Cases:
    /// - Employee promotion or demotion
    /// - Temporary elevated privileges
    /// - Role correction after misconfiguration
    /// </remarks>
    /// <param name="userId">The GUID identifier of the user whose role will be updated</param>
    /// <param name="request">Role update request containing the new role designation</param>
    /// <returns>Success confirmation or error details</returns>
    /// <response code="200">Role updated successfully</response>
    /// <response code="400">Validation failed (invalid role name or user already has this role)</response>
    /// <response code="404">User not found or has been deleted</response>
    [HttpPut("{userId}/role")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> UpdateRole(Guid userId, [FromBody] UpdateRoleRequest request)
    {
        _logger.LogInformation("Updating role for user {UserId} to {NewRole}", userId, request.NewRole);

        var command = _mapper.Map<UpdateRoleCommand>(request);

        command = command with
        {
            UserId = UserId.Create(userId)
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase))
                ? NotFound(ApiResponse.CreateFailure(result.Errors))
                : BadRequest(ApiResponse.CreateFailure(result.Errors));
        }

        return Ok(ApiResponse.CreateSuccess("Role updated successfully"));
    }

    /// <summary>
    /// Activates a previously deactivated user account, enabling login and system access
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/users/{userId}/activate
    ///
    /// This endpoint enables a user account that was previously deactivated. Once activated:
    /// - User can log in to the system
    /// - User can perform role-appropriate operations
    /// - Failed login attempt counter is reset
    /// - Account lockout status is cleared
    ///
    /// Business Rules:
    /// - User must exist and not be deleted
    /// - User must currently be in inactive state
    /// - Cannot activate an already active account (idempotency)
    /// - Activation is logged for audit trail
    ///
    /// Use Cases:
    /// - Reactivating account after temporary suspension
    /// - Enabling account after administrative review
    /// - Restoring access after resolved security concern
    /// - Onboarding returning employees
    /// </remarks>
    /// <param name="userId">The GUID identifier of the user account to activate</param>
    /// <returns>Success confirmation or error details</returns>
    /// <response code="200">User activated successfully - account is now enabled</response>
    /// <response code="400">User is already active or cannot be activated</response>
    /// <response code="404">User not found or has been deleted</response>
    [HttpPost("{userId}/activate")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> ActivateUser(Guid userId)
    {
        _logger.LogInformation("Activating user {UserId}", userId);

        var command = _mapper.Map<ActivateUserCommand>(new ActivateUserRequest());
        command = command with
        {
            UserId = UserId.Create(userId)
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase))
                ? NotFound(ApiResponse.CreateFailure(result.Errors))
                : BadRequest(ApiResponse.CreateFailure(result.Errors));
        }

        return Ok(ApiResponse.CreateSuccess("User activated successfully"));
    }

    /// <summary>
    /// Deactivates an active user account, preventing login while preserving account data
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/users/{userId}/deactivate
    ///     {
    ///        "reason": "Employee on extended leave"
    ///     }
    ///
    /// Deactivation vs Deletion:
    /// - **Deactivate**: Prevents login but preserves all data; reversible via activation
    /// - **Delete**: Soft delete that marks user as deleted; requires restoration
    ///
    /// Effects of Deactivation:
    /// - User cannot log in or access the system
    /// - Active sessions may be terminated (depends on implementation)
    /// - Historical data and audit trail remain intact
    /// - Account can be reactivated without data loss
    ///
    /// Business Rules:
    /// - User must exist and not be deleted
    /// - User must currently be active
    /// - Reason for deactivation is recommended for audit purposes
    /// - Cannot deactivate an already inactive account
    ///
    /// Use Cases:
    /// - Employee suspension or leave of absence
    /// - Security incident investigation
    /// - Temporary access restriction
    /// - Policy violation pending review
    /// </remarks>
    /// <param name="userId">The GUID identifier of the user account to deactivate</param>
    /// <param name="request">Deactivation request with optional reason</param>
    /// <returns>Success confirmation or error details</returns>
    /// <response code="200">User deactivated successfully - account is now disabled</response>
    /// <response code="400">User is already inactive or cannot be deactivated</response>
    /// <response code="404">User not found or has been deleted</response>
    [HttpPost("{userId}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeactivateUser(Guid userId, [FromBody] DeactivateUserRequest request)
    {
        _logger.LogInformation("Deactivating user {UserId}", userId);

        var command = _mapper.Map<DeactivateUserCommand>(request);

        command = command with
        {
            UserId = UserId.Create(userId)
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase))
                ? NotFound(ApiResponse.CreateFailure(result.Errors))
                : BadRequest(ApiResponse.CreateFailure(result.Errors));
        }

        return Ok(ApiResponse.CreateSuccess("User deactivated successfully"));
    }

    /// <summary>
    /// Performs a soft delete on a user account, marking it as deleted while retaining data
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/users/{userId}
    ///     {
    ///        "reason": "User requested account closure",
    ///        "deletedBy": "admin@example.com"
    ///     }
    ///
    /// Soft Delete vs Hard Delete:
    /// - **Soft Delete** (this endpoint): Marks record as deleted; data remains in database
    /// - **Hard Delete**: Permanently removes data from database (not implemented)
    ///
    /// Effects of Soft Deletion:
    /// - User is marked as deleted with timestamp
    /// - User cannot log in or be retrieved by normal queries
    /// - Historical transactions and audit trail remain intact
    /// - Account can potentially be restored (if endpoint is enabled)
    /// - Preserves referential integrity with related records
    ///
    /// Business Rules:
    /// - User must exist (not already deleted)
    /// - Deletion reason is strongly recommended for compliance
    /// - Deleted users do not appear in active user lists
    /// - Username and email become available for new registrations (optional policy)
    ///
    /// Data Retention:
    /// - Soft deleted records typically retained for audit/legal requirements
    /// - Consider data retention policies (GDPR, CCPA, etc.)
    /// - May require periodic purge of old soft-deleted records
    ///
    /// Use Cases:
    /// - Employee termination
    /// - User-requested account deletion
    /// - Compliance with data removal requests
    /// - Cleanup of inactive or fraudulent accounts
    ///
    /// Note: This endpoint uses HTTP DELETE with a request body, which is supported but uncommon.
    /// </remarks>
    /// <param name="userId">The GUID identifier of the user account to delete</param>
    /// <param name="request">Deletion request with reason and metadata</param>
    /// <returns>Success confirmation or error details</returns>
    /// <response code="200">User deleted successfully - account is now marked as deleted</response>
    /// <response code="400">User is already deleted or deletion is not permitted</response>
    /// <response code="404">User not found in the system</response>
    [HttpDelete("{userId}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteUser(Guid userId, [FromBody] DeleteUserRequest request)
    {
        _logger.LogInformation("Soft deleting user {UserId}", userId);

       var command = _mapper.Map<DeleteUserCommand>(request);
        command = command with
        {
            UserId = UserId.Create(userId)
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase))
                ? NotFound(ApiResponse.CreateFailure(result.Errors))
                : BadRequest(ApiResponse.CreateFailure(result.Errors));
        }

        return Ok(ApiResponse.CreateSuccess("User deleted successfully"));
    }

    /// <summary>
    /// Restore soft deleted user account
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Operation result</returns>
    /// <response code="200">User restored successfully</response>
    /// <response code="400">User is not deleted</response>
    /// <response code="404">User not found</response>
    //[HttpPost("{userId}/restore")]
    //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    //public async Task<ActionResult<ApiResponse>> RestoreUser(Guid userId)
    //{
    //    _logger.LogInformation("Restoring soft deleted user {UserId}", userId);

    //    var command = _mapper.Map<RestoreUserCommand>(new { UserId = userId });

    //    command = command with
    //    {
    //        UserId = UserId.Create(userId)
    //    };

    //    var result = await _mediator.Send(command);

    //    if (!result.IsSuccess)
    //    {
    //        return result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase))
    //            ? NotFound(ApiResponse.CreateFailure(result.Errors))
    //            : BadRequest(ApiResponse.CreateFailure(result.Errors));
    //    }

    //    return Ok(ApiResponse.CreateSuccess("User restored successfully. Note: User must be manually activated."));
    //}

    #endregion

    #region Queries

    /// <summary>
    /// Retrieves detailed information for a specific user by their unique identifier
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/users/7d5f6ea2-9b1c-4f9f-9e7e-1e2a3b4c5d6e
    ///
    /// Returns comprehensive user profile including:
    /// - User identification (ID, username, email)
    /// - Role and permissions level
    /// - Account status (active/inactive, deleted flag)
    /// - Timestamps (created, last modified, last login)
    /// - Security info (failed login attempts, lockout status)
    ///
    /// Use Cases:
    /// - Displaying user profile page
    /// - Verifying user details before operations
    /// - Administrative user management
    /// - Audit and compliance reporting
    ///
    /// Performance:
    /// - Single database query by primary key (fast)
    /// - Consider caching for frequently accessed users
    /// </remarks>
    /// <param name="userId">The GUID identifier of the user to retrieve</param>
    /// <returns>Complete user profile data or error details</returns>
    /// <response code="200">Returns full user details including profile, role, and status</response>
    /// <response code="404">User not found or has been deleted</response>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(ApiResponse<UserDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDetailsDto>>> GetUserById(Guid userId)
    {
        _logger.LogInformation("Retrieving user details for {UserId}", userId);

        var request = new GetUserByIdRequest { UserId = userId };
        var query = _mapper.Map<GetUserByIdQuery>(request);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse<UserDetailsDto>.CreateSuccess(result.Data!));
    }

    /// <summary>
    /// Retrieves detailed information for a user by their username
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/users/username/john.doe
    ///
    /// Returns the same comprehensive user profile as GetUserById, but searches by username.
    ///
    /// Use Cases:
    /// - User lookup by memorable identifier (username)
    /// - Displaying user info when only username is known
    /// - Pre-login user verification
    /// - Search functionality in admin interfaces
    ///
    /// Performance Considerations:
    /// - Username should be indexed for fast lookups
    /// - Case sensitivity depends on database collation
    ///
    /// Note: Username must be URL-encoded if it contains special characters.
    /// </remarks>
    /// <param name="username">The unique username to search for</param>
    /// <returns>Complete user profile data or error details</returns>
    /// <response code="200">Returns full user details including profile, role, and status</response>
    /// <response code="404">User with specified username not found or has been deleted</response>
    [HttpGet("username/{username}")]
    [ProducesResponseType(typeof(ApiResponse<UserDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDetailsDto>>> GetUserByUsername(string username)
    {
        _logger.LogInformation("Retrieving user details for username {Username}", username);

        var request = new GetUserByUsernameRequest { Username = username };
        var query = _mapper.Map<GetUserByUsernameQuery>(request);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse<UserDetailsDto>.CreateSuccess(result.Data!));
    }

    /// <summary>
    /// Retrieves detailed information for a user by their email address
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/users/email/john.doe@example.com
    ///
    /// Returns the same comprehensive user profile as GetUserById, but searches by email.
    ///
    /// Use Cases:
    /// - User lookup by email for password reset flows
    /// - Contact-based user search
    /// - Email verification workflows
    /// - Duplicate email detection
    ///
    /// Performance Considerations:
    /// - Email should be indexed for fast lookups
    /// - Email comparison is typically case-insensitive
    ///
    /// Important: Email must be URL-encoded (@ becomes %40, etc.)
    /// Example: john.doe@example.com → john.doe%40example.com
    /// </remarks>
    /// <param name="email">The email address to search for (must be URL-encoded)</param>
    /// <returns>Complete user profile data or error details</returns>
    /// <response code="200">Returns full user details including profile, role, and status</response>
    /// <response code="404">User with specified email not found or has been deleted</response>
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(ApiResponse<UserDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDetailsDto>>> GetUserByEmail(string email)
    {
        _logger.LogInformation("Retrieving user details for email {Email}", email);

        var request = new GetUserByEmailRequest { Email = email };
        var query = _mapper.Map<GetUserByEmailQuery>(request);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse<UserDetailsDto>.CreateSuccess(result.Data!));
    }

    ///// <summary>
    ///// Get all users
    ///// </summary>
    ///// <returns>List of all users</returns>
    ///// <response code="200">Returns list of users</response>
    //[HttpGet]
    //[ProducesResponseType(typeof(ApiResponse<List<UserDto>>), StatusCodes.Status200OK)]
    //public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAllUsers()
    //{
    //    _logger.LogInformation("Retrieving all users");

    //    var query = _mapper.Map<GetAllUsersQuery>(new { });
    //    var result = await _mediator.Send(query);

    //    if (!result.IsSuccess)
    //        return BadRequest(ApiResponse.CreateFailure(result.Errors));

    //    return Ok(ApiResponse<List<UserDto>>.CreateSuccess(result.Data!));
    //}

    ///// <summary>
    ///// Get all active users
    ///// </summary>
    ///// <returns>List of active users</returns>
    ///// <response code="200">Returns list of active users</response>
    //[HttpGet("active")]
    //[ProducesResponseType(typeof(ApiResponse<List<UserDto>>), StatusCodes.Status200OK)]
    //public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetActiveUsers()
    //{
    //    _logger.LogInformation("Retrieving all active users");

    //    var query = _mapper.Map<GetActiveUsersQuery>(new { });

    //    var result = await _mediator.Send(query);

    //    if (!result.IsSuccess)
    //        return BadRequest(ApiResponse.CreateFailure(result.Errors));

    //    return Ok(ApiResponse<List<UserDto>>.CreateSuccess(result.Data!));
    //}

    /// <summary>
    /// Retrieves a paginated list of users filtered by their assigned role
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/users/role/Teller?pageNumber=1&amp;pageSize=20
    ///
    /// Valid Roles:
    /// - **Admin**: System administrators with full access
    /// - **Manager**: Supervisors with elevated permissions
    /// - **Teller**: Front-line staff with basic operations access
    ///
    /// Response Structure (PaginatedResult):
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": {
    ///     "items": [
    ///       {
    ///         "userId": "guid",
    ///         "username": "string",
    ///         "email": "string",
    ///         "role": "string",
    ///         "isActive": true,
    ///         "createdAt": "datetime"
    ///       }
    ///     ],
    ///     "totalCount": 45,
    ///     "pageNumber": 1,
    ///     "pageSize": 20,
    ///     "totalPages": 3
    ///   }
    /// }
    /// ```
    ///
    /// Pagination:
    /// - Default page size: 10
    /// - Maximum page size: 100 (enforced server-side)
    /// - Page numbers are 1-indexed
    /// - Empty results return empty items array, not 404
    ///
    /// Use Cases:
    /// - Listing all users with specific role for management
    /// - Role-based reporting and analytics
    /// - Bulk operations on users by role
    /// - Organizational hierarchy views
    ///
    /// Performance:
    /// - Results are filtered by role and active status
    /// - Consider caching for frequently requested roles
    /// - Use appropriate page size to balance performance and UX
    /// </remarks>
    /// <param name="role">The role to filter users by (Admin, Manager, or Teller)</param>
    /// <param name="pageNumber">The page number to retrieve (1-indexed, default: 1)</param>
    /// <param name="pageSize">Number of users per page (default: 10, maximum: 100)</param>
    /// <returns>Paginated list of users with the specified role</returns>
    /// <response code="200">Returns paginated list of users with metadata (total count, page info)</response>
    /// <response code="400">Invalid role name or invalid pagination parameters (page size exceeds max)</response>
    [HttpGet("role/{role}")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaginatedResult<UserDto>>>> GetUsersByRole(
        string role,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Retrieving users with role {Role} (Page {PageNumber}, Size {PageSize})", role, pageNumber, pageSize);

        var request = new GetUsersByRoleRequest 
        { 
            Role = role, 
            PageNumber = pageNumber, 
            PageSize = pageSize 
        };
        var query = _mapper.Map<GetAllUserQueryRole>(request);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse.CreateFailure(result.Errors));

        return Ok(ApiResponse<PaginatedResult<UserDto>>.CreateSuccess(result.Data!));
    }

    #endregion
}
