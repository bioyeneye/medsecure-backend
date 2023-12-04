using MedSecureSystem.Application.Commons;
using MedSecureSystem.Application.Dtos;
using MedSecureSystem.Application.Features.Delivery;

namespace MedSecureSystem.Application.Interfaces
{
    public interface IFeedbackService
    {
        Task<ApiResult<FeedbackModel>> AddFeedbackAsync(CreateDeliveryRequestFeedbackModel model, string role, string user, string businessid);
        Task<ApiResult<IEnumerable<FeedbackModel>>> GetFeedbackByDeliveryRequestIdAsync(long deliveryRequestId);
    }

}