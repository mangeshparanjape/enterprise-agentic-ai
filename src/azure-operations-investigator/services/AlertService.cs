public sealed class MockAlertService : IAlertService
{
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