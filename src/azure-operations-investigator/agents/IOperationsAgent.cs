public interface IOperationsAgent
{
    Task<string> ChatAsync(string message);
}