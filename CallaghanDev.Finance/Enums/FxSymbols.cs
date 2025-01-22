
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace CallaghanDev.Finance.Enums
{
    public class CurrencyPairTypeAttribute : Attribute
    {
        public FxPairType Type { get; }

        public CurrencyPairTypeAttribute(FxPairType type)
        {
            Type = type;
        }
    }

    public enum FxSymbols
    {
        [Description("US Dollar/New Zealand Dollar")]
        [EnumMember(Value = "USD/NZD")]
        USDNZD,

        [Description("British Pound/US Dollar")]
        [EnumMember(Value = "GBP/USD")]
        GBPUSD,

        [Description("US Dollar/British Pound")]
        [EnumMember(Value = "USD/GBP")]
        [CurrencyPairTypeAttribute(FxPairType.Major)]
        USDGBP,

        [Description("US Dollar/Japanese Yen")]
        [EnumMember(Value = "USD/JPY")]
        USDJPY,

        [Description("Japanese Yen/US Dollar")]
        [EnumMember(Value = "JPY/USD")]
        JPYUSD,

        [Description("Euro/US Dollar")]
        [EnumMember(Value = "EUR/USD")]
        EURUSD,

        [Description("US Dollar/Euro")]
        [EnumMember(Value = "USD/EUR")]
        USDEUR,

        [Description("Euro/Japanese Yen")]
        [EnumMember(Value = "EUR/JPY")]
        EURJPY,

        [Description("Japanese Yen/Euro")]
        [EnumMember(Value = "JPY/EUR")]
        JPYEUR,

        [Description("Australian Dollar/US Dollar")]
        [EnumMember(Value = "AUD/USD")]
        AUDUSD,

        [Description("US Dollar/Australian Dollar")]
        [EnumMember(Value = "USD/AUD")]
        USDAUD,

        [Description("Euro/British Pound")]
        [EnumMember(Value = "EUR/GBP")]
        EURGBP,

        [Description("US Dollar/Canadian Dollar")]
        [EnumMember(Value = "USD/CAD")]
        USDCAD,

        [Description("Canadian Dollar/US Dollar")]
        [EnumMember(Value = "CAD/USD")]
        CADUSD,

        [Description("US Dollar/Swiss Franc")]
        [EnumMember(Value = "USD/CHF")]
        USDCHF,

        [Description("Swiss Franc/US Dollar")]
        [EnumMember(Value = "CHF/USD")]
        CHFUSD,

        [Description("Euro/Swiss Franc")]
        [EnumMember(Value = "EUR/CHF")]
        EURCHF,

        [Description("Swiss Franc/Euro")]
        [EnumMember(Value = "CHF/EUR")]
        CHFEUR,

        [Description("British Pound/Euro")]
        [EnumMember(Value = "GBP/EUR")]
        GBPEUR,

        [Description("British Pound/Japanese Yen")]
        [EnumMember(Value = "GBP/JPY")]
        GBPJPY,

        [Description("Japanese Yen/British Pound")]
        [EnumMember(Value = "JPY/GBP")]
        JPYGBP,

        [Description("Swiss Franc/Japanese Yen")]
        [EnumMember(Value = "CHF/JPY")]
        CHFJPY,

        [Description("Japanese Yen/Swiss Franc")]
        [EnumMember(Value = "JPY/CHF")]
        JPYCHF,

        [Description("British Pound/Swiss Franc")]
        [EnumMember(Value = "GBP/CHF")]
        GBPCHF,

        [Description("British Pound/Canadian Dollar")]
        [EnumMember(Value = "GBP/CAD")]
        GBPCAD,

        [Description("Canadian Dollar/Japanese Yen")]
        [EnumMember(Value = "CAD/JPY")]
        CADJPY,

        [Description("Euro/Canadian Dollar")]
        [EnumMember(Value = "EUR/CAD")]
        EURCAD,

        [Description("Canadian Dollar/Swiss Franc")]
        [EnumMember(Value = "CAD/CHF")]
        CADCHF,

        [Description("US Dollar/South African Rand")]
        [EnumMember(Value = "USD/ZAR")]
        USDZAR,

        [Description("US Dollar/Singapore Dollar")]
        [EnumMember(Value = "USD/SGD")]
        USDSGD,

        [Description("South African Rand/Japanese Yen")]
        [EnumMember(Value = "ZAR/JPY")]
        ZARJPY,

        [Description("British Pound/South African Rand")]
        [EnumMember(Value = "GBP/ZAR")]
        GBPZAR,

        [Description("Euro/Singapore Dollar")]
        [EnumMember(Value = "EUR/SGD")]
        EURSGD,

        [Description("British Pound/Singapore Dollar")]
        [EnumMember(Value = "GBP/SGD")]
        GBPSGD,

        [Description("Euro/South African Rand")]
        [EnumMember(Value = "EUR/ZAR")]
        EURZAR,

        [Description("Indian Rupee/Japanese Yen")]
        [EnumMember(Value = "INR/JPY")]
        INRJPY,

        [Description("Singapore Dollar/Japanese Yen")]
        [EnumMember(Value = "SGD/JPY")]
        SGDJPY,

        [Description("US Dollar/Hong Kong Dollar")]
        [EnumMember(Value = "USD/HKD")]
        USDHKD,

        [Description("New Zealand Dollar/US Dollar")]
        [EnumMember(Value = "NZD/USD")]
        NZDUSD,

        [Description("Australian Dollar/Japanese Yen")]
        [EnumMember(Value = "AUD/JPY")]
        AUDJPY,

        [Description("British Pound/Australian Dollar")]
        [EnumMember(Value = "GBP/AUD")]
        GBPAUD,

        [Description("Australian Dollar/New Zealand Dollar")]
        [EnumMember(Value = "AUD/NZD")]
        AUDNZD,

        [Description("Australian Dollar/Canadian Dollar")]
        [EnumMember(Value = "AUD/CAD")]
        AUDCAD,

        [Description("Australian Dollar/Swiss Franc")]
        [EnumMember(Value = "AUD/CHF")]
        AUDCHF,

        [Description("Euro/Australian Dollar")]
        [EnumMember(Value = "EUR/AUD")]
        EURAUD,

        [Description("New Zealand Dollar/Japanese Yen")]
        [EnumMember(Value = "NZD/JPY")]
        NZDJPY,

        [Description("British Pound/New Zealand Dollar")]
        [EnumMember(Value = "GBP/NZD")]
        GBPNZD,

        [Description("Euro/New Zealand Dollar")]
        [EnumMember(Value = "EUR/NZD")]
        EURNZD,

        [Description("New Zealand Dollar/Swiss Franc")]
        [EnumMember(Value = "NZD/CHF")]
        NZDCHF,

        [Description("New Zealand Dollar/Canadian Dollar")]
        [EnumMember(Value = "NZD/CAD")]
        NZDCAD,

        [Description("Australian Dollar/Singapore Dollar")]
        [EnumMember(Value = "AUD/SGD")]
        AUDSGD,

        [Description("Australian Dollar/Euro")]
        [EnumMember(Value = "AUD/EUR")]
        AUDEUR,

        [Description("Australian Dollar/British Pound")]
        [EnumMember(Value = "AUD/GBP")]
        AUDGBP,

        [Description("New Zealand Dollar/Australian Dollar")]
        [EnumMember(Value = "NZD/AUD")]
        NZDAUD,

        [Description("New Zealand Dollar/Euro")]
        [EnumMember(Value = "NZD/EUR")]
        NZDEUR,

        [Description("New Zealand Dollar/British Pound")]
        [EnumMember(Value = "NZD/GBP")]
        NZDGBP,

        [Description("US Dollar/Norwegian Krone")]
        [EnumMember(Value = "USD/NOK")]
        USDNOK,

        [Description("British Pound/Norwegian Krone")]
        [EnumMember(Value = "GBP/NOK")]
        GBPNOK,

        [Description("Euro/Norwegian Krone")]
        [EnumMember(Value = "EUR/NOK")]
        EURNOK,

        [Description("Euro/Swedish Krona")]
        [EnumMember(Value = "EUR/SEK")]
        EURSEK,

        [Description("British Pound/Swedish Krona")]
        [EnumMember(Value = "GBP/SEK")]
        GBPSEK,

        [Description("US Dollar/Swedish Krona")]
        [EnumMember(Value = "USD/SEK")]
        USDSEK,

        [Description("Swiss Franc/Norwegian Krone")]
        [EnumMember(Value = "CHF/NOK")]
        CHFNOK,

        [Description("Canadian Dollar/Norwegian Krone")]
        [EnumMember(Value = "CAD/NOK")]
        CADNOK,

        [Description("Euro/Danish Krone")]
        [EnumMember(Value = "EUR/DKK")]
        EURDKK,

        [Description("British Pound/Danish Krone")]
        [EnumMember(Value = "GBP/DKK")]
        GBPDKK,

        [Description("Norwegian Krone/Japanese Yen")]
        [EnumMember(Value = "NOK/JPY")]
        NOKJPY,

        [Description("Norwegian Krone/Swedish Krona")]
        [EnumMember(Value = "NOK/SEK")]
        NOKSEK,

        [Description("Swedish Krona/Japanese Yen")]
        [EnumMember(Value = "SEK/JPY")]
        SEKJPY,

        [Description("US Dollar/Danish Krone")]
        [EnumMember(Value = "USD/DKK")]
        USDDKK,

        [Description("Mexican Peso/Japanese Yen")]
        [EnumMember(Value = "MXN/JPY")]
        MXNJPY,

        [Description("US Dollar/Turkish Lira")]
        [EnumMember(Value = "USD/TRY")]
        USDTRY,

        [Description("Turkish Lira/Japanese Yen")]
        [EnumMember(Value = "TRY/JPY")]
        TRYJPY,

        [Description("US Dollar/Mexican Peso")]
        [EnumMember(Value = "USD/MXN")]
        USDMXN,

        [Description("Euro/Turkish Lira")]
        [EnumMember(Value = "EUR/TRY")]
        EURTRY,

        [Description("British Pound/Israeli Shekel")]
        [EnumMember(Value = "GBP/ILS")]
        GBPILS,

        [Description("Euro/Hungarian Forint")]
        [EnumMember(Value = "EUR/HUF")]
        EURHUF,

        [Description("Swiss Franc/Hungarian Forint")]
        [EnumMember(Value = "CHF/HUF")]
        CHFHUF,

        [Description("British Pound/Hungarian Forint")]
        [EnumMember(Value = "GBP/HUF")]
        GBPHUF,

        [Description("Euro/Mexican Peso")]
        [EnumMember(Value = "EUR/MXN")]
        EURMXN,

        [Description("Euro/Czech Koruna")]
        [EnumMember(Value = "EUR/CZK")]
        EURCZK,

        [Description("Euro/Israeli Shekel")]
        [EnumMember(Value = "EUR/ILS")]
        EURILS,

        [Description("Euro/Polish Zloty")]
        [EnumMember(Value = "EUR/PLN")]
        EURPLN,

        [Description("British Pound/Czech Koruna")]
        [EnumMember(Value = "GBP/CZK")]
        GBPCZK,

        [Description("British Pound/Mexican Peso")]
        [EnumMember(Value = "GBP/MXN")]
        GBPMXN,

        [Description("British Pound/Polish Zloty")]
        [EnumMember(Value = "GBP/PLN")]
        GBPPLN,

        [Description("British Pound/Turkish Lira")]
        [EnumMember(Value = "GBP/TRY")]
        GBPTRY,

        [Description("Polish Zloty/Japanese Yen")]
        [EnumMember(Value = "PLN/JPY")]
        PLNJPY,

        [Description("US Dollar/Czech Koruna")]
        [EnumMember(Value = "USD/CZK")]
        USDCZK,

        [Description("US Dollar/Hungarian Forint")]
        [EnumMember(Value = "USD/HUF")]
        USDHUF,

        [Description("US Dollar/Israeli Shekel")]
        [EnumMember(Value = "USD/ILS")]
        USDILS,

        [Description("US Dollar/Polish Zloty")]
        [EnumMember(Value = "USD/PLN")]
        USDPLN,

        [Description("US Dollar/Brazilian Real")]
        [EnumMember(Value = "USD/BRL")]
        USDBRL,

        [Description("US Dollar/Chinese Yuan Offshore")]
        [EnumMember(Value = "USD/CNH")]
        USDCNH,

        [Description("US Dollar/South Korean Won")]
        [EnumMember(Value = "USD/KRW")]
        USDKRW,

        [Description("Brazilian Real/Japanese Yen")]
        [EnumMember(Value = "BRL/JPY")]
        BRLJPY,

        [Description("British Pound/Indian Rupee")]
        [EnumMember(Value = "GBP/INR")]
        GBPINR,

        [Description("Chinese Yuan Offshore/Japanese Yen")]
        [EnumMember(Value = "CNH/JPY")]
        CNHJPY,

        [Description("Australian Dollar/Chinese Yuan Offshore")]
        [EnumMember(Value = "AUD/CNH")]
        AUDCNH,

        [Description("Canadian Dollar/Chinese Yuan Offshore")]
        [EnumMember(Value = "CAD/CNH")]
        CADCNH,

        [Description("Euro/Chinese Yuan Offshore")]
        [EnumMember(Value = "EUR/CNH")]
        EURCNH,

        [Description("British Pound/Chinese Yuan Offshore")]
        [EnumMember(Value = "GBP/CNH")]
        GBPCNH,

        [Description("New Zealand Dollar/Chinese Yuan Offshore")]
        [EnumMember(Value = "NZD/CNH")]
        NZDCNH,

        [Description("US Dollar/Indonesian Rupiah")]
        [EnumMember(Value = "USD/IDR")]
        USDIDR,

        [Description("US Dollar/Indian Rupee")]
        [EnumMember(Value = "USD/INR")]
        USDINR,

        [Description("US Dollar/Philippine Peso")]
        [EnumMember(Value = "USD/PHP")]
        USDPHP,

        [Description("US Dollar/Thai Baht")]
        [EnumMember(Value = "USD/THB")]
        USDTHB,

        [Description("US Dollar/Taiwan Dollar")]
        [EnumMember(Value = "USD/TWD")]
        USDTWD,

        [Description("Australian Dollar/Hong Kong Dollar")]
        [EnumMember(Value = "AUD/HKD")]
        AUDHKD,

        [Description("Australian Dollar/Indian Rupee")]
        [EnumMember(Value = "AUD/INR")]
        AUDINR,

        [Description("Australian Dollar/South African Rand")]
        [EnumMember(Value = "AUD/ZAR")]
        AUDZAR,

        [Description("Canadian Dollar/Euro")]
        [EnumMember(Value = "CAD/EUR")]
        CADEUR,

        [Description("Canadian Dollar/British Pound")]
        [EnumMember(Value = "CAD/GBP")]
        CADGBP,

        [Description("Canadian Dollar/Hong Kong Dollar")]
        [EnumMember(Value = "CAD/HKD")]
        CADHKD,

        [Description("Canadian Dollar/Singapore Dollar")]
        [EnumMember(Value = "CAD/SGD")]
        CADSGD,

        [Description("Swiss Franc/Australian Dollar")]
        [EnumMember(Value = "CHF/AUD")]
        CHFAUD,

        [Description("Swiss Franc/Canadian Dollar")]
        [EnumMember(Value = "CHF/CAD")]
        CHFCAD,

        [Description("Swiss Franc/Singapore Dollar")]
        [EnumMember(Value = "CHF/SGD")]
        CHFSGD,

        [Description("Swiss Franc/Hong Kong Dollar")]
        [EnumMember(Value = "CHF/HKD")]
        CHFHKD,

        [Description("Chinese Yuan Offshore/Hong Kong Dollar")]
        [EnumMember(Value = "CNH/HKD")]
        CNHHKD,

        [Description("Chinese Yuan Offshore/Singapore Dollar")]
        [EnumMember(Value = "CNH/SGD")]
        CNHSGD,

        [Description("Danish Krone/Japanese Yen")]
        [EnumMember(Value = "DKK/JPY")]
        DKKJPY,

        [Description("Danish Krone/Norwegian Krone")]
        [EnumMember(Value = "DKK/NOK")]
        DKKNOK,

        [Description("Danish Krone/Swedish Krona")]
        [EnumMember(Value = "DKK/SEK")]
        DKKSEK,

        [Description("Hong Kong Dollar/Japanese Yen")]
        [EnumMember(Value = "HKD/JPY")]
        HKDJPY,

        [Description("Indian Rupee/Singapore Dollar")]
        [EnumMember(Value = "INR/SGD")]
        INRSGD,

        [Description("Norwegian Krone/Hong Kong Dollar")]
        [EnumMember(Value = "NOK/HKD")]
        NOKHKD,

        [Description("Norwegian Krone/Singapore Dollar")]
        [EnumMember(Value = "NOK/SGD")]
        NOKSGD,

        [Description("New Zealand Dollar/Hong Kong Dollar")]
        [EnumMember(Value = "NZD/HKD")]
        NZDHKD,

        [Description("New Zealand Dollar/Singapore Dollar")]
        [EnumMember(Value = "NZD/SGD")]
        NZDSGD,

        [Description("New Zealand Dollar/South African Rand")]
        [EnumMember(Value = "NZD/ZAR")]
        NZDZAR,

        [Description("Swedish Krona/Hong Kong Dollar")]
        [EnumMember(Value = "SEK/HKD")]
        SEKHKD,

        [Description("Swedish Krona/Norwegian Krone")]
        [EnumMember(Value = "SEK/NOK")]
        SEKNOK,

        [Description("Swedish Krona/Singapore Dollar")]
        [EnumMember(Value = "SEK/SGD")]
        SEKSGD,

        [Description("Singapore Dollar/Hong Kong Dollar")]
        [EnumMember(Value = "SGD/HKD")]
        SGDHKD,

        [Description("South African Rand/Singapore Dollar")]
        [EnumMember(Value = "ZAR/SGD")]
        ZARSGD,

        [Description("South African Rand/Hong Kong Dollar")]
        [EnumMember(Value = "ZAR/HKD")]
        ZARHKD,

        [Description("Swiss Franc/Swedish Krona")]
        [EnumMember(Value = "CHF/SEK")]
        CHFSEK,

        [Description("Swiss Franc/Danish Krone")]
        [EnumMember(Value = "CHF/DKK")]
        CHFDKK,

        [Description("Swedish Krona/Danish Krone")]
        [EnumMember(Value = "SEK/DKK")]
        SEKDKK,

        [Description("Euro/Brazilian Real")]
        [EnumMember(Value = "EUR/BRL")]
        EURBRL,

        [Description("British Pound/Brazilian Real")]
        [EnumMember(Value = "GBP/BRL")]
        GBPBRL,

        [Description("Euro/South Korean Won")]
        [EnumMember(Value = "EUR/KRW")]
        EURKRW,

        [Description("British Pound/South Korean Won")]
        [EnumMember(Value = "GBP/KRW")]
        GBPKRW,

        [Description("Singapore Dollar/Chinese Yuan Offshore")]
        [EnumMember(Value = "SGD/CNH")]
        SGDCNH,

        [Description("Euro/Philippine Peso")]
        [EnumMember(Value = "EUR/PHP")]
        EURPHP,

        [Description("Euro/Thai Baht")]
        [EnumMember(Value = "EUR/THB")]
        EURTHB,

        [Description("Euro/Taiwan Dollar")]
        [EnumMember(Value = "EUR/TWD")]
        EURTWD,

        [Description("Euro/Indonesian Rupiah")]
        [EnumMember(Value = "EUR/IDR")]
        EURIDR,

        [Description("Brazilian Real/US Dollar")]
        [EnumMember(Value = "BRL/USD")]
        BRLUSD,

        [Description("Singapore Dollar/US Dollar")]
        [EnumMember(Value = "SGD/USD")]
        SGDUSD,

        [Description("Hong Kong Dollar/US Dollar")]
        [EnumMember(Value = "HKD/USD")]
        HKDUSD,


        [Description("Chinese Yuan Offshore/US Dollar")]
        [EnumMember(Value = "CNH/USD")]
        CNHUSD,

        [Description("South Korean Won/US Dollar")]
        [EnumMember(Value = "KRW/USD")]
        KRWUSD,

        [Description("Canadian Dollar/Polish Zloty")]
        [EnumMember(Value = "CAD/PLN")]
        CADPLN,

        [Description("Canadian Dollar/Czech Koruna")]
        [EnumMember(Value = "CAD/CZK")]
        CADCZK,

        [Description("Canadian Dollar/Hungarian Forint")]
        [EnumMember(Value = "CAD/HUF")]
        CADHUF,

        [Description("Swiss Franc/Polish Zloty")]
        [EnumMember(Value = "CHF/PLN")]
        CHFPLN,

        [Description("Swiss Franc/Czech Koruna")]
        [EnumMember(Value = "CHF/CZK")]
        CHFCZK,
    }
}
