namespace CoreBanking.API.Models.Requests;

public record UpdateUsernameRequest
{
    public string NewUsername { get; init; } = string.Empty;
}
