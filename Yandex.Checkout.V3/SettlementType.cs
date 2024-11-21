namespace Yandex.Checkout.V3;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

[JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
public enum SettlementType
{
    /// <summary>
    /// Безналичный расчет
    /// </summary>
    Cashless,

    /// <summary>
    /// Предоплата (аванс)
    /// </summary>
    Prepayment,

    /// <summary>
    /// Постоплата (кредит)
    /// </summary>
    Postpayment,

    /// <summary>
    /// Встречное предоставление
    /// </summary>
    Consideration,

    /// <summary>
    /// Выплата
    /// </summary>
    Payout
}
