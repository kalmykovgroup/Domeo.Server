using Domeo.Shared.Contracts;
using Domeo.Shared.Contracts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projects.Abstractions.Commands.Projects;
using Projects.Abstractions.DTOs;

namespace Projects.API.Controllers;

[ApiController]
[Route("projects")]
public class ProjectsController : ControllerBase
{
    private readonly ISender _sender;

    public ProjectsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = "Permission:projects:read")]
    public async Task<IActionResult> GetProjects(
        [FromQuery] Guid? clientId,
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? type,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortOrder,
        CancellationToken cancellationToken)
    {
        var query = new GetProjectsQuery(
            clientId,
            search,
            status,
            type,
            page ?? 1,
            pageSize ?? 20,
            sortBy,
            sortOrder);

        var result = await _sender.Send(query, cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse<PaginatedResponse<ProjectDto>>.Ok(result.Value))
            : Ok(ApiResponse<PaginatedResponse<ProjectDto>>.Fail(result.Error.Description));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:projects:read")]
    public async Task<IActionResult> GetProject(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetProjectByIdQuery(id), cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse<ProjectDto>.Ok(result.Value))
            : Ok(ApiResponse<ProjectDto>.Fail(result.Error.Description));
    }

    [HttpPost]
    [Authorize(Policy = "Permission:projects:write")]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateProjectCommand(request.Name, request.Type, request.ClientId, request.Notes);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse<IdResponse>.Ok(new IdResponse(result.Value), "Project created successfully"))
            : Ok(ApiResponse<IdResponse>.Fail(result.Error.Description));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Permission:projects:write")]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProjectCommand(id, request.Name, request.Notes);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Project updated successfully"))
            : Ok(ApiResponse.Fail(result.Error.Description));
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = "Permission:projects:write")]
    public async Task<IActionResult> UpdateProjectStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProjectStatusCommand(id, request.Status);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Status updated successfully"))
            : Ok(ApiResponse.Fail(result.Error.Description));
    }

    [HttpPut("{id:guid}/questionnaire")]
    [Authorize(Policy = "Permission:projects:write")]
    public async Task<IActionResult> UpdateQuestionnaire(Guid id, [FromBody] UpdateQuestionnaireRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateQuestionnaireCommand(id, request.QuestionnaireData);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Questionnaire updated successfully"))
            : Ok(ApiResponse.Fail(result.Error.Description));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Permission:projects:delete")]
    public async Task<IActionResult> DeleteProject(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteProjectCommand(id), cancellationToken);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Project deleted successfully"))
            : Ok(ApiResponse.Fail(result.Error.Description));
    }
}
