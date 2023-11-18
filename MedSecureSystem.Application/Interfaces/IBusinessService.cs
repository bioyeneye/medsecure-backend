using MedSecureSystem.Application.Commons;
using MedSecureSystem.Application.Dtos;
using MedSecureSystem.Application.Features.Businesses.Commands;
using MedSecureSystem.Shared.Query;
using Microsoft.AspNetCore.Http;

namespace MedSecureSystem.Application.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBusinessService
    {
        Task<ApiResult<bool>> CompleteVerificationAsync(CompleteRegistrationCommand request);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        Task<ApiResult<BusinessDto>> CreateBusinessAsync(CreateBusinessCommand command, string url);
        Task<ApiResult<BusinessCredentialsDto>> EnableCredential(long id, string clientid, bool v);
        Task<ApiResult<BusinessCredentialsDto>> GenerateCredential(long id);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ApiResult<BusinessDto>> GetBusinessByIdAsync(long id);
        Task<ApiResult<BusinessDto>> GetBusinessByTenantKeyAsync(string tenantKey);
        Task<ApiResult<PaginatedResult<BusinessDetailsDto>>> GetBusinessessAsync(PaginationQuery query);
        Task<ApiResult<BusinessCredentialsDto>> GetCredentialById(long id, string clientid);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ApiResult<List<BusinessCredentialsDto>>> GetCredentials(long id);
    }

}

