namespace CoreBanking.API.Models.Requests;

/// <summary>
/// Request model for activating a user account
/// </summary>
/// <remarks>
/// This is an empty request model since activation only requires the userId from the route.
/// It exists to maintain consistency with other endpoints and support future extensions.
/// </remarks>
public class ActivateUserRequest
{
    // Currently no properties needed - userId comes from route
    // This can be extended in the future if activation needs additional parameters
}
