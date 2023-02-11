using Application.Core;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities
{
    public class List
    {
        public class Query : IRequest<Result<PagedList<ActivityDto>>> 
        {
            public ActivityParams Params { get; set; }
        } //mediator query

        public class Handler : IRequestHandler<Query, Result<PagedList<ActivityDto>>>
        {
            private readonly DataContext Context;
            private readonly IMapper mapper;
            private readonly IUserAccessor userAccessor;
            
            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor) 
            {
                this.userAccessor = userAccessor;
                this.mapper = mapper;
                this.Context = context;
            }

            public async Task<Result<PagedList<ActivityDto>>> Handle(Query request, CancellationToken token)
            { //cancellation tokens cancel a request after time
                var query = this.Context.Activities //list of activities
                    .Where(d => d.Date >= request.Params.StartDate)
                    .OrderBy(d => d.Date)
                    .ProjectTo<ActivityDto>(this.mapper.ConfigurationProvider,
                        new {currentUsername = this.userAccessor.GetUsername()})
                    .AsQueryable();

                if (request.Params.IsGoing && !request.Params.IsHost)
                {
                    query = query.Where(x => x.Attendees.Any(a => a.Username == this.userAccessor.GetUsername()));
                }

                if (request.Params.IsHost && !request.Params.IsGoing)
                {
                    query = query.Where(x => x.HostUsername == this.userAccessor.GetUsername());
                }

                return Result<PagedList<ActivityDto>>.Success(
                    await PagedList<ActivityDto>.CreateAsync(query, request.Params.pageNumber, 
                        request.Params.PageSize)
                );
            }
        }
    }
}