namespace CoreBanking.Core.ValueObjects
{
    public record HoldId
    {
        public Guid Value { get; }

        private HoldId(Guid value)
        {
            Value = value;
        }

        public static HoldId Create()
        {
            return new HoldId(Guid.NewGuid());
        }

        public static HoldId Create(Guid value)
        {
            return new HoldId(value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}