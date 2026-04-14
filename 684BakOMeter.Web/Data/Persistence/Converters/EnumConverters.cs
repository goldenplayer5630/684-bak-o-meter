using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace _684BakOMeter.Web.Data.Persistence.Converters
{
    internal static class EnumConverters
    {
        public static ValueConverter<TEnum, string> ToStringConverter<TEnum>() where TEnum : struct, Enum
        {
            return new ValueConverter<TEnum, string>(
                v => v.ToString(),
                v => Enum.Parse<TEnum>(v));
        }
    }
}
