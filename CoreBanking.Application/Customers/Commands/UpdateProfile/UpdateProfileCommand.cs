using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.Customers.Commands.UpdateProfile
{
    public class UpdateProfileCommand : ICommand<CustomerId>
    {
        public CustomerId CustomerId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // Address fields
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = "NG";
    }
}
