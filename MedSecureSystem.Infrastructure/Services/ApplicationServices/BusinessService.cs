using AutoMapper;
using Azure.Core;
using MedSecureSystem.Application.Commons;
using MedSecureSystem.Application.Dtos;
using MedSecureSystem.Application.Features.Businesses.Commands;
using MedSecureSystem.Application.Interfaces;
using MedSecureSystem.Domain.Entities;
using MedSecureSystem.Domain.Enums;
using MedSecureSystem.Domain.Interfaces;
using MedSecureSystem.Shared.Attributes;
using MedSecureSystem.Shared.Helpers;
using MedSecureSystem.Shared.Query;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MedSecureSystem.Infrastructure.Services.ApplicationServices
{
    [InfrastructureServiceLifetime(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped)]
    public class BusinessService : IBusinessService
    {
        private readonly IGenericRepository<Business> _businessRepository;
        private readonly IGenericRepository<BusinessCredential> _businessCredentialRepository;
        private readonly ILogger<BusinessService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly OpenIddictApplicationManager<CustomApplication> _applicationManager;
        private readonly IEmailSender _emailSender; // Assuming you have an email service implemented

        public BusinessService(IGenericRepository<Business> businessRepository, ILogger<BusinessService> logger, IMapper mapper, UserManager<ApplicationUser> userManager, OpenIddictApplicationManager<CustomApplication> applicationManager, IEmailSender emailSender, IGenericRepository<BusinessCredential> businessCredentialRepository)
        {
            _businessRepository = businessRepository;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _applicationManager = applicationManager;
            _emailSender = emailSender;
            _businessCredentialRepository = businessCredentialRepository;
        }

        public async Task<ApiResult<PaginatedResult<BusinessDetailsDto>>> GetBusinessessAsync(PaginationQuery query)
        {
            try
            {
                var result = await _businessRepository.GetAllPaginatedAsync(
                                orderBy: q => q.OrderBy(b => b.Name),
                                currentPage: query.Page,
                                pageSize: query.PageSize);


                return ApiResult<PaginatedResult<BusinessDetailsDto>>.SuccessResult(new PaginatedResult<BusinessDetailsDto>(
                    result.Items.Any() ? _mapper.Map<IEnumerable<BusinessDetailsDto>>(result.Items) : Enumerable.Empty<BusinessDetailsDto>()
                    , result.TotalCount, query.Page, query.PageSize));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}", nameof(GetBusinessessAsync));
                throw;
            }


        }

        public async Task<ApiResult<BusinessDto>> CreateBusinessAsync(CreateBusinessCommand command, string url)
        {
            try
            {
                var verificationToken = GenerateVerificationToken(); // Implement this method

                var business = _mapper.Map<Business>(command);
                business.BusinessUniqueKey = Guid.NewGuid().ToString();
                business.IsActive = false;
                business.IsVerified = false;
                business.VerificationToken = verificationToken;

                await _businessRepository.AddAsync(business);
                await _businessRepository.SaveAsync();

                if (business.Id > 0)
                {
                    var templatePath = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("Templates", "BusinessVerificationEmailTemplate.html"));
                    var template = await File.ReadAllTextAsync(templatePath);


                    var verificationUrl = $"{url}?email={WebUtility.UrlEncode(command.Email)}&token={WebUtility.UrlEncode(verificationToken)}";

                    template = template.Replace("{name}", command.Name)
                        .Replace("{VerificationUrl}", verificationUrl);

                    await _emailSender.SendEmailAsync(command.Email, "MedSecure: Business created", template);

                    return ApiResult<BusinessDto>.SuccessResult(_mapper.Map<BusinessDto>(business), "Business created successfully");
                }

                return ApiResult<BusinessDto>.FailureResult(null, "Error creating busines");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}: {message}", nameof(CreateBusinessAsync), ex.Message);
                throw;
            }
        }

        public async Task<ApiResult<BusinessDto>> GetBusinessByTenantKeyAsync(string tenantKey)
        {
            try
            {
                var business = await _businessRepository.FirstOrDefaultAsync(b => b.BusinessUniqueKey == tenantKey);
                if (business == null) return ApiResult<BusinessDto>.NotFoundResult($"Business with Tentant Key: {tenantKey} not found");
                return ApiResult<BusinessDto>.SuccessResult(_mapper.Map<BusinessDto>(business));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}: {message}", nameof(GetBusinessByIdAsync), ex.Message);
                throw;
            }
        }

        public async Task<ApiResult<BusinessDto>> GetBusinessByIdAsync(long id)
        {
            try
            {
                var business = await _businessRepository.GetByIdAsync(id);
                if (business == null) return ApiResult<BusinessDto>.NotFoundResult($"Business with id: {id} not found");
                return ApiResult<BusinessDto>.SuccessResult(_mapper.Map<BusinessDto>(business));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}: {message}", nameof(GetBusinessByIdAsync), ex.Message);
                throw;
            }
        }

        private string GenerateVerificationToken()
        {
            var tokenData = new byte[32];
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(tokenData);
            return Convert.ToBase64String(tokenData);

        }

        public async Task<ApiResult<bool>> CompleteVerificationAsync(CompleteRegistrationCommand request)
        {
            try
            {
                await _businessRepository.BeginTransactionAsync();
                // request.Email = WebUtility.UrlDecode(request.Email);
                // request.Token = WebUtility.UrlDecode(request.Token);
                var business = await _businessRepository.FirstOrDefaultAsync(b => b.Email == request.Email);

                if (business == null) return ApiResult<bool>.NotFoundResult($"Business with email: {request.Email} not found");
                if (business.IsVerified) return ApiResult<bool>.FailureResult(new List<string> { $"Business with email: {request.Email} is verified" }, $"Business with email: {request.Email} is verified");

                if (business != null && business.VerificationToken == request.Token)
                {
                    business.IsVerified = true;
                    business.IsActive = true;
                    business.VerificationToken = ""; // Clear the token
                    _businessRepository.Update(business);

                    var businessAdminUser = new ApplicationUser
                    {
                        UserName = request.Email,
                        Email = request.Email,
                        TenantKey = business.BusinessUniqueKey,
                        EmailConfirmed = true
                    };
                    var tempPassword = PasswordHelper.GenerateTemporaryPassword(); // Implement this method to generate a secure temporary password
                    var createUserResult = await _userManager.CreateAsync(businessAdminUser, tempPassword);

                    if (!createUserResult.Succeeded)
                    {
                        _logger.LogInformation("Error user creation, {method}: {message}", nameof(CompleteVerificationAsync), createUserResult);
                        return ApiResult<bool>.FailureResult(createUserResult.Errors.Select(c => c.Description).ToList(), $"Business admin can't be created");

                    }
                    // Store the flag that indicates a password change is required
                    await _userManager.SetAuthenticationTokenAsync(businessAdminUser, "Default", "PasswordChangeRequired", "Yes");
                    // Add user to Business Admin role
                    await _userManager.AddToRoleAsync(businessAdminUser, UserTypes.BusinessAdmin.ToString());

                    var templatePath = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("Templates", "BusinessAccountVerificationEmailTemplate.html"));
                    var template = await File.ReadAllTextAsync(templatePath);

                    template = template.Replace("{name}", business.Name)
                        .Replace("{tempPassword}", tempPassword);

                    _logger.LogInformation("{message}: {pass}", template, tempPassword);


                    // Send an email to the business admin to complete the registration process
                    // Implement the SendEmailAsync method according to your email service provider
                    await _emailSender.SendEmailAsync(request.Email, "Complete Your Business Admin Registration", template);

                    await _businessRepository.CommitTransactionAsync();
                    return ApiResult<bool>.SuccessResult(true, "Business verifed");
                }

                return ApiResult<bool>.FailureResult(new List<string> { $"Verficayion token is invalid" }, $"Verficayion token is invalid");
            }
            catch (Exception ex)
            {
                _businessRepository.RollbackTransaction();
                _logger.LogError(ex, "{method}: {message}", nameof(CompleteVerificationAsync), ex.Message);

                throw;
            }

        }

        public async Task<ApiResult<BusinessCredentialsDto>> GenerateCredential(long id)
        {
            try
            {
                await _businessRepository.BeginTransactionAsync();

                var business = await _businessRepository.GetByIdAsync(id);
                if (business == null) return ApiResult<BusinessCredentialsDto>.NotFoundResult($"Business with id: {id} not found");

                // Generate client ID and secret.
                var clientId = Guid.NewGuid().ToString("N");
                var clientSecret = Guid.NewGuid().ToString("N"); // This should be a strong, unique secret.

                // Hash the client secret using SHA256.
                var hashedSecret = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(clientSecret)));

                // Create the application descriptor.
                var descriptor = new CustomApplication
                {
                    ClientId = clientId,
                    DisplayName = business.Name,
                    IsActive = true,
                    BusinessKey = business.BusinessUniqueKey,
                    Permissions = JsonSerializer.Serialize(new List<string>
                    {
                        OpenIddictConstants.Permissions.Endpoints.Token,
                            OpenIddictConstants.Permissions.Endpoints.Revocation,
                            OpenIddictConstants.Permissions.Endpoints.Authorization,
                            OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                            OpenIddictConstants.Permissions.GrantTypes.Password,
                            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    })
                };

                // Create the application in the OpenIddict database.

                var credential = new BusinessCredential
                {
                    ClientSecret = EncryptionHelper.EncryptSecret(hashedSecret, clientId),
                    ClientId = clientId,
                    BusinessId = id,
                    IsActive = true,
                };

                await _applicationManager.CreateAsync(descriptor, hashedSecret);

                await _businessCredentialRepository.AddAsync(credential);
                await _businessRepository.CommitTransactionAsync();

                return ApiResult<BusinessCredentialsDto>.SuccessResult(new BusinessCredentialsDto
                {
                    ClientId = clientId,
                    ClientSecret = hashedSecret,
                    IsActive = true
                });
            }
            catch (Exception ex)
            {
                _businessRepository.RollbackTransaction();
                _logger.LogError(ex, "{method}: {message}", nameof(GenerateCredential), ex.Message);

                throw;
            }
        }

        public async Task<ApiResult<List<BusinessCredentialsDto>>> GetCredentials(long id)
        {
            try
            {

                var business = await _businessRepository.GetByIdAsync(id);
                if (business == null) return ApiResult<List<BusinessCredentialsDto>>.NotFoundResult($"Business with id: {id} not found");

                var credentials = await _businessCredentialRepository.GetAll(c => c.BusinessId == id);

                return ApiResult<List<BusinessCredentialsDto>>.SuccessResult(credentials.Count > 0
                   ? _mapper.Map<IEnumerable<BusinessCredentialsDto>>(credentials).ToList()
                   : Enumerable.Empty<BusinessCredentialsDto>().ToList());

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}: {message}", nameof(GetCredentials), ex.Message);

                throw;
            }
        }

        public async Task<ApiResult<BusinessCredentialsDto>> GetCredentialById(long id, string clientid)
        {
            try
            {

                var business = await _businessRepository.GetByIdAsync(id);
                if (business == null) return ApiResult<BusinessCredentialsDto>.NotFoundResult($"Business with id: {id} not found");

                var credential = await _businessCredentialRepository.FirstOrDefaultAsync(c => c.BusinessId == id && c.ClientId == clientid);
                if (credential == null)
                {
                    return ApiResult<BusinessCredentialsDto>.NotFoundResult("Credential not found");
                }

                var credentialDto = _mapper.Map<BusinessCredentialsDto>(credential);
                credentialDto.ClientSecret = EncryptionHelper.DecryptSecret(credentialDto.ClientSecret, clientid);

                return ApiResult<BusinessCredentialsDto>.SuccessResult(credentialDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}: {message}", nameof(GetCredentials), ex.Message);

                throw;
            }
        }

        public async Task<ApiResult<BusinessCredentialsDto>> EnableCredential(long id, string clientid, bool status)
        {
            try
            {
                await _businessCredentialRepository.BeginTransactionAsync();

                var business = await _businessRepository.GetByIdAsync(id);
                if (business == null) return ApiResult<BusinessCredentialsDto>.NotFoundResult($"Business with id: {id} not found");

                var credential = await _businessCredentialRepository.FirstOrDefaultAsync(c => c.BusinessId == id && c.ClientId == clientid);
                if (credential == null)
                {
                    return ApiResult<BusinessCredentialsDto>.NotFoundResult("Credential not found");
                }

                credential.IsActive = status;
                credential.UpdatedAt = DateTime.UtcNow;


                var clientApplication = await _applicationManager.FindByClientIdAsync(clientid);
                if (clientApplication == null)
                {
                    throw new Exception("Application not found");
                }

                clientApplication.IsActive = status;
                await _applicationManager.UpdateAsync(clientApplication);

                var credentialDto = _mapper.Map<BusinessCredentialsDto>(credential);
                credentialDto.ClientSecret = EncryptionHelper.DecryptSecret(credentialDto.ClientSecret, clientid);
                _businessCredentialRepository.Update(credential);

                await _businessCredentialRepository.CommitTransactionAsync();
                return ApiResult<BusinessCredentialsDto>.SuccessResult(credentialDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}: {message}", nameof(GetCredentials), ex.Message);
                _businessRepository.RollbackTransaction();

                throw;
            }
        }
    }
}
