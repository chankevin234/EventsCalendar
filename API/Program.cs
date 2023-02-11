using API.Extensions;
using API.Middleware;
using API.SignalR;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Persistence;

var builder = WebApplication.CreateBuilder(args); //creates kestrel server

// Add services to the container
builder.Services.AddControllers(opt => 
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    opt.Filters.Add(new AuthorizeFilter(policy)); //every controller endpoint requires authentication now!
});
// Services have now been added to "Extensions" class
builder.Services.AddApplicationServices(builder.Configuration);
// service for using identity services
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build(); //builds the app

/* Configure the HTTP request pipeline. (TOP of MIDDLEWARE Tree)
MIDDLEWARE is assembled into an app pipeline to handle requests and response (decides if it will pass request to next component in pipeline) */
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment()) //applies middleware (controls api traffic in/out)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy"); // adds CORS middleware HEADER to allow cross domain requests 

app.UseAuthentication(); //checks if user is legit authed
app.UseAuthorization(); //is the user allowed to do this?

app.UseDefaultFiles(); //fetch index.html from wwwroot folder and serve to kestrel server
app.UseStaticFiles();

app.MapControllers();
app.MapHub<ChatHub>("/chat");
app.MapFallbackToController("Index", "Fallback");


// Get access to a service (scopes)
using var scope = app.Services.CreateScope(); //"using" means this is temp
var services = scope.ServiceProvider; 

try
{
    var context = services.GetRequiredService<DataContext>(); 
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    await context.Database.MigrateAsync();
    await Seed.SeedData(context, userManager);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred in migration");
}


app.Run();
