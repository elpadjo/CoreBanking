namespace CoreBanking.API.Models.Requests;

public record LoginUserRequest
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
