public interface IAlertService
{
    Task<AlertDetails> GetAlertDetailsAsync(string alertId);
}