namespace CoreBanking.API.Models.Requests;

/// <summary>
/// Request model for retrieving a user by their username
/// </summary>
public class GetUserByUsernameRequest
{
    /// <summary>
    /// The username to search for (comes from route parameter)
    /// </summary>
    public required string Username { get; set; }
}
