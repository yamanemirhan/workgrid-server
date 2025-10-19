using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application.Workspace.Commands;
using Application.Workspace.Queries;
using Shared.Responses;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WorkspaceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WorkspaceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkspace([FromBody] CreateWorkspaceCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Workspace created successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ResponseHelper.Unauthorized(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseHelper.Error("An error occurred while creating workspace"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserWorkspaces()
        {
            try
            {
                var result = await _mediator.Send(new GetUserWorkspacesQuery());
                return Ok(ResponseHelper.Success(result, "Workspaces retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ResponseHelper.Unauthorized(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving workspaces"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkspace(Guid id)
        {
            try
            {
                var result = await _mediator.Send(new GetWorkspaceQuery { Id = id });
                if (result == null)
                {
                    return NotFound(ResponseHelper.NotFound("Workspace not found"));
                }
                return Ok(ResponseHelper.Success(result, "Workspace retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ResponseHelper.Unauthorized(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ResponseHelper.NotFound(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving workspace"));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkspace(Guid id, [FromBody] UpdateWorkspaceCommand command)
        {
            try
            {
                command.Id = id; // Ensure the ID from route is used
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Workspace updated successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ResponseHelper.Unauthorized(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ResponseHelper.NotFound(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseHelper.Error("An error occurred while updating workspace"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkspace(Guid id)
        {
            try
            {
                var result = await _mediator.Send(new DeleteWorkspaceCommand { Id = id });
                return Ok(ResponseHelper.Success(result, "Workspace deleted successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ResponseHelper.Unauthorized(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ResponseHelper.NotFound(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseHelper.Error("An error occurred while deleting workspace"));
            }
        }
    }
}
