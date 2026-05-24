namespace DentalClinic.SharedKernel.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message, string errorCode = "BUSINESS_RULE_VIOLATION")
        : base(message, errorCode)
    {
    }

    public BusinessRuleException(string message, string errorCode, Exception innerException)
        : base(message, errorCode, innerException)
    {
    }
}
