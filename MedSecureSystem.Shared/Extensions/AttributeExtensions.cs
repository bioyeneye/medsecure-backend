using System.Reflection;

namespace MedSecureSystem.Shared.Extensions
{
    public static class AttributeExtensions
    {
        public static TValue GetAttributeValue<TAttribute, TValue>(
            this MemberInfo memberInfo,
            Func<TAttribute, TValue> valueSelector,
#pragma warning disable CS8601 // Possible null reference assignment.
            TValue defaultValue = default)
#pragma warning restore CS8601 // Possible null reference assignment.
            where TAttribute : Attribute
        {
            var attribute = memberInfo.GetCustomAttribute<TAttribute>(false);
            return attribute != null ? valueSelector(attribute) : defaultValue;
        }
    }
}
