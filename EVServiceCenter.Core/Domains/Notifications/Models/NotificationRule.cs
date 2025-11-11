using System.Text.Json.Serialization;

namespace EVServiceCenter.Core.Domains.Notifications.Models;

/// <summary>
/// JSON-based notification rule model for flexible condition evaluation
/// Scalability: Supports complex nested conditions with AND/OR logic
/// Maintainability: JSON structure is easy to edit and version control
/// Performance: Compiled expression trees for fast evaluation (future enhancement)
/// </summary>
public class NotificationRuleCondition
{
    /// <summary>
    /// Logical operator for combining multiple conditions
    /// </summary>
    [JsonPropertyName("operator")]
    public RuleOperator Operator { get; set; } = RuleOperator.And;

    /// <summary>
    /// List of conditions to evaluate
    /// </summary>
    [JsonPropertyName("conditions")]
    public List<RuleConditionItem>? Conditions { get; set; }
}

/// <summary>
/// Individual condition item in a rule
/// </summary>
public class RuleConditionItem
{
    /// <summary>
    /// Field name to evaluate (e.g., "Status", "AppointmentDate", "Priority")
    /// </summary>
    [JsonPropertyName("field")]
    public string Field { get; set; } = null!;

    /// <summary>
    /// Comparison operator
    /// </summary>
    [JsonPropertyName("operator")]
    public ComparisonOperator Operator { get; set; }

    /// <summary>
    /// Value to compare against
    /// </summary>
    [JsonPropertyName("value")]
    public object? Value { get; set; }

    /// <summary>
    /// For range queries: second value
    /// Example: Field=Price, Operator=Between, Value=100, ValueTo=500
    /// </summary>
    [JsonPropertyName("valueTo")]
    public object? ValueTo { get; set; }
}

/// <summary>
/// Logical operators for combining conditions
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RuleOperator
{
    /// <summary>
    /// All conditions must be true
    /// </summary>
    And,

    /// <summary>
    /// At least one condition must be true
    /// </summary>
    Or
}

/// <summary>
/// Comparison operators for condition evaluation
/// Performance: Optimized for database query translation
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ComparisonOperator
{
    /// <summary>
    /// Exact match: field == value
    /// </summary>
    Equals,

    /// <summary>
    /// Not equal: field != value
    /// </summary>
    NotEquals,

    /// <summary>
    /// Greater than: field > value
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Greater than or equal: field >= value
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Less than: field < value
    /// </summary>
    LessThan,

    /// <summary>
    /// Less than or equal: field <= value
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// String contains (case-insensitive): field.Contains(value)
    /// </summary>
    Contains,

    /// <summary>
    /// String starts with: field.StartsWith(value)
    /// </summary>
    StartsWith,

    /// <summary>
    /// String ends with: field.EndsWith(value)
    /// </summary>
    EndsWith,

    /// <summary>
    /// Value in list: value in [field]
    /// Example: Status IN ["Pending", "Confirmed"]
    /// </summary>
    In,

    /// <summary>
    /// Value not in list
    /// </summary>
    NotIn,

    /// <summary>
    /// Value between range: value >= field AND value <= valueTo
    /// </summary>
    Between,

    /// <summary>
    /// Field is null or empty
    /// </summary>
    IsNull,

    /// <summary>
    /// Field is not null or empty
    /// </summary>
    IsNotNull,

    /// <summary>
    /// Date/time relative comparison: Today, Yesterday, Tomorrow, LastWeek, etc.
    /// </summary>
    IsToday,
    IsYesterday,
    IsTomorrow,
    IsThisWeek,
    IsLastWeek,
    IsThisMonth,
    IsLastMonth
}

/// <summary>
/// Example JSON rule structures:
///
/// Simple rule:
/// {
///   "operator": "And",
///   "conditions": [
///     { "field": "Status", "operator": "Equals", "value": "Pending" },
///     { "field": "AppointmentDate", "operator": "IsToday" }
///   ]
/// }
///
/// Complex rule with OR logic:
/// {
///   "operator": "Or",
///   "conditions": [
///     { "field": "Priority", "operator": "In", "value": ["High", "Urgent"] },
///     { "field": "Status", "operator": "Equals", "value": "Overdue" }
///   ]
/// }
///
/// Range query:
/// {
///   "operator": "And",
///   "conditions": [
///     { "field": "TotalAmount", "operator": "Between", "value": 1000000, "valueTo": 5000000 },
///     { "field": "CreatedDate", "operator": "IsThisMonth" }
///   ]
/// }
/// </summary>
public static class RuleExamples
{
    // Documentation only - not used in runtime
}
