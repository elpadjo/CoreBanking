namespace CoreBanking.API.Models.Requests;

public record DeleteUserRequest
{
    public string DeletedBy { get; init; } = string.Empty;
}
