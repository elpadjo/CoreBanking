namespace CoreBanking.Core.ValueObjects
{
    public record Address
    {
        public string Street { get; init; }
        public string City { get; init; }
        public string State { get; init; }
        public string ZipCode { get; init; }
        public string Country { get; init; }

        // EF Core needs this
        private Address() { }

        public Address(string street, string city, string state, string zipCode, string country = "US")
        {
            Street = street ?? throw new ArgumentNullException(nameof(street));
            City = city ?? throw new ArgumentNullException(nameof(city));
            State = state ?? throw new ArgumentNullException(nameof(state));
            ZipCode = zipCode ?? throw new ArgumentNullException(nameof(zipCode));
            Country = country;
        }

        public override string ToString() => $"{Street}, {City}, {State} {ZipCode}, {Country}";
    }
}