namespace CoreBanking.Core.ValueObjects
{
    public record TransferId
    {
        public Guid Value { get; }

        private TransferId(Guid value)
        {
            Value = value;
        }

        public static TransferId Create()
        {
            return new TransferId(Guid.NewGuid());
        }

        public static TransferId Create(Guid value)
        {
            return new TransferId(value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}