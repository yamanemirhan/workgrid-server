using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application.Member.Commands;
using Application.Member.Queries;
using Shared.Responses;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MemberController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MemberController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("invite")]
        public async Task<IActionResult> InviteMember([FromBody] InviteMemberCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Invitation sent successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while sending invitation"));
            }
        }

        [HttpPost("accept-invitation")]
        public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Invitation accepted successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while accepting invitation"));
            }
        }

        [AllowAnonymous]
        [HttpPost("accept-invitation/public")]
        public async Task<IActionResult> AcceptPublicInvitation([FromBody] AcceptPublicInvitationCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Invitation accepted successfully. Welcome to the workspace!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ResponseHelper.Error(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(ResponseHelper.Error(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ResponseHelper.Error(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseHelper.Error("An error occurred while accepting invitation"));
            }
        }

        [HttpPost("invitations/{invitationId}/resend")]
        public async Task<IActionResult> ResendInvitation(Guid invitationId)
        {
            try
            {
                var command = new ResendInvitationCommand { InvitationId = invitationId };
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Invitation resent successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while resending invitation"));
            }
        }

        [HttpDelete("invitations/{invitationId}")]
        public async Task<IActionResult> CancelInvitation(Guid invitationId)
        {
            try
            {
                var command = new CancelInvitationCommand { InvitationId = invitationId };
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Invitation cancelled successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while cancelling invitation"));
            }
        }

        [HttpGet("invitations/{workspaceId}")]
        public async Task<IActionResult> GetWorkspaceInvitations(Guid workspaceId)
        {
            try
            {
                var query = new GetWorkspaceInvitationsQuery { WorkspaceId = workspaceId };
                var result = await _mediator.Send(query);
                return Ok(ResponseHelper.Success(result, "Invitations retrieved successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving invitations"));
            }
        }

        [HttpGet("{workspaceId}/members")]
        public async Task<IActionResult> GetWorkspaceMembers(Guid workspaceId)
        {
            try
            {
                var query = new GetWorkspaceMembersQuery { WorkspaceId = workspaceId };
                var result = await _mediator.Send(query);
                return Ok(ResponseHelper.Success(result, "Workspace members retrieved successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving workspace members"));
            }
        }

        [HttpPut("{workspaceId}/members/{memberId}/role")]
        public async Task<IActionResult> UpdateMemberRole(Guid workspaceId, Guid memberId, [FromBody] UpdateMemberRoleCommand command)
        {
            try
            {
                // Ensure the IDs match
                command.WorkspaceId = workspaceId;
                command.MemberId = memberId;
                
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Member role updated successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while updating member role"));
            }
        }

        [HttpPost("{workspaceId}/leave")]
        public async Task<IActionResult> LeaveWorkspace(Guid workspaceId)
        {
            try
            {
                var command = new LeaveWorkspaceCommand { WorkspaceId = workspaceId };
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "You have successfully left the workspace"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while leaving the workspace"));
            }
        }

        [HttpDelete("{workspaceId}/members/{memberId}")]
        public async Task<IActionResult> RemoveMember(Guid workspaceId, Guid memberId)
        {
            try
            {
                var command = new RemoveMemberCommand { WorkspaceId = workspaceId, MemberId = memberId };
                var result = await _mediator.Send(command);
                return Ok(ResponseHelper.Success(result, "Member removed successfully"));
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
                return StatusCode(500, ResponseHelper.Error("An error occurred while removing member"));
            }
        }
    }
}
