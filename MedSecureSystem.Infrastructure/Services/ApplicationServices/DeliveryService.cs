using AutoMapper;
using MedSecureSystem.Application.Commons;
using MedSecureSystem.Application.Dtos;
using MedSecureSystem.Application.Features.Delivery;
using MedSecureSystem.Application.Interfaces;
using MedSecureSystem.Domain.Entities;
using MedSecureSystem.Domain.Interfaces;
using MedSecureSystem.Shared.Attributes;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Azure.Core;
using MedSecureSystem.Domain.Enums;
using MedSecureSystem.Shared.Query;
using Microsoft.Extensions.DependencyInjection;
using MedSecureSystem.Infrastructure.Data;

namespace MedSecureSystem.Infrastructure.Services.ApplicationServices
{
    [InfrastructureServiceLifetime(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped)]
    public class DeliveryService : IDeliveryService
    {
        private readonly IGenericRepository<DeliveryRequest> _deliveryRequestRepository;
        private readonly IGenericRepository<Business> _businessRepository;
        private readonly IGenericRepository<DeliveryRequestItem> _deliveryRequestItemRepository;
        private readonly ILogger<DeliveryService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private IServiceProvider _serviceProvider;


        public DeliveryService(IGenericRepository<DeliveryRequest> deliveryRequestRepository, IGenericRepository<DeliveryRequestItem> deliveryRequestItemRepository, ILogger<DeliveryService> logger, IMapper mapper, UserManager<ApplicationUser> userManager, IEmailSender emailSender, IGenericRepository<Business> businessRepository, IServiceProvider serviceProvider)
        {
            _deliveryRequestRepository = deliveryRequestRepository;
            _deliveryRequestItemRepository = deliveryRequestItemRepository;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _emailSender = emailSender;
            _businessRepository = businessRepository;
            _serviceProvider = serviceProvider;
        }


        public async Task<ApiResult<DeliveryDto>> CreateDeliveryRequest(string patientId, string patientEmail, CreateDeliveryRequestModel request)
        {
            try
            {
                var business = await _businessRepository.GetByIdAsync(request.BusinessId);
                if (business == null) return ApiResult<DeliveryDto>.NotFoundResult($"Business with id: {request.BusinessId} not found");

                if (!business.IsActive) return ApiResult<DeliveryDto>.NotFoundResult($"Business with id: {request.BusinessId} not active");
                if (!business.IsVerified) return ApiResult<DeliveryDto>.NotFoundResult($"Business with id: {request.BusinessId} not verified");

                var deliveryRequest = new DeliveryRequest
                {
                    BusinessId = request.BusinessId,
                    PatientId = patientId,
                    Items = _mapper.Map<IEnumerable<DeliveryRequestItem>>(request.Items).ToList(),
                    CreatedAt = DateTime.UtcNow
                };

                await _deliveryRequestRepository.AddAsync(deliveryRequest);
                await _businessRepository.SaveAsync();

                if (deliveryRequest.Id > 0)
                {
                    //var templatePath = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("Templates", "BusinessDeliveryEmailTemplate.html"));
                    //var template = await File.ReadAllTextAsync(templatePath);

                    var businessAdmin = await _userManager.FindByEmailAsync(business.Email);

                    await _emailSender.SendEmailAsync(businessAdmin.Email, "MedSecure: New Delivery Request", CreateHtmlEmailContent(deliveryRequest, businessAdmin.FirstName));
                    await _emailSender.SendEmailAsync(patientEmail, "MedSecure: New Delivery Request", "Your Request has been created successfully");

                    return ApiResult<DeliveryDto>.SuccessResult(_mapper.Map<DeliveryDto>(deliveryRequest));
                }

                return ApiResult<DeliveryDto>.FailureResult(null, "Error: Request can not be created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}", nameof(CreateDeliveryRequest));
                throw;
            }
        }

        public string CreateHtmlEmailContent(DeliveryRequest deliveryRequest, string businessAdminName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html><head><style>");
            sb.AppendLine("body {font-family: Arial, sans-serif; line-height: 1.6;}");
            sb.AppendLine(".container {width: 80%; margin: auto; padding: 20px;}");
            sb.AppendLine("table {width: 100%; border-collapse: collapse;}");
            sb.AppendLine("th, td {border: 1px solid #ddd; padding: 8px; text-align: left;}");
            sb.AppendLine("th {background-color: #f2f2f2;}");
            sb.AppendLine("</style></head><body>");
            sb.AppendLine("</style></head><body>");
            sb.AppendLine("<div class='container'>");
            sb.AppendFormat("<p>Dear {0},</p>", businessAdminName);
            sb.AppendLine("<p>I hope this message finds you well. We are reaching out to provide you with a summary of the items needed for the upcoming delivery request. Below is the list of items requested:</p>");
            sb.AppendFormat("<p><strong>Request ID:</strong> {0}<br><strong>Date:</strong> {1}</p>", deliveryRequest.Id, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Item Name</th><th>Quantity</th><th>Note</th></tr>");

            foreach (var item in deliveryRequest.Items)
            {
                sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", item.Name, item.Quantity, item.Note);
            }
            sb.AppendLine("</table>");
            sb.AppendLine("<p>We kindly ask you to review and prepare these items as per the requirements. Please ensure that all items are ready for pickup by [Pickup Date/Time].</p>");
            sb.AppendLine("<p>If there are any issues or further clarifications needed, please do not hesitate to contact us. Your prompt attention to this request is greatly appreciated.</p>");
            sb.AppendLine("<p>Thank you for your cooperation and support.</p>");
            sb.AppendLine("</div></body></html>");

            return sb.ToString();
        }

        public async Task<ApiResult<DeliveryDto>> GetDeliveryRequest(string user, string role, string businessid, long id)
        {
            try
            {
                var query = _deliveryRequestRepository
                            .Query()
                            .Include(dr => dr.Items)
                            .Include(u => u.Business)
                            .Include(dr => dr.Patient)
                            .Include(dr => dr.Driver)
                            .Include(dr => dr.Agent)
                                .Where(dr => dr.Id == id);

                switch (role)
                {
                    case nameof(UserTypes.Patient):
                        query = query.Where(dr => dr.PatientId == user);
                        break;
                    case nameof(UserTypes.BusinessAgent):
                        query = query.Where(dr => dr.AgentId == user);
                        break;
                    case nameof(UserTypes.Driver):
                        query = query.Where(dr => dr.DriverId == user);
                        break;
                    case nameof(UserTypes.BusinessAdmin):
                        if (!string.IsNullOrWhiteSpace(businessid))
                        {
                            query = query.Where(dr => businessid.Equals(dr.BusinessId));
                        }
                        break;
                }

                var request = await query.FirstOrDefaultAsync();
                if (request == null)
                {
                    return ApiResult<DeliveryDto>.NotFoundResult("Delivery not found");
                }

                var data = _mapper.Map<DeliveryDto>(request);
                return ApiResult<DeliveryDto>.SuccessResult(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}", nameof(GetDeliveryRequest));
                throw;
            }
        }


        public async Task<ApiResult<PaginatedResult<DeliveryDto>>> GetDeliveryRequests(string user, string role, string businessid, PaginationQuery pagination)
        {
            try
            {

                using (var context = _serviceProvider.GetRequiredService<MedSecureContext>())
                {


                    var query = context.DeliveryRequests
                                .Include(dr => dr.Items)
                                .Include(u => u.Business)
                                .Include(dr => dr.Patient)
                                .Include(dr => dr.Driver)
                                .Include(dr => dr.Agent)
                                    .AsNoTracking();


                    switch (role)
                    {
                        case nameof(UserTypes.Patient):
                            query = query.Where(dr => dr.PatientId == user);
                            break;
                        case nameof(UserTypes.BusinessAgent):
                            query = query.Where(dr => dr.AgentId == user);
                            break;
                        case nameof(UserTypes.Driver):
                            query = query.Where(dr => dr.DriverId == user);
                            break;
                        case nameof(UserTypes.BusinessAdmin):
                            if (!string.IsNullOrWhiteSpace(businessid))
                            {
                                query = query.Where(dr => businessid.Equals(dr.BusinessId));
                            }
                            break;
                    }
                    var count = await query.CountAsync();

                    query = query.OrderByDescending(c => c.CreatedAt);
                    if (pagination.Page > 0 && pagination.PageSize > 0)
                    {
                        query = query.Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize);
                    }

                    var entities = await query.ToListAsync();

                    /*var entities = await query
                                        .OrderByDescending(c => c.CreatedAt)
                                        .Skip((pagination.Page - 1) * pagination.PageSize)
                                        .Take(pagination.PageSize)
                                        .ToListAsync();*/
                    var items = _mapper.Map<IEnumerable<DeliveryDto>>(entities);
                    var request = new PaginatedResult<DeliveryDto>(items, count, pagination.Page, pagination.PageSize);


                    return ApiResult<PaginatedResult<DeliveryDto>>.SuccessResult(request);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}", nameof(GetDeliveryRequest));
                throw;
            }
        }

        // Accept a delivery request by a business agent
        public async Task<ApiResult<bool>> AcceptRequestByAgent(long requestId, string agentId, string email, string businessId)
        {
            try
            {
                var deliveryRequest = await _deliveryRequestRepository.GetByIdAsync(requestId);
                var bsuiness = await _businessRepository.FirstOrDefaultAsync(c => c.BusinessUniqueKey == businessId);
                if (deliveryRequest != null && deliveryRequest.BusinessId == bsuiness.Id)
                {
                    if (deliveryRequest.Status != DeliveryRequestStatus.Requested)
                    {
                        return ApiResult<bool>.FailureResult(null, $"Delivery Request needs to be in REQUESTED state");
                    }
                    var code = GenerateRandomUniqueNumbers(1);

                    deliveryRequest.CodeToGiveToDriver = code;
                    deliveryRequest.Status = DeliveryRequestStatus.Preparing;
                    deliveryRequest.AgentId = agentId;
                    deliveryRequest.AgentAcceptedTime = DateTime.UtcNow;
                    deliveryRequest.UpdatedAt = DateTime.UtcNow;
                    _deliveryRequestRepository.Update(deliveryRequest);
                    await _deliveryRequestRepository.SaveAsync();
                    await _emailSender.SendEmailAsync(email, "MedSecure: Agent Request confirmed", $"Request preparation comfirmed, you this code below to complete the request\ncode: {code}");
                    return ApiResult<bool>.SuccessResult(true, $"Request granted to prepare delivery");
                }

                return ApiResult<bool>.NotFoundResult($"Delivery Request with id: {requestId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}", nameof(StartDeliveryByDriver));
                throw;
            }
        }
        // Complete the preparation of a delivery request by a business agent
        public async Task<ApiResult<bool>> CompletePreparationByAgent(long requestId, string agentId, string code, string businessId)
        {
            try
            {
                var deliveryRequest = await _deliveryRequestRepository.GetByIdAsync(requestId);
                var bsuiness = await _businessRepository.FirstOrDefaultAsync(c => c.BusinessUniqueKey == businessId);

                if (deliveryRequest != null && deliveryRequest.BusinessId == bsuiness.Id && deliveryRequest.AgentId == agentId)
                {
                    if (deliveryRequest.Status == DeliveryRequestStatus.Preparing)
                    {
                        if (deliveryRequest.CodeToGiveToDriver == code)
                        {
                            deliveryRequest.Status = DeliveryRequestStatus.ReadyForPickup;
                            deliveryRequest.AgentCompletedTime = DateTime.UtcNow;
                            deliveryRequest.UpdatedAt = DateTime.UtcNow;

                            _deliveryRequestRepository.Update(deliveryRequest);
                            await _deliveryRequestRepository.SaveAsync();

                            return ApiResult<bool>.SuccessResult(true, $"Delivery Request prepared, awaiting driver pickup");
                        }
                        return ApiResult<bool>.FailureResult(null, $"Code is wrong");

                    }

                    return ApiResult<bool>.FailureResult(null, $"Delivery Request needs to be in preparing state");
                }

                return ApiResult<bool>.NotFoundResult($"Delivery Request with id: {requestId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}", nameof(CompletePreparationByAgent));
                throw;
            }
        }

        // Assign a driver to a delivery request
        public async Task<ApiResult<bool>> AssignDriver(long requestId, string email, string driverId)
        {
            try
            {
                var deliveryRequest = await _deliveryRequestRepository.GetByIdAsync(requestId);
                if (deliveryRequest != null)
                {
                    if (deliveryRequest.Status == DeliveryRequestStatus.ReadyForPickup)
                    {
                        var code = GenerateRandomUniqueNumbers(1);

                        deliveryRequest.CodeToConfirmDelivery = code;
                        deliveryRequest.Status = DeliveryRequestStatus.OnWayForDelivery;
                        deliveryRequest.DriverId = driverId;
                        deliveryRequest.DriverAcceptedTime = DateTime.UtcNow;
                        deliveryRequest.UpdatedAt = DateTime.UtcNow;


                        _deliveryRequestRepository.Update(deliveryRequest);
                        await _deliveryRequestRepository.SaveAsync();

                        await _emailSender.SendEmailAsync(email, "MedSecure: Driver Request confirmed", $"Request acceptance comfirmed, you this code below to complete the request\ncode: {code}");

                        return ApiResult<bool>.SuccessResult(true, $"Delivery Request prepared, awaiting driver pickup");

                    }
                    return ApiResult<bool>.FailureResult(null, $"Delivery Request needs to be in ReadyForPickup state");
                }

                return ApiResult<bool>.NotFoundResult($"Delivery Request with id: {requestId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}", nameof(AssignDriver));
                throw;
            }
        }

        // Mark the delivery as started by a driver
        public async Task<ApiResult<bool>> StartDeliveryByDriver(long requestId, string driverId)
        {
            try
            {
                var deliveryRequest = await _deliveryRequestRepository.GetByIdAsync(requestId);
                if (deliveryRequest != null && deliveryRequest.DriverId == driverId)
                {
                    if (deliveryRequest.Status == DeliveryRequestStatus.OnWayForDelivery)
                    {
                        var code = !string.IsNullOrWhiteSpace(deliveryRequest.CodeToConfirmReception) ? deliveryRequest.CodeToConfirmReception : GenerateRandomUniqueNumbers(1);

                        deliveryRequest.Status = DeliveryRequestStatus.OnWayForDelivery;
                        deliveryRequest.CodeToConfirmReception = code;
                        deliveryRequest.DriverStartedDeliveryTime = DateTime.UtcNow;
                        deliveryRequest.UpdatedAt = DateTime.UtcNow;

                        _deliveryRequestRepository.Update(deliveryRequest);
                        await _deliveryRequestRepository.SaveAsync();

                        var patient = await _userManager.FindByIdAsync(deliveryRequest.PatientId);
                        await _emailSender.SendEmailAsync(patient.Email, "MedSecure: Delivery on the way", $"Your delivery request is on it's way, use this code below to complete the request to prove reception\ncode: {code}");

                        return ApiResult<bool>.SuccessResult(true, $"Driver has started the delivery");
                    }
                    return ApiResult<bool>.FailureResult(null, "Error: Delivery can not be started");
                }
                return ApiResult<bool>.NotFoundResult($"Delivery Request with id: {requestId} not found");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}", nameof(StartDeliveryByDriver));
                throw;
            }
        }

        public async Task<ApiResult<bool>> ConfirmDelivery(long requestId, string driverId, string deliveryCode)
        {
            try
            {
                var deliveryRequest = await _deliveryRequestRepository.GetByIdAsync(requestId);
                if (deliveryRequest != null && deliveryRequest.DriverId == driverId)
                {
                    if (deliveryRequest.CodeToConfirmDelivery != deliveryCode)
                    {
                        return ApiResult<bool>.NotFoundResult($"Delivery confirmation failed, wrong code");
                    }

                    if (deliveryRequest.Status != DeliveryRequestStatus.OnWayForDelivery)
                    {
                        return ApiResult<bool>.FailureResult(null, $"To confirm delivery the status must be OnWayForDelivery");
                    }

                    deliveryRequest.Status = DeliveryRequestStatus.DriverCompletedDelivery;
                    deliveryRequest.DriverCompletedTime = DateTime.UtcNow;
                    deliveryRequest.UpdatedAt = DateTime.UtcNow;

                    _deliveryRequestRepository.Update(deliveryRequest);
                    await _deliveryRequestRepository.SaveAsync();

                    return ApiResult<bool>.SuccessResult(true, $"Delivery confirmed");
                }

                return ApiResult<bool>.NotFoundResult($"Delivery Request with id: {requestId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}", nameof(ConfirmDelivery));
                throw;
            }
        }

        public async Task<ApiResult<bool>> ConfirmPatientDelivery(long requestId, string patientid, string deliveryCode)
        {
            try
            {
                var deliveryRequest = await _deliveryRequestRepository.GetByIdAsync(requestId);
                if (deliveryRequest != null && deliveryRequest.PatientId == patientid)
                {
                    if (deliveryRequest.CodeToConfirmReception != deliveryCode)
                    {
                        return ApiResult<bool>.NotFoundResult($"Delivery confirmation failed, wrong code");
                    }

                    if (deliveryRequest.Status != DeliveryRequestStatus.DriverCompletedDelivery)
                    {
                        return ApiResult<bool>.FailureResult(null, $"To confirm delivery the status must be OnWayForDelivery");
                    }

                    deliveryRequest.Status = DeliveryRequestStatus.Delivered;
                    deliveryRequest.DriverCompletedTime = DateTime.UtcNow;
                    deliveryRequest.UpdatedAt = DateTime.UtcNow;

                    _deliveryRequestRepository.Update(deliveryRequest);
                    await _deliveryRequestRepository.SaveAsync();

                    var patient = await _userManager.FindByIdAsync(deliveryRequest.PatientId);
                    await _emailSender.SendEmailAsync(patient.Email, "MedSecure: Patient Delivery Completed", $"Cool your delivery of Id: {deliveryRequest.Id} is completed");

                    var agent = await _userManager.FindByIdAsync(deliveryRequest.AgentId);
                    await _emailSender.SendEmailAsync(agent.Email, "MedSecure: Agent Delivery Completed", $"Cool your delivery of Id: {deliveryRequest.Id} is completed");


                    return ApiResult<bool>.SuccessResult(true, $"Delivery confirmed");
                }

                return ApiResult<bool>.NotFoundResult($"Delivery Request with id: {requestId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}", nameof(ConfirmDelivery));
                throw;
            }
        }

        public async Task<ApiResult<bool>> CancelRequestAsync(long requestId, string patientid)
        {
            try
            {
                var deliveryRequest = await _deliveryRequestRepository.GetByIdAsync(requestId);
                if (deliveryRequest != null && deliveryRequest.PatientId == patientid)
                {
                    if ((deliveryRequest.Status == DeliveryRequestStatus.Requested
                    || deliveryRequest.Status == DeliveryRequestStatus.Preparing
                    || deliveryRequest.Status == DeliveryRequestStatus.ReadyForPickup))
                    {
                        return ApiResult<bool>.FailureResult(null, $"Sorry you can't cancel the request");
                    }

                    deliveryRequest.UpdatedAt = DateTime.UtcNow;


                    deliveryRequest.Status = DeliveryRequestStatus.Canceled; // Assuming 'Cancelled' is a valid status
                    _deliveryRequestRepository.Update(deliveryRequest);
                    await _deliveryRequestRepository.SaveAsync();

                    return ApiResult<bool>.SuccessResult(true, $"Delivery request cancelled");
                }


                return ApiResult<bool>.NotFoundResult($"Delivery Request with id: {requestId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{method}", nameof(ConfirmDelivery));
                throw;
            }
        }

        private readonly Random _random = new Random();

        public string GenerateRandomUniqueNumbers(int count)
        {
            var uniqueNumbers = new HashSet<string>();
            while (uniqueNumbers.Count < count)
            {
                int number = _random.Next(100000, 1000000); // 6-digit number
                uniqueNumbers.Add(number.ToString("D6"));
            }
            return string.Join(",", uniqueNumbers); // Join all numbers into a single string
        }

    }
}
