using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities
{
    public class Create
    {
        //used in ActivitiesController.cs
        public class Command : IRequest<Result<Unit>> // "unit" comes from MediatR as "returns nothing" 
        {
            public Activity Activity { get; set; }
        }

        // uses the fluent validator class
        public class CommandValidator : AbstractValidator<Command> 
        {
            public CommandValidator() //constructor 
            {
                RuleFor(x => x.Activity).SetValidator(new ActivityValidator());
            }
        }


        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext context;
            public readonly IUserAccessor UserAccessor;
            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                this.UserAccessor = userAccessor;
                this.context = context;
                
            }
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await this.context.Users.FirstOrDefaultAsync(x => 
                    x.UserName == UserAccessor.GetUsername()); //gives access to user obj

                var attendee = new ActivityAttendee
                {
                    AppUser = user,
                    Activity = request.Activity,
                    IsHost = true
                };

                request.Activity.Attendees.Add(attendee); 

                this.context.Activities.Add(request.Activity); // saves the attendee in memory/activity table/entity framework

                var result = await this.context.SaveChangesAsync() > 0; // save the change 

                if (!result) {
                    return Result<Unit>.Failure("Failed to create activity");
                } 

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}