using System.Net;
using System.Net.Http.Json;
using AgriSure.BuildingBlocks.Identity;

namespace AgriSure.Claims.Api.Infrastructure;

public sealed class PolicyClient(HttpClient httpClient)
{
    public async Task<PolicyEligibility?> GetClaimEligibilityAsync(
        Guid policyId,
        ActorContext actor,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/policies/{policyId}/claim-eligibility");
        request.Headers.Add(ActorContext.TenantHeader, actor.TenantId);
        request.Headers.Add(ActorContext.ActorIdHeader, actor.ActorId);
        request.Headers.Add(ActorContext.ActorNameHeader, actor.ActorName);
        request.Headers.Add(ActorContext.RoleHeader, actor.Role);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PolicyEligibility>(cancellationToken);
    }
}

public sealed record PolicyEligibility(
    Guid PolicyId,
    string PolicyNumber,
    Guid ProducerId,
    string ProducerName,
    string Crop,
    string County,
    bool IsEligible,
    decimal CoverageLevel,
    decimal ApprovedYield,
    decimal DemonstrationPrice,
    IReadOnlyCollection<EligibleField> Fields);

public sealed record EligibleField(
    Guid FieldId,
    string FieldNumber,
    decimal InsuredAcres);
