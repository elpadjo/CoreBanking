namespace CoreBanking.Infrastructure.Extensions
{
    public static class TypeInfoExtensions
    {
        public static bool IsSubclassOfRawGeneric(this Type type, Type generic)
        {
            while (type != null && type != typeof(object))
            {
                var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (generic == cur)
                    return true;
                type = type.BaseType;
            }
            return false;
        }
    }
}
