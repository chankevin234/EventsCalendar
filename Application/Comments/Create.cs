using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Comments
{
    public class Create //Create Handler
    {
        public class Command : IRequest<Result<CommentDto>>
        {
            public string Body { get; set; } //comment's body
            public Guid ActivityId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator() 
            {
                RuleFor(x => x.Body).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, Result<CommentDto>>
        {
            private readonly DataContext context;
            private readonly IMapper mapper;
            private readonly IUserAccessor userAccessor;
            
            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
            {
                this.userAccessor = userAccessor;
                this.mapper = mapper;
                this.context = context;
            }

            public async Task<Result<CommentDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                var activity = await this.context.Activities.FindAsync(request.ActivityId);

                if (activity == null) return null;

                var user = await this.context.Users
                    .Include(p => p.Photos)
                    .SingleOrDefaultAsync(x => x.UserName == this.userAccessor.GetUsername());
                
                var comment = new Comment
                {
                    Author = user,
                    Activity = activity,
                    Body = request.Body
                };

                activity.Comments.Add(comment);

                var success = await this.context.SaveChangesAsync() > 0;

                if (success) {
                    return Result<CommentDto>.Success(this.mapper.Map<CommentDto>(comment));
                }

                return Result<CommentDto>.Failure("Failed to add comment");
            }
        }
    }
}