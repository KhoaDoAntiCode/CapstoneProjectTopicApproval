namespace CapstoneRegistration.API.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(404, message) { }

    public NotFoundException(string entityName, object key)
        : base(404, $"{entityName} with id '{key}' was not found.") { }
}
