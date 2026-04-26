namespace Frock_backend.routes.Domain.Exceptions;

public class OsrmUnavailableException : Exception
{
    public OsrmUnavailableException(string message) : base(message) { }
    public OsrmUnavailableException(string message, Exception inner) : base(message, inner) { }
}
