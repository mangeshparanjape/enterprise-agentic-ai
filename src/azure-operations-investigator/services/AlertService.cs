namespace EnterpriseAiPortfolio.Services;
public sealed class MockAlertService : IAlertService
{
    public Task<IReadOnlyList<AlertSummary>> GetActiveAlertsAsync()
    {
        return Task.FromResult<IReadOnlyList<AlertSummary>>(
            new List<AlertSummary>
            {
                new AlertSummary("A123", "Sev1", "apim-payments-usgovaz-001"),
                new AlertSummary("A124", "Sev2", "apim-payments-usgovaz-002"),
                new AlertSummary("A125", "Sev3", "apim-payments-usgovaz-003")
            });
    }

    public Task<AlertDetails> GetAlertDetailsAsync(string alertId)
    {
        return Task.FromResult(
        new AlertDetails(
          alertId,
          "Sev1",
          "apim-payments-usgovaz-001",
          "USGov Arizona",
          "APIM capacity exceeded 80%",
          new[]
          {
              "Check APIM capacity metric",
              "Review APIMGatewayLogs",
              "Confirm autoscale rule fired"
          },
          new AlertSafety(
              "ReadOnly",
              false)));
    }
}