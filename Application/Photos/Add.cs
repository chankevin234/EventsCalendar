using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Photos
{
    public class Add
    {
        public class Command : IRequest<Result<Photo>>
        {
            public IFormFile File { get; set; } 
        }

        public class Handler : IRequestHandler<Command, Result<Photo>>
        {
            public readonly DataContext Context;
            public readonly IPhotoAccessor PhotoAccessor;
            public readonly IUserAccessor UserAccessor;
            public Handler(DataContext context, IPhotoAccessor photoAccessor, IUserAccessor userAccessor)
            {
                this.UserAccessor = userAccessor;
                this.PhotoAccessor = photoAccessor;
                this.Context = context;
            }

            public async Task<Result<Photo>> Handle(Command request, CancellationToken cancellationToken)
            {
                //get current logged in user
                var user = await this.Context.Users.Include(p => p.Photos)
                    .FirstOrDefaultAsync(x => x.UserName == this.UserAccessor.GetUsername());
                
                if (user == null) return null;

                var photoUploadResult = await this.PhotoAccessor.AddPhoto(request.File);

                var photo = new Photo
                {
                    Url = photoUploadResult.Url,
                    Id = photoUploadResult.PublicId
                }; 

                // check if there is a MAIN user photo
                if (!user.Photos.Any(x => x.IsMain)) photo.IsMain = true;

                user.Photos.Add(photo);

                var result = await this.Context.SaveChangesAsync() > 0; //bool

                if (result) {
                    return Result<Photo>.Success(photo);
                }

                return Result<Photo>.Failure("Problem adding photo");
            }
        }
    }
}