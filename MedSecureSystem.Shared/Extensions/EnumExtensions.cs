using System.ComponentModel;

namespace MedSecureSystem.Shared.Extensions
{
    public static class EnumExtensions
    {
        public static List<string> GetEnumNames<TEnum>() where TEnum : struct, Enum
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(e => e.ToString()).ToList();
        }
        public static string GetEnumDescription(this Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
    }


}
