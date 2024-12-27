using CallaghanDev.Finance.Enums;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace CallaghanDev.IG.Extensions
{
    public static class FxSymbolExtensions
    {
        public static string ToDisplayString(this FxSymbols symbol)
        {
            var memberInfo = typeof(FxSymbols).GetMember(symbol.ToString())[0];
            var enumMemberAttribute = memberInfo.GetCustomAttribute<EnumMemberAttribute>();
            return enumMemberAttribute?.Value ?? symbol.ToString();
        }

        public static string GetDescription(this FxSymbols symbol)
        {
            var memberInfo = typeof(FxSymbols).GetMember(symbol.ToString())[0];
            var descriptionAttribute = memberInfo.GetCustomAttribute<DescriptionAttribute>();
            return descriptionAttribute?.Description ?? symbol.ToString();
        }

        public static FxPairType GetPairType(this FxSymbols symbol)
        {
            var memberInfo = typeof(FxSymbols).GetMember(symbol.ToString())[0];
            var pairTypeAttribute = memberInfo.GetCustomAttribute<CurrencyPairTypeAttribute>();
            return pairTypeAttribute?.Type ?? FxPairType.Major;
        }
    }
}
