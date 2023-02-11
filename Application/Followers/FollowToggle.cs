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

namespace Application.Followers
{
    public class FollowToggle
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string TargetUsername { get; set; } //get this from the request token
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext context;
            private readonly IUserAccessor userAccessor;
            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                this.userAccessor = userAccessor;
                this.context = context;

            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                //get the user you will use to FOLLOW the user
                var observer = await this.context.Users.FirstOrDefaultAsync(x => x.UserName == this.userAccessor.GetUsername());

                //get the user you are targeting to add a follower
                var target = await this.context.Users.FirstOrDefaultAsync(x => x.UserName == request.TargetUsername);
            
                if (target == null) {
                    return null;
                }

                var following = await this.context.UserFollowings.FindAsync(observer.Id, target.Id);

                //checks whether target has a following, if not, create one
                if (following == null) {
                    following = new UserFollowing
                    {
                        Observer = observer,
                        Target = target
                    };

                    this.context.UserFollowings.Add(following);
                }
                else {
                    this.context.UserFollowings.Remove(following);
                }

                var success = await this.context.SaveChangesAsync() > 0; 
                
                //check if this was done successfully
                if (success) {
                    return Result<Unit>.Success(Unit.Value);
                }

                return Result<Unit>.Failure("Failed to update following");


            }
        }
    }
}