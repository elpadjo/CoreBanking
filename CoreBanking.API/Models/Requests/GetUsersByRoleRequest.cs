namespace CoreBanking.API.Models.Requests;

/// <summary>
/// Request model for retrieving users by their role with pagination
/// </summary>
public class GetUsersByRoleRequest
{
    /// <summary>
    /// The role to filter users by (comes from route parameter)
    /// </summary>
    public required string Role { get; set; }

    /// <summary>
    /// Page number for pagination (1-indexed)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of users per page
    /// </summary>
    public int PageSize { get; set; } = 10;
}
