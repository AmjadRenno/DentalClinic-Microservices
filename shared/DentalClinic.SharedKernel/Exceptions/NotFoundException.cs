namespace DentalClinic.SharedKernel.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found
/// </summary>
public class NotFoundException : DomainException
{
    public string EntityName { get; }
    public object EntityId { get; }

    public NotFoundException(string entityName, object entityId)
        : base($"{entityName} with id '{entityId}' was not found.", "ENTITY_NOT_FOUND")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    public NotFoundException(string message) 
        : base(message, "NOT_FOUND")
    {
        EntityName = string.Empty;
        EntityId = string.Empty;
    }
}
