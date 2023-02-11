using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Profiles
{
    /* this will be a handler and we want to return a list of activities based on a predicate and the username of the user
    whose profile we are looking at. 
    In this handler we want to return a list of UserActivityDto the user is attending and the
    predicate will either be:
        1. Activities in the past
        2. Activities the user is hosting
        3. The activities the user is going to in the future (default case) */

    public class ListActivities
    {
        public class Query : IRequest<Result<List<UserActivityDto>>> //return a list of UserActivityDtos
        {
            //based on a predicate and username (of user whose profile we are looking at)
            public string Username { get; set; }
            public string Predicate { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<UserActivityDto>>>
        {
            private readonly DataContext context; //information from DB
            private readonly IMapper mapper;
            public Handler(DataContext context, IMapper mapper)
            {
                this.mapper = mapper;
                this.context = context;
            }

            public async Task<Result<List<UserActivityDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var query = this.context.ActivityAttendees
                    .Where(u => u.AppUser.UserName == request.Username) //get the user
                    .OrderBy(a => a.Activity.Date) //order their activities by date
                    .ProjectTo<UserActivityDto>(this.mapper.ConfigurationProvider) //goes from actattendee to userativitydto
                    .AsQueryable();

                // var predicate = request.Predicate;
                // switch (predicate)
                // {
                //     case "past":
                //         query = query.Where(a => a.Date <= DateTime.Now);
                //         break;
                //     case "isHost":
                //         query = query.Where(a => a.HostUsername == request.Username);
                //         break;
                //     default:
                //         query = query.Where(a => a.Date >= DateTime.Now);
                //         break;
                // }
                query = request.Predicate switch
                {
                    "past" => query.Where(a => a.Date <= DateTime.Now),
                    "hosting" => query.Where(a => a.HostUsername ==
                    request.Username),
                    _ => query.Where(a => a.Date >= DateTime.Now)
                };

                var activities = await query.ToListAsync();

                return Result<List<UserActivityDto>>.Success(activities);
            }
        }
    }
}