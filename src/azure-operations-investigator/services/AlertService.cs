public sealed class MockAlertService : IAlertService
{
    public Task<string> GetAlertDetailsAsync(string alertId)
    {
        return Task.FromResult("""
        {
          "alertId": "A123",
          "severity": "Sev1",
          "resource": "apim-payments-usgovaz-001",
          "region": "USGov Arizona",
          "signal": "APIM capacity exceeded 80%",
          "nextSteps": [
            "Check APIM capacity metric",
            "Review APIMGatewayLogs for 429 and 5xx",
            "Confirm autoscale rule fired"
          ],
          "safety": {
            "accessLevel": "ReadOnly",
            "destructiveAction": false
          }
        }
        """);
    }
}