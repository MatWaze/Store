namespace Yandex.Checkout.V3;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

/// <summary>
/// Признак предмета расчета
/// </summary>
[JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
public enum PaymentSubject
{
    /// <summary>
    /// Товар
    /// </summary>
    Commodity,

    /// <summary>
    /// Подакцизный товар
    /// </summary>
    Excise,

    /// <summary>
    /// Работа
    /// </summary>
    Job,

    /// <summary>
    /// Услуга
    /// </summary>
    Service,

    /// <summary>
    /// Ставка в азартной игре
    /// </summary>
    GamblingBet,

    /// <summary>
    /// Выигрыш в азартной игре
    /// </summary>
    GamblingPrize,

    /// <summary>
    /// Лотерейный билет
    /// </summary>
    Lottery,

    /// <summary>
    /// Выигрыш в лотерее
    /// </summary>
    LotteryPrize,

    /// <summary>
    /// Результаты интеллектуальной деятельности
    /// </summary>
    IntellectualActivity,

    /// <summary>
    /// Платеж
    /// </summary>
    Payment,

    /// <summary>
    /// Агентское вознаграждение
    /// </summary>
    AgentCommission,

    /// <summary>
    /// Несколько вариантов
    /// </summary>
    Composite,

    /// <summary>
    /// Другое
    /// </summary>
    Another
}
