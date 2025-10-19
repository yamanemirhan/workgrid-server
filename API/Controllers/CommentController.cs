using Application.Comment.Commands;
using Application.Comment.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Responses;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CommentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("card/{cardId}")]
        public async Task<IActionResult> GetCardComments(Guid cardId)
        {
            try
            {
                var query = new GetCardCommentsQuery { CardId = cardId };
                var result = await _mediator.Send(query);
                return Ok(ResponseHelper.Success(result, "Comments retrieved successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving comments"));
            }
        }     
     
        [HttpPost]
        public async Task<IActionResult> CreateComment([FromForm] CreateCommentCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Comment created successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while creating comment"));
            }
        }

        [HttpPut("{commentId}")]
        public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] UpdateCommentCommand command)
        {
            try
            {
                command.CommentId = commentId;
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Comment updated successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while updating comment"));
            }
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            try
            {
                var command = new DeleteCommentCommand { CommentId = commentId };
                await _mediator.Send(command);
                return Ok(ResponseHelper.Success("Comment deleted successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while deleting comment"));
            }
        }

        [HttpPost("{commentId}/reactions")]
        public async Task<IActionResult> AddReaction(Guid commentId, [FromBody] AddCommentReactionCommand command)
        {
            try
            {
                command.CommentId = commentId;
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Reaction added successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while adding reaction"));
            }
        }

        [HttpDelete("{commentId}/reactions")]
        public async Task<IActionResult> RemoveReaction(Guid commentId)
        {
            try
            {
                var command = new RemoveCommentReactionCommand { CommentId = commentId };
                var result = await _mediator.Send(command);
                if (!result)
                    return NotFound(ResponseHelper.Error("Reaction not found"));

                return Ok(ResponseHelper.Success("Reaction removed successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while removing reaction"));
            }
        }
    }
}