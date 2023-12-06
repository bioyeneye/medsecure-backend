using MedSecureSystem.Application.Commons;
using MedSecureSystem.Application.Dtos;
using MedSecureSystem.Application.Features.Delivery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MedSecureSystem.API.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class FeedbacksController : ControllerBase
    {
        [HttpGet]
        [SwaggerOperation(Summary = "All feedback", Description = "Get all feedback created")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<PaginatedResult<DeliveryRequestFeedbackDto>>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<PaginatedResult<DeliveryRequestFeedbackDto>>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFeebacks()
        {

            return Ok();
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create delivery request feedback", Description = "Create delivery request feedback")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<DeliveryDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<DeliveryDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] Application.Features.Delivery.CreateDeliveryRequestModel request)
        {

            return Ok();
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "get delivery request feedback", Description = "get delivery request")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<DeliveryRequestFeedbackDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<DeliveryRequestFeedbackDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRequest(string id)
        {

            return Ok();
        }

        [HttpGet("{id}/status")]
        [SwaggerOperation(Summary = "Cancel delivery request", Description = "Create delivery request")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<DeliveryRequestFeedbackDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<DeliveryRequestFeedbackDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFeedbackStatus(string id)
        {

            return Ok();
        }

    }
}
