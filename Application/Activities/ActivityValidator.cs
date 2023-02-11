using Domain;
using FluentValidation;

namespace Application.Activities //folder path
{
    public class ActivityValidator : AbstractValidator<Activity> //pass in an activity to validate
    {
        public ActivityValidator()
        {
            RuleFor(x => x.Title).NotEmpty();    
            RuleFor(x => x.Description).NotEmpty();    
            RuleFor(x => x.Date).NotEmpty();    
            RuleFor(x => x.Category).NotEmpty();    
            RuleFor(x => x.City).NotEmpty();    
            RuleFor(x => x.Venue).NotEmpty();    
        }
        
    }
}