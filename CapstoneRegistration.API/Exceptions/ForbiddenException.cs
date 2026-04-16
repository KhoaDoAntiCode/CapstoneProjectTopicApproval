namespace CapstoneRegistration.API.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.") : base(403, message) { }
}
