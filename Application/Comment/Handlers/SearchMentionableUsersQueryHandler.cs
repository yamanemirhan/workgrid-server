//using MediatR;
//using Microsoft.AspNetCore.Http;

//namespace Application.Comment.Queries;

//internal class SearchMentionableUsersQueryHandler(ICommentService _commentService, 
//    IHttpContextAccessor _httpContextAccessor) : IRequestHandler<SearchMentionableUsersQuery, IEnumerable<Domain.Entities.User>>
//{
//    public async Task<IEnumerable<Domain.Entities.User>> Handle(SearchMentionableUsersQuery request, CancellationToken cancellationToken)
//    {
//        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
//        if (!Guid.TryParse(userIdClaim, out var userId))
//            throw new UnauthorizedAccessException("User ID not found in token");

//        return await _commentService.SearchMentionableUsersAsync(request.CardId, request.SearchTerm, userId);
//    }
//}