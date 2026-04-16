namespace CapstoneRegistration.API.Exceptions;

public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(400, message) { }
}
