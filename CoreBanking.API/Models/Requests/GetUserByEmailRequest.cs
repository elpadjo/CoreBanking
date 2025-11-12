namespace CoreBanking.API.Models.Requests;

/// <summary>
/// Request model for retrieving a user by their email address
/// </summary>
public class GetUserByEmailRequest
{
    /// <summary>
    /// The email address to search for (comes from route parameter)
    /// </summary>
    public required string Email { get; set; }
}
