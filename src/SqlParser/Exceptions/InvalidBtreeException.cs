namespace SqlParser.Exceptions;

public class InvalidBtreeException : Exception
{
    public InvalidBtreeException(string? message) : base(message)
    {
    }
}