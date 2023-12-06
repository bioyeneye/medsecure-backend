using Azure.Core;
using Humanizer;
using MedSecureSystem.Application.Commons;
using MedSecureSystem.Application.Dtos;
using MedSecureSystem.Application.Features.Delivery;
using MedSecureSystem.Application.Interfaces;
using MedSecureSystem.Domain.Entities;
using MedSecureSystem.Domain.Enums;
using MedSecureSystem.Shared.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace MedSecureSystem.API.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class DeliveryController : BaseIdentityController
    {
        public DeliveryController(UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider) : base(userManager, serviceProvider)
        {
        }

        [HttpPost]
        [Authorize(Roles = "Patient")]
        [SwaggerOperation(Summary = "Create delivery request", Description = "Create delivery request")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<DeliveryDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<DeliveryDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] CreateDeliveryRequestModel request)
        {

            var patientId = GetClaimValue("sub");
            var patientEmail = GetClaimValue("email");

            var deliveryService = _serviceProvider.GetRequiredService<IDeliveryService>();
            var result = await deliveryService.CreateDeliveryRequest(patientId, patientEmail, request);
            return HandleBusinessResult(result);
        }

        [HttpGet()]
        [SwaggerOperation(Summary = "All request creatd", Description = "Get all request created")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<PaginatedResult<DeliveryDto>>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<PaginatedResult<DeliveryDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRequests([FromQuery] PaginationQuery query)
        {
            var deliveryService = _serviceProvider.GetRequiredService<IDeliveryService>();
            var role = GetRole();
            var user = GetClaimValue("sub");
            var businessid = GetClaimValue("businessid");

            var request = await deliveryService.GetDeliveryRequests(user, role, businessid, query);
            return Ok(request);
        }


        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "get delivery request", Description = "get delivery request")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<DeliveryDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<DeliveryDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRequest([FromRoute] long id)
        {
            var deliveryService = _serviceProvider.GetRequiredService<IDeliveryService>();
            var role = GetRole();
            var user = GetClaimValue("sub");
            var businessid = GetClaimValue("businessid");

            var result = await deliveryService.GetDeliveryRequest(user, role, businessid, id);
            return HandleBusinessResult(result);
        }


        // POST: api/DeliveryRequest/AcceptByAgent
        [HttpPost("{id}/accept-preparation")]
        [Authorize(Roles = "BusinessAgent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<bool>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AcceptByAgent(long requestId)
        {
            var deliveryService = _serviceProvider.GetRequiredService<IDeliveryService>();
            var role = GetRole();
            var user = GetClaimValue("sub");
            var businessid = GetClaimValue("businessid");
            var email = GetClaimValue("email");

            return HandleBusinessResult(await deliveryService.AcceptRequestByAgent(requestId, user, email, businessid));
        }
        // POST: api/DeliveryRequest/CompletePreparationByAgent
        [HttpPost("{id}/complete-preparation")]
        [Authorize(Roles = "BusinessAgent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<bool>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompletePreparationByAgent([FromRoute] long requestId, [FromBody] CodeModel model)
        {
            var deliveryService = _serviceProvider.GetRequiredService<IDeliveryService>();
            var role = GetRole();
            var user = GetClaimValue("sub");
            var businessid = GetClaimValue("businessid");
            var email = GetClaimValue("email");

            return HandleBusinessResult(await deliveryService.CompletePreparationByAgent(requestId, user, model.Code, businessid));
        }


        // POST: api/DeliveryRequest/AssignDriver
        [HttpPost("{id}/accept-delivery")]
        [Authorize(Roles = "Driver")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<bool>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AssignDriver([FromRoute] long id)
        {
            var deliveryService = _serviceProvider.GetRequiredService<IDeliveryService>();
            var role = GetRole();
            var user = GetClaimValue("sub");
            var businessid = GetClaimValue("businessid");
            var email = GetClaimValue("email");

            return HandleBusinessResult(await deliveryService.AssignDriver(id, email, user));
        }

        // POST: api/DeliveryRequest/StartDeliveryByDriver
        [HttpPost("{id}/start-delivery")]
        [Authorize(Roles = "Driver")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<bool>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StartDeliveryByDriver([FromRoute] long id)
        {
            var deliveryService = _serviceProvider.GetRequiredService<IDeliveryService>();
            var role = GetRole();
            var user = GetClaimValue("sub");
            var businessid = GetClaimValue("businessid");
            var email = GetClaimValue("email");

            return HandleBusinessResult(await deliveryService.StartDeliveryByDriver(id, user));
        }

        // POST: api/DeliveryRequest/ConfirmDelivery
        [HttpPost("{id}/confirm-delivery")]
        [Authorize(Roles = "Driver")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<bool>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmDriverDelivery(long id, [FromBody] CodeModel model)
        {
            var deliveryService = _serviceProvider.GetRequiredService<IDeliveryService>();
            var role = GetRole();
            var user = GetClaimValue("sub");
            var businessid = GetClaimValue("businessid");
            var email = GetClaimValue("email");

            return HandleBusinessResult(await deliveryService.ConfirmDelivery(id, user, model.Code));
        }

        // POST: api/DeliveryRequest/ConfirmDelivery
        [HttpPost("{id}/accept-confirmation")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<bool>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmDelivery(long id, [FromBody] CodeModel model)
        {
            var deliveryService = _serviceProvider.GetRequiredService<IDeliveryService>();
            var role = GetRole();
            var user = GetClaimValue("sub");
            var businessid = GetClaimValue("businessid");
            var email = GetClaimValue("email");

            return HandleBusinessResult(await deliveryService.ConfirmPatientDelivery(id, user, model.Code));
        }
    }

    // 

    public class CodeModel
    {
        [Required]
        public string Code { get; set; }
    }
}
