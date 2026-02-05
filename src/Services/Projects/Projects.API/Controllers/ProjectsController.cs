using Domeo.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projects.Application.Commands.Projects;
using Projects.Contracts.DTOs.Projects;
using Projects.Contracts.Routes;

namespace Projects.API.Controllers;

[ApiController]
[Route(ProjectsRoutes.Controller.Projects)]
public class ProjectsController : ControllerBase
{
    private readonly ISender _sender;

    public ProjectsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
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
        return Ok(ApiResponse<PaginatedResponse<ProjectDto>>.Ok(result));
    }

    [HttpGet(ProjectsRoutes.Controller.ById)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> GetProject(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetProjectByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<ProjectDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateProjectCommand(request.Name, request.Type, request.ClientId, request.Notes);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse<IdResponse>.Ok(new IdResponse(result), "Project created successfully"));
    }

    [HttpPut(ProjectsRoutes.Controller.ById)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProjectCommand(id, request.Name, request.Notes);
        await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse.Ok("Project updated successfully"));
    }

    [HttpPut(ProjectsRoutes.Controller.Status)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> UpdateProjectStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProjectStatusCommand(id, request.Status);
        await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse.Ok("Status updated successfully"));
    }

    [HttpPut(ProjectsRoutes.Controller.Questionnaire)]
    [Authorize(Roles = "sales,designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> UpdateQuestionnaire(Guid id, [FromBody] UpdateQuestionnaireRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateQuestionnaireCommand(id, request.QuestionnaireData);
        await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse.Ok("Questionnaire updated successfully"));
    }

    [HttpDelete(ProjectsRoutes.Controller.ById)]
    [Authorize(Roles = "designer,catalogAdmin,systemAdmin")]
    public async Task<IActionResult> DeleteProject(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteProjectCommand(id), cancellationToken);
        return Ok(ApiResponse.Ok("Project deleted successfully"));
    }
}
