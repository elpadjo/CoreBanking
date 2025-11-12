namespace CoreBanking.API.Models.Requests;

public record UpdateRoleRequest
{
    public string NewRole { get; init; } = string.Empty;
    public string ChangedBy { get; init; } = string.Empty;
}
