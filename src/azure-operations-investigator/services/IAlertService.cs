public interface IAlertService
{
    Task<AlertDetails> GetAlertDetailsAsync(string alertId);
    Task<IReadOnlyList<AlertSummary>> GetActiveAlertsAsync();
}