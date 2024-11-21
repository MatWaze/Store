namespace Yandex.Checkout.V3;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

/// <summary>
/// Признак способа расчета
/// </summary>
[JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
public enum PaymentMode
{
    /// <summary>
    /// Полная предоплата
    /// </summary>
    FullPrepayment,

    /// <summary>
    /// Частичная предоплата
    /// </summary>
    PartialPrepayment,

    /// <summary>
    /// Аванс
    /// </summary>
    Advance,

    /// <summary>
    /// Полный расчет
    /// </summary>
    FullPayment,

    /// <summary>
    /// Частичный расчет и кредит
    /// </summary>
    PartialPayment,

    /// <summary>
    /// Кредит
    /// </summary>
    Credit,

    /// <summary>
    /// Выплата по кредиту
    /// </summary>
    CreditPayment
}
