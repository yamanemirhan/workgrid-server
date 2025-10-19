using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application.Card.Commands;
using Application.Card.Queries;
using Shared.Responses;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCard([FromBody] CreateCardCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Card created successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while creating card"));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCard(Guid id, [FromBody] UpdateCardCommand command)
        {
            try
            {
                command.Id = id;
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Card updated successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while updating card"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCard(Guid id)
        {
            try
            {
                var command = new DeleteCardCommand { Id = id };
                await _mediator.Send(command);
                return Ok(ResponseHelper.Success("Card deleted successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while deleting card"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCardDetails(Guid id)
        {
            try
            {
                var query = new GetCardDetailsQuery { Id = id };
                var result = await _mediator.Send(query);
                return Ok(ResponseHelper.Success(result, "Card retrieved successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving card"));
            }
        }

        [HttpGet("list/{listId}")]
        public async Task<IActionResult> GetListCards(Guid listId)
        {
            try
            {
                var query = new GetListCardsQuery { ListId = listId };
                var result = await _mediator.Send(query);
                return Ok(ResponseHelper.Success(result, "Cards retrieved successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving cards"));
            }
        }

        // Card Status Management
        [HttpPut("{cardId}/status/{statusId}")]
        public async Task<IActionResult> ChangeCardStatus(Guid cardId, Guid statusId)
        {
            try
            {
                var command = new ChangeCardStatusCommand { CardId = cardId, StatusId = statusId };
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Card status changed successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while changing card status"));
            }
        }

        // Card Member Assignment Endpoints
        [HttpPost("{cardId}/members/{userId}")]
        public async Task<IActionResult> AssignUserToCard(Guid cardId, Guid userId)
        {
            try
            {
                var command = new AssignUserToCardCommand { CardId = cardId, UserId = userId };
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "User assigned to card successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while assigning user to card"));
            }
        }

        [HttpDelete("{cardId}/members/{userId}")]
        public async Task<IActionResult> UnassignUserFromCard(Guid cardId, Guid userId)
        {
            try
            {
                var command = new UnassignUserFromCardCommand { CardId = cardId, UserId = userId };
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "User unassigned from card successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while unassigning user from card"));
            }
        }

        // Card Follow Endpoints
        [HttpPost("{cardId}/follow")]
        public async Task<IActionResult> FollowCard(Guid cardId)
        {
            try
            {
                var command = new FollowCardCommand { CardId = cardId };
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Card followed successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while following card"));
            }
        }

        [HttpDelete("{cardId}/follow")]
        public async Task<IActionResult> UnfollowCard(Guid cardId)
        {
            try
            {
                var command = new UnfollowCardCommand { CardId = cardId };
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Card unfollowed successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while unfollowing card"));
            }
        }
    }
}
