namespace LibraryInventoryApi.Api.Services;

public class LibraryException : Exception
{
    public LibraryException(string message) : base(message) { }
}

public class NotFoundException : LibraryException
{
    public NotFoundException(string message) : base(message) { }
}

public class BusinessRuleException : LibraryException
{
    public BusinessRuleException(string message) : base(message) { }
}
