using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities
{
    public class UpdateAttendance
    {
        //joins user if not part of event
        //removes user if already in event
        //if user is host, cancel event
        public class Command : IRequest<Result<Unit>> 
        {//uses MediatR (for responses) and .Core for Result obj type
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            public DataContext Context { get; }
            public IUserAccessor UserAccessor { get; }
            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                this.UserAccessor = userAccessor;
                this.Context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var activity = await this.Context.Activities
                    .Include(activity => activity.Attendees).ThenInclude(u => u.AppUser)
                    .FirstOrDefaultAsync(x => x.Id == request.Id); //get the activity
                
                if (activity == null) {
                    return null; //404 null
                }

                var user = await this.Context.Users.FirstOrDefaultAsync(x =>
                    x.UserName == this.UserAccessor.GetUsername());

                if (user == null) {
                    return null;
                }

                var HostUsername = activity.Attendees.FirstOrDefault(x => x.IsHost)?.AppUser.UserName;

                var attendance = activity.Attendees.FirstOrDefault(x => x.AppUser.UserName == user.UserName);

                if (attendance != null && HostUsername == user.UserName) { //this is the host, so cancel
                    activity.IsCancelled = !activity.IsCancelled;
                }

                if (attendance != null && HostUsername != user.UserName) { //normal user, remove them
                    activity.Attendees.Remove(attendance);
                }

                if (attendance == null) { //add them (but first create new attendance)
                    attendance = new ActivityAttendee
                    {
                        AppUser = user,
                        Activity = activity,
                        IsHost = false
                    };

                    activity.Attendees.Add(attendance); // add
                }

                var result = await this.Context.SaveChangesAsync() > 0;

                return result ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure("Problem updating");    

            }
        }
    }
}