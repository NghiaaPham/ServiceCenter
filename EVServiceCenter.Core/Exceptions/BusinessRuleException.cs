namespace EVServiceCenter.Core.Exceptions
{
    public class BusinessRuleException : Exception
    {
        public string ErrorCode { get; }

        public BusinessRuleException(string message, string errorCode = "BUSINESS_RULE_VIOLATION")
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}