using Application.Core;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Activities
{
    public class Edit
    {
        public class Command : IRequest<Result<Unit>>
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
            public IMapper mapper;
            public Handler(DataContext context, IMapper mapper)
            {
                this.mapper = mapper;
                this.context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var activity = await this.context.Activities.FindAsync(request.Activity.Id); //tracking this activity in memory

                if (activity == null) {
                    return null;
                }

                // activity.Title = request.Activity.Title ?? activity.Title; // updates a single property -- WILL USE AUTOMAPPER
                this.mapper.Map(request.Activity, activity);

                var result = await this.context.SaveChangesAsync() > 0; //actual update bool

                if (!result) {
                    return Result<Unit>.Failure("Failed to update activity");
                }

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}