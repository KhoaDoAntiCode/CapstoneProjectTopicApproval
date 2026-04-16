namespace CapstoneRegistration.API.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Unauthorized.") : base(401, message) { }
}
