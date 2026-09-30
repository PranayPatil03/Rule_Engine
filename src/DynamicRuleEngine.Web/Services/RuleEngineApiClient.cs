using System.Net.Http.Json;
using DynamicRuleEngine.Application.DTOs;

namespace DynamicRuleEngine.Web.Services;

public sealed class RuleEngineApiClient
{
    private readonly HttpClient _http;

    public RuleEngineApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<WorkflowSummaryDto>> GetWorkflowsAsync()
    {
        return await _http.GetFromJsonAsync<List<WorkflowSummaryDto>>(
            "api/configuration/workflow/workflows") ?? [];
    }

    public async Task<FrameworkDto> GetFrameworkAsync(string workflowCode)
    {
        var url = $"api/framework/{Uri.EscapeDataString(workflowCode)}";
        return (await _http.GetFromJsonAsync<FrameworkDto>(url))!;
    }

    public async Task<IReadOnlyList<ParameterDto>> GetParametersAsync(string workflowCode)
    {
        var url = $"api/configuration/parameters?workflowCode={Uri.EscapeDataString(workflowCode)}";
        return await _http.GetFromJsonAsync<List<ParameterDto>>(url) ?? [];
    }

    public async Task<ParameterDto> SaveParameterAsync(string workflowCode, ParameterDto dto)
    {
        HttpResponseMessage response;

        if (dto.Id == 0)
        {
            response = await _http.PostAsJsonAsync(
                $"api/configuration/parameters?workflowCode={Uri.EscapeDataString(workflowCode)}",
                dto);
        }
        else
        {
            response = await _http.PutAsJsonAsync(
                $"api/configuration/parameters/{dto.Id}?workflowCode={Uri.EscapeDataString(workflowCode)}",
                dto);
        }

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ParameterDto>())!;
    }

    public async Task DeleteParameterAsync(string workflowCode, int id)
    {
        var response = await _http.DeleteAsync(
            $"api/configuration/parameters/{id}?workflowCode={Uri.EscapeDataString(workflowCode)}");

        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<RuleDto>> GetRulesAsync(string workflowCode)
    {
        var url = $"api/configuration/rules?workflowCode={Uri.EscapeDataString(workflowCode)}";
        return await _http.GetFromJsonAsync<List<RuleDto>>(url) ?? [];
    }

    public async Task<RuleDto> SaveRuleAsync(string workflowCode, RuleDto dto)
    {
        HttpResponseMessage response;

        if (dto.Id == 0)
        {
            response = await _http.PostAsJsonAsync(
                $"api/configuration/rules?workflowCode={Uri.EscapeDataString(workflowCode)}",
                dto);
        }
        else
        {
            response = await _http.PutAsJsonAsync(
                $"api/configuration/rules/{dto.Id}?workflowCode={Uri.EscapeDataString(workflowCode)}",
                dto);
        }

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RuleDto>())!;
    }

    public async Task DeleteRuleAsync(string workflowCode, int id)
    {
        var response = await _http.DeleteAsync(
            $"api/configuration/rules/{id}?workflowCode={Uri.EscapeDataString(workflowCode)}");

        response.EnsureSuccessStatusCode();
    }

    public async Task<WorkflowJsonDto> GetWorkflowJsonAsync(string workflowCode)
    {
        var url = $"api/configuration/workflow/json?workflowCode={Uri.EscapeDataString(workflowCode)}";
        return (await _http.GetFromJsonAsync<WorkflowJsonDto>(url))!;
    }

    public async Task<WorkflowJsonDto> SaveWorkflowJsonAsync(string workflowCode, string jsonContent)
    {
        var response = await _http.PutAsJsonAsync(
            $"api/configuration/workflow/json?workflowCode={Uri.EscapeDataString(workflowCode)}",
            new WorkflowJsonSaveDto { JsonContent = jsonContent });

        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(string.IsNullOrWhiteSpace(message) ? "Workflow JSON save failed." : message);
        }

        return (await response.Content.ReadFromJsonAsync<WorkflowJsonDto>())!;
    }

    public async Task<WorkflowJsonDto> PublishAsync(string workflowCode)
    {
        var response = await _http.PostAsync(
            $"api/configuration/workflow/publish?workflowCode={Uri.EscapeDataString(workflowCode)}",
            null);

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WorkflowJsonDto>())!;
    }

    public async Task<EvaluateResponseDto> EvaluateFrameworkAsync(FrameworkEvaluationRequestDto request)
    {
        var response = await _http.PostAsJsonAsync(
            "api/rule-engine/evaluate-framework",
            request);

        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                string.IsNullOrWhiteSpace(message)
                    ? "Framework calculation failed."
                    : message);
        }

        return (await response.Content.ReadFromJsonAsync<EvaluateResponseDto>())
            ?? throw new InvalidOperationException(
                "Framework calculation API returned no result.");
    }

    public async Task<EvaluateResponseDto> EvaluateAsync(EvaluateRequestDto request)
    {
        var response = await _http.PostAsJsonAsync(
            "api/rule-engine/evaluate",
            request);

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<EvaluateResponseDto>())!;
    }

    public async Task<GenerateReportResponseDto> GenerateReportAsync(EvaluateRequestDto request)
    {
        var response = await _http.PostAsJsonAsync(
            "api/report/generate",
            request);

        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                string.IsNullOrWhiteSpace(message)
                    ? "Report generation failed."
                    : message);
        }

        var result = await response.Content.ReadFromJsonAsync<GenerateReportResponseDto>();
        if (result is null)
            throw new InvalidOperationException("The report API returned no result.");

        return result;
    }
}
