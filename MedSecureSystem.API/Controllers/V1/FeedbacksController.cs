using MedSecureSystem.Application.Commons;
using MedSecureSystem.Application.Dtos;
using MedSecureSystem.Application.Features.Delivery;
using MedSecureSystem.Application.Interfaces;
using MedSecureSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MedSecureSystem.API.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class FeedbacksController : BaseIdentityController
    {
        public FeedbacksController(UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider) : base(userManager, serviceProvider)
        {
        }

        [HttpPost()]
        [Authorize(Roles = "BusinessAdmin,Patient,BusinessAgent,Driver")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<FeedbackModel>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<FeedbackModel>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateFeedback([FromBody] CreateDeliveryRequestFeedbackModel model)
        {
            var deliveryService = _serviceProvider.GetRequiredService<IFeedbackService>();
            var role = GetRole();
            var user = GetClaimValue("sub");
            var businessid = GetClaimValue("businessid");
            var email = GetClaimValue("email");

            return HandleBusinessResult(await deliveryService.AddFeedbackAsync(model, role, user, businessid));
        }

        [HttpGet("requestid")]
        [Authorize(Roles = "BusinessAdmin,Patient,BusinessAgent,Driver")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<IEnumerable<FeedbackModel>>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<IEnumerable<FeedbackModel>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AllFeedBacks(long requestid)
        {
            var deliveryService = _serviceProvider.GetRequiredService<IFeedbackService>();
            var role = GetRole();
            var user = GetClaimValue("sub");
            var businessid = GetClaimValue("businessid");
            var email = GetClaimValue("email");

            return HandleBusinessResult(await deliveryService.GetFeedbackByDeliveryRequestIdAsync(requestid));
        }

    }


    //
}
