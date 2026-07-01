public interface IAlertService
{
    Task<string> GetAlertDetailsAsync(string alertId);
}