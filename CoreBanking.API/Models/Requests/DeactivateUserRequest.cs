namespace CoreBanking.API.Models.Requests;

public record DeactivateUserRequest
{
    public string Reason { get; init; } = "Administrative action";
}
