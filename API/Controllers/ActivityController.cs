using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Shared.Responses;
using Application.Activity.Queries;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ActivityController : ControllerBase
{
    private readonly IMediator _mediator;

    public ActivityController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("workspace/{workspaceId}")]
    public async Task<IActionResult> GetWorkspaceActivities(Guid workspaceId, int take = 50)
    {
        try
        {
            var query = new GetWorkspaceActivitiesQuery { WorkspaceId = workspaceId, Take = take };
            var result = await _mediator.Send(query);
            return Ok(ResponseHelper.Success(result, "Workspace activities retrieved successfully"));
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
            return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving workspace activities"));
        }
    }

    [HttpGet("board/{boardId}")]
    public async Task<IActionResult> GetBoardActivities(Guid boardId, int take = 50)
    {
        try
        {
            var query = new GetBoardActivitiesQuery { BoardId = boardId, Take = take };
            var result = await _mediator.Send(query);
            return Ok(ResponseHelper.Success(result, "Board activities retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving board activities"));
        }
    }

    [HttpGet("list/{listId}")]
    public async Task<IActionResult> GetListActivities(Guid listId, int take = 50)
    {
        try
        {
            var query = new GetListActivitiesQuery { ListId = listId, Take = take };
            var result = await _mediator.Send(query);
            return Ok(ResponseHelper.Success(result, "List activities retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving list activities"));
        }
    }

    [HttpGet("card/{cardId}")]
    public async Task<IActionResult> GetCardActivities(Guid cardId, int take = 50)
    {
        try
        {
            var query = new GetCardActivitiesQuery { CardId = cardId, Take = take };
            var result = await _mediator.Send(query);
            return Ok(ResponseHelper.Success(result, "Card activities retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving card activities"));
        }
    }
}