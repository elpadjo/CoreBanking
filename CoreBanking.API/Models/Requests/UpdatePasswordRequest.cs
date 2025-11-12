namespace CoreBanking.API.Models.Requests;

public record UpdatePasswordRequest
{
    public string NewPassword { get; init; } = string.Empty;
}
