namespace Yandex.Checkout.V3;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

[JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
public enum ReceiptType
{
    /// <summary>
    /// Приход
    /// </summary>
    Payment,

    /// <summary>
    /// Возврат прихода
    /// </summary>
    Refund
}
