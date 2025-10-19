using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application.CardStatus.Commands;
using Application.CardStatus.Queries;
using Shared.Responses;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CardStatusController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CardStatusController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("workspace/{workspaceId}")]
        public async Task<IActionResult> GetWorkspaceCardStatuses(Guid workspaceId)
        {
            try
            {
                var query = new GetWorkspaceCardStatusesQuery { WorkspaceId = workspaceId };
                var result = await _mediator.Send(query);
                return Ok(ResponseHelper.Success(result, "Card statuses retrieved successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving card statuses"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCardStatusById(Guid id)
        {
            try
            {
                var query = new GetCardStatusByIdQuery { Id = id };
                var result = await _mediator.Send(query);
                
                if (result == null)
                {
                    return NotFound(ResponseHelper.Error("Card status not found"));
                }

                return Ok(ResponseHelper.Success(result, "Card status retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ResponseHelper.Unauthorized(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving card status"));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCardStatus([FromBody] CreateCardStatusCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Card status created successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while creating card status"));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCardStatus(Guid id, [FromBody] UpdateCardStatusCommand command)
        {
            try
            {
                command.Id = id;
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Card status updated successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while updating card status"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCardStatus(Guid id)
        {
            try
            {
                var command = new DeleteCardStatusCommand { Id = id };
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Card status deleted successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while deleting card status"));
            }
        }    
    }
}