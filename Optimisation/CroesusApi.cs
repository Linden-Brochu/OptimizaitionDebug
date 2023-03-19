using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Optimisation;

/**
 * Endpoints:
 * - GET /CroesusValidation
    @QueryParam TeamName
    @QueryParam Password
    @Returns { Message: string, TotalValue: double, Time: DateTime, SubmissionCount: int, Submission: { action: string, ticker: string, date: string }[] }
 * - POST /CroesusValidation
    @QueryParam TeamName
    @QueryParam Password
    @QueryParam FinalValidationMode ("ON" or "OFF")
    @JsonBody { action: string, ticker: string, date: string }[]
    @Returns { Message: string, TotalValue: double }
 */

public static class CroesusApi
{
    static readonly string BASE_URL = "https://api.csgames2023.sandbox.croesusfin.cloud";
    static readonly string TEAM_NAME = "aeron-ets";
    static readonly string PASSWORD = "atlantis-myth";

    static HttpClient client = new HttpClient();

    public static async Task<HttpResponseMessage> GetCroesusValidation()
    {
        var url = $"{BASE_URL}/CroesusValidation?TeamName={TEAM_NAME}&TeamPassword={PASSWORD}";
        return await client.GetAsync(url);
    }

    public static async Task<HttpResponseMessage> PostCroesusValidation(List<CroesusApiAction> actions, string finalValidationMode = "OFF")
    {
        var url = $"{BASE_URL}/CroesusValidation?TeamName={TEAM_NAME}&TeamPassword={PASSWORD}&FinalValidationMode={finalValidationMode}";
        var json = JsonSerializer.Serialize(actions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PostAsync(url, content);
    }
}