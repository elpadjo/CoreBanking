namespace CoreBanking.API.Models.Requests;

/// <summary>
/// Request model for retrieving a user by their ID
/// </summary>
public class GetUserByIdRequest
{
    /// <summary>
    /// The user ID (comes from route parameter)
    /// </summary>
    public Guid UserId { get; set; }
}
