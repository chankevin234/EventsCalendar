using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Core;
using MediatR;
using Persistence;

namespace Application.Activities
{
    public class Delete
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id {get; set;}
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            public DataContext Context;
            public Handler(DataContext context)
            {
                this.Context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                //check if activity is null
                var activity = await this.Context.Activities.FindAsync(request.Id);
                
                if (activity == null) { //if null, return null value and 404 not found
                    return null;
                }

                this.Context.Remove(activity); //removes from memory

                var result = await this.Context.SaveChangesAsync() > 0; // result becomes a bool, is it greater than 0?

                if (!result) {
                    return Result<Unit>.Failure("Failed to delete activity");
                }

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
}