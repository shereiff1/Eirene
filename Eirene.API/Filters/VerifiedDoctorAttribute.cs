using System.Security.Claims;
using System.Threading.Tasks;
using Eirene.DAL.Repository.Abstraction.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Eirene.API.Filters;

public class VerifiedDoctorFilter : IAsyncActionFilter
{
    private readonly IDoctorProfileRepository _doctorProfileRepository;

    public VerifiedDoctorFilter(IDoctorProfileRepository doctorProfileRepository)
    {
        _doctorProfileRepository = doctorProfileRepository;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var doctorProfile = await _doctorProfileRepository.GetByIdAsync(userId);
        if (doctorProfile == null || !doctorProfile.IsVerified)
        {
            context.Result = new ObjectResult(new { Message = "You must be a verified doctor to access this resource." })
            {
                StatusCode = 403
            };
            return;
        }

        await next();
    }
}

public class VerifiedDoctorAttribute : TypeFilterAttribute
{
    public VerifiedDoctorAttribute() : base(typeof(VerifiedDoctorFilter))
    {
    }
}
