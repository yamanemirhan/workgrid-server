using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application.Board.Commands;
using Application.Board.Queries;
using Shared.Responses;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BoardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BoardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBoard([FromBody] CreateBoardCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Board created successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ResponseHelper.Unauthorized(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ResponseHelper.Error(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseHelper.Error("An error occurred while creating board"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBoard(Guid id)
        {
            try
            {
                var result = await _mediator.Send(new GetBoardQuery { Id = id });
                if (result == null)
                {
                    return NotFound(ResponseHelper.NotFound("Board not found"));
                }
                return Ok(ResponseHelper.Success(result, "Board retrieved successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving board"));
            }
        }

        [HttpGet("workspace/{workspaceId}")]
        public async Task<IActionResult> GetWorkspaceBoards(Guid workspaceId)
        {
            try
            {
                var query = new GetWorkspaceBoardsQuery { WorkspaceId = workspaceId };
                var result = await _mediator.Send(query);
                return Ok(ResponseHelper.Success(result, "Boards retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ResponseHelper.Unauthorized(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving boards"));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBoard(Guid id, [FromBody] UpdateBoardCommand command)
        {
            try
            {
                command.Id = id; // Ensure the ID from route is used
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Board updated successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while updating board"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBoard(Guid id)
        {
            try
            {
                var result = await _mediator.Send(new DeleteBoardCommand { Id = id });
                return Ok(ResponseHelper.Success(result, "Board deleted successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while deleting board"));
            }
        }

        // Board Member Assignment Endpoints
        [HttpPost("{boardId}/members/{userId}")]
        public async Task<IActionResult> AssignUserToBoard(Guid boardId, Guid userId)
        {
            try
            {
                var command = new AssignUserToBoardCommand { BoardId = boardId, UserId = userId };
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "User assigned to board successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ResponseHelper.Unauthorized(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ResponseHelper.Error(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ResponseHelper.Error(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseHelper.Error("An error occurred while assigning user to board"));
            }
        }

        [HttpDelete("{boardId}/members/{userId}")]
        public async Task<IActionResult> UnassignUserFromBoard(Guid boardId, Guid userId)
        {
            try
            {
                var command = new UnassignUserFromBoardCommand { BoardId = boardId, UserId = userId };
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "User unassigned from board successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ResponseHelper.Unauthorized(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ResponseHelper.Error(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ResponseHelper.Error(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseHelper.Error("An error occurred while unassigning user from board"));
            }
        }
    }
}
