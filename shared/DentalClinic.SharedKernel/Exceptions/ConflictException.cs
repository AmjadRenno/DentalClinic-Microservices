namespace DentalClinic.SharedKernel.Exceptions;

/// <summary>
/// Exception thrown when an operation conflicts with existing state
/// </summary>
public class ConflictException : DomainException
{
    public ConflictException(string message, string errorCode = "CONFLICT")
        : base(message, errorCode)
    {
    }

    public ConflictException(string entityName, object entityId, string reason)
        : base($"Conflict for {entityName} with id '{entityId}': {reason}", "CONFLICT")
    {
    }
}
