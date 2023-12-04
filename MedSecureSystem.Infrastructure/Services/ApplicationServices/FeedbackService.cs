using AutoMapper;
using MedSecureSystem.Application.Commons;
using MedSecureSystem.Application.Dtos;
using MedSecureSystem.Application.Features.Delivery;
using MedSecureSystem.Application.Interfaces;
using MedSecureSystem.Domain.Entities;
using MedSecureSystem.Domain.Enums;
using MedSecureSystem.Domain.Interfaces;
using MedSecureSystem.Shared.Attributes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedSecureSystem.Infrastructure.Services.ApplicationServices
{
    [InfrastructureServiceLifetime(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped)]
    public class FeedbackService : IFeedbackService
    {
        private readonly IGenericRepository<Feedback> _feedbackRepository;
        private readonly IGenericRepository<DeliveryRequest> _deliveryRequestRepository;
        private readonly IGenericRepository<Business> _businessRepository;
        private readonly IGenericRepository<DeliveryRequestItem> _deliveryRequestItemRepository;
        private readonly ILogger<DeliveryService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private IServiceProvider _serviceProvider;


        public FeedbackService(IGenericRepository<DeliveryRequest> deliveryRequestRepository, IGenericRepository<DeliveryRequestItem> deliveryRequestItemRepository, ILogger<DeliveryService> logger, IMapper mapper, UserManager<ApplicationUser> userManager, IEmailSender emailSender, IGenericRepository<Business> businessRepository, IServiceProvider serviceProvider, IGenericRepository<Feedback> feedbackRepository)
        {
            _deliveryRequestRepository = deliveryRequestRepository;
            _deliveryRequestItemRepository = deliveryRequestItemRepository;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _emailSender = emailSender;
            _businessRepository = businessRepository;
            _serviceProvider = serviceProvider;
            _feedbackRepository = feedbackRepository;
        }

        public async Task<ApiResult<FeedbackModel>> AddFeedbackAsync(CreateDeliveryRequestFeedbackModel model, string role, string user, string businessid)
        {
            var query = _deliveryRequestRepository
                            .Query()
                            .Include(dr => dr.Items)
                            .Include(u => u.Business)
                            .Include(dr => dr.Patient)
                            .Include(dr => dr.Driver)
                            .Include(dr => dr.Agent)
                                .Where(dr => dr.Id == model.DeliveryRequestId);

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
                return ApiResult<FeedbackModel>.NotFoundResult("Delivery not found");
            }

            UserTypes roleUser = UserTypes.Patient;
            switch (role)
            {
                case nameof(UserTypes.Patient): roleUser = UserTypes.Patient; break;
                case nameof(UserTypes.BusinessAdmin): roleUser = UserTypes.BusinessAdmin; break;
                case nameof(UserTypes.Driver): roleUser = UserTypes.Driver; break;
                case nameof(UserTypes.BusinessAgent): roleUser = UserTypes.BusinessAgent; break;
                default:
                    break;
            }

            var entity = new Feedback
            {
                CreatedAt = DateTime.UtcNow,
                DeliveryRequestId = model.DeliveryRequestId,
                UserId = user,
                Comments = model.Comments,
                UserType = roleUser
            };

            await _feedbackRepository.AddAsync(entity);
            await _feedbackRepository.SaveAsync();
            if (entity.Id > 0)
            {
                return ApiResult<FeedbackModel>.SuccessResult(_mapper.Map<FeedbackModel>(entity));
            }

            return ApiResult<FeedbackModel>.FailureResult(null, "Failed saving feedback");

        }

        public async Task<ApiResult<IEnumerable<FeedbackModel>>> GetFeedbackByDeliveryRequestIdAsync(long deliveryRequestId)
        {
            var result = await _feedbackRepository
                .Query()
                .Include(c => c.User)
                .Include(c => c.DeliveryRequest)
                .Where(f => f.DeliveryRequestId == deliveryRequestId)
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            return ApiResult<IEnumerable<FeedbackModel>>.SuccessResult(_mapper.Map<IEnumerable<FeedbackModel>>(result));
        }
    }
}
