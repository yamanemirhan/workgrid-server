using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application.List.Commands;
using Application.List.Queries;
using Shared.Responses;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ListController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ListController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateList([FromBody] CreateListCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "List created successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while creating list"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetList(Guid id)
        {
            try
            {
                var result = await _mediator.Send(new GetListQuery { Id = id });
                if (result == null)
                {
                    return NotFound(ResponseHelper.NotFound("List not found"));
                }
                return Ok(ResponseHelper.Success(result, "List retrieved successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving list"));
            }
        }

        [HttpGet("board/{boardId}")]
        public async Task<IActionResult> GetBoardLists(Guid boardId)
        {
            try
            {
                var query = new GetBoardListsQuery { BoardId = boardId };
                var result = await _mediator.Send(query);
                return Ok(ResponseHelper.Success(result, "Lists retrieved successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving lists"));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateList(Guid id, [FromBody] UpdateListCommand command)
        {
            try
            {
                command.Id = id; // Ensure the ID from route is used
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "List updated successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while updating list"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteList(Guid id)
        {
            try
            {
                var result = await _mediator.Send(new DeleteListCommand { Id = id });
                return Ok(ResponseHelper.Success(result, "List deleted successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while deleting list"));
            }
        }
    }
}