namespace CoreBanking.Application.Customers.Commands.CreateCustomer
{
    public class CreateCustomerDto
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;


        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string Street { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string State { get; init; } = string.Empty;
        public string ZipCode { get; init; } = string.Empty;
        public string Country { get; init; } = "NG";


        public string BVN { get; init; }
        public int CreditScore { get; init; }
        public DateTime DateOfBirth { get; init; }
    }
}
