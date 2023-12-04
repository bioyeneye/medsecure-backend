using MedSecureSystem.Application.Commons;
using MedSecureSystem.Application.Dtos;
using MedSecureSystem.Application.Features.Delivery;
using MedSecureSystem.Shared.Query;

namespace MedSecureSystem.Application.Interfaces
{
    public interface IDeliveryService
    {
        Task<ApiResult<bool>> AcceptRequestByAgent(long requestId, string agentId, string email, string businessId);
        Task<ApiResult<bool>> AssignDriver(long requestId, string email, string driverId);
        Task<ApiResult<bool>> CancelRequestAsync(long requestId, string patientid);
        Task<ApiResult<bool>> CompletePreparationByAgent(long requestId, string agentId, string code, string businessId);
        Task<ApiResult<bool>> ConfirmDelivery(long requestId, string driverId, string deliveryCode);
        Task<ApiResult<bool>> ConfirmPatientDelivery(long requestId, string patientid, string deliveryCode);
        Task<ApiResult<DeliveryDto>> CreateDeliveryRequest(string patientId, string patientEmail, CreateDeliveryRequestModel request);
        Task<ApiResult<DeliveryDto>> GetDeliveryRequest(string user, string role, string businessid, long id);
        Task<ApiResult<PaginatedResult<DeliveryDto>>> GetDeliveryRequests(string user, string role, string businessid, PaginationQuery pagination);
        Task<ApiResult<bool>> StartDeliveryByDriver(long requestId, string driverId);
    }

}