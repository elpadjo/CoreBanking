namespace CoreBanking.Core.ValueObjects
{
    public record Address
    {
        public string Street { get; }
        public string City { get; }
        public string State { get; }
        public string ZipCode { get; }
        public string Country { get; }

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
