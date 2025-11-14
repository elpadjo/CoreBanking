namespace CoreBanking.Core.ValueObjects
{
    public record UserId
    {
        public Guid Value { get; }

        private UserId(Guid value)
        {
            Value = value;
        }

        public static UserId Create()
        {
            return new UserId(Guid.NewGuid());
        }

        public static UserId Create(Guid value)
        {
            return new UserId(value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}