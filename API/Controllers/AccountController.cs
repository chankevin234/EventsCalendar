using System.Security.Claims;
using API.DTOs;
using API.Services;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    // [AllowAnonymous] //ensures that following endpoints don't need auth (can use the postman token) 
    //*** should use [Authorize] on individual controllers unless you want, AllowAnonymous will override it since it has been placed at the top level
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase //we aren't using the "basecontroller.cs" w/ Mediatr since we should keep business logic separate from identity
    {
        public UserManager<AppUser> userManager;
        public TokenService tokenService { get; }
        public AccountController(UserManager<AppUser> userManager, TokenService tokenService)
        {
            this.tokenService = tokenService;
            this.userManager = userManager;
            
        }

        [AllowAnonymous]
        [HttpPost("login")] //login endpoint
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await this.userManager.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(x => x.Email == loginDto.Email); //finds a user by email

            if (user == null) { //if a user doesn't exist, you get unauth
                return Unauthorized();
            }

            var result = await this.userManager.CheckPasswordAsync(user, loginDto.Password); //verify if pass matches user (bool)

            if (result) 
            {
                return CreateUserObject(user);
            }

            return Unauthorized(); //if pass don't match
        }

        // [Authorize]
        [AllowAnonymous] 
        [HttpPost("register")] //register endpoint
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            //check if email already exists
            if (await this.userManager.Users.AnyAsync(x => x.Email == registerDto.Email))
            {
                ModelState.AddModelError("email", "Email Taken");
                return ValidationProblem();
            }

            //check if username already exists
            if (await this.userManager.Users.AnyAsync(x => x.UserName == registerDto.Username))
            {
                ModelState.AddModelError("username", "Username Taken");
                return ValidationProblem();
            }

            var user = new AppUser
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                UserName = registerDto.Username
            };

            var result = await this.userManager.CreateAsync(user, registerDto.Password); //try to create a new userDto

            if (result.Succeeded)
            {
                return CreateUserObject(user);
            }

            return BadRequest(result.Errors);
        }

        [Authorize]
        [HttpGet] //get a user by claim
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await this.userManager.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(x => x.Email == User.FindFirstValue(ClaimTypes.Email));

            return CreateUserObject(user);

        }

        //Creates a user object to return as a UserDto
        private UserDto CreateUserObject(AppUser user)
        {
            return new UserDto // return a userdto
            {
                DisplayName = user.DisplayName,
                Image = user?.Photos?.FirstOrDefault(x => x.IsMain)?.Url, // ? means you will return null if image doesn't exist
                Token = this.tokenService.CreateToken(user),
                Username = user.UserName
            };
        }
    }
}