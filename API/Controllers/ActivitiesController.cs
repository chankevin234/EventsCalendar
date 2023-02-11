using Application.Activities;
using Application.Core;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    // [AllowAnonymous] // placed here for now to allow us to get our activities while setting up client login
    public class ActivitiesController : BaseApiController
    {
        [Authorize] // this means you are using the authentication service (not necessary now)
        [HttpGet] //api/activities
        public async Task<IActionResult> GetActivities([FromQuery]ActivityParams param) 
        {
            return HandlePagedResult(await Mediator.Send(new List.Query{Params = param})); //"List" is a .cs file from Application/Activities
        }

        [Authorize] // this means you are using the authentication service (not necessary now)
        [HttpGet("{id}")] //api/activities/<activity id>
        public async Task<ActionResult<Activity>> GetActivity(Guid id) 
        {
            // inherited from base api controller
            return HandleResult(await Mediator.Send(new Details.Query{Id = id}));
        }

        [HttpPost] //Post
        public async Task<IActionResult> CreateActivity(Activity activity)
        {
            return HandleResult(await Mediator.Send(new Create.Command {Activity = activity}));
        } 
        
        [Authorize(Policy = "IsActivityHost")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditActivity(Guid id, Activity activity) 
        {
            activity.Id = id;
            return HandleResult(await Mediator.Send(new Edit.Command{Activity =  activity}));
        }

        [Authorize(Policy = "IsActivityHost")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(Guid id)
        {
            return HandleResult(await Mediator.Send(new Delete.Command{Id = id}));
        }

        [HttpPost("{id}/attend")]
        public async Task<IActionResult> Attend(Guid id)
        {
            return HandleResult(await Mediator.Send(new UpdateAttendance.Command{Id = id}));
        }

    }

}