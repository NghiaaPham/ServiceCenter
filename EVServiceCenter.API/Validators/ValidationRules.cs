namespace EVServiceCenter.API.Validators;

public static class ValidationRules
{
    public const string RuleSetCreate = "Create";
    public const string RuleSetUpdate = "Update";

    public static class Messages
    {
        public const string Required = "{PropertyName} is required.";
        public const string MustBePositive = "{PropertyName} must be greater than 0.";
        public const string MustBeNonNegative = "{PropertyName} must be greater than or equal to 0.";
        public const string MaxLength = "{PropertyName} must be at most {MaxLength} characters.";
        public const string InvalidEmail = "{PropertyName} is not a valid email address.";
        public const string InvalidRange = "{PropertyName} must be between {From} and {To}.";
        public const string NotInFuture = "{PropertyName} cannot be in the future.";
    }
}

