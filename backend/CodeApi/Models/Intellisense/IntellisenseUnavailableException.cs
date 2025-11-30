namespace CodeApi.Services;

public class IntellisenseUnavailableException : Exception
{
    public IntellisenseUnavailableException(string message) : base(message)
    {
    }
}
