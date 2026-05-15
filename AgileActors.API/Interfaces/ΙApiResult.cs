namespace AgileActors.API.Interfaces
{
    public interface IApiResult
    {
        string Source { get; }
        bool IsSuccess { get; }
        string? ErrorMessage { get; }
    }
}