using AutoMapper;
using MedSecureSystem.Application.Dtos;
using MedSecureSystem.Application.Features.Businesses.Commands;
using MedSecureSystem.Application.Features.Delivery;
using MedSecureSystem.Domain.Entities;
using MedSecureSystem.Shared.Extensions;

namespace MedSecureSystem.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Business, BusinessDto>();
            CreateMap<Business, BusinessDetailsDto>()
                .ForMember(dest => dest.Admin, opt => opt.MapFrom(src => src.Email)); ;
            CreateMap<BusinessDto, Business>();
            CreateMap<CreateBusinessCommand, Business>();
            // Define other mappings here


            //business credentials
            CreateMap<BusinessCredential, BusinessCredentialsDto>();

            // Delivery
            CreateMap<DeliveryRequestItem, DeliveryRequestItemModel>();
            CreateMap<DeliveryRequestItemModel, DeliveryRequestItem>();

            CreateMap<DeliveryRequest, DeliveryDto>()
                //.IncludeMembers(d => d.Items)
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.GetEnumDescription()))
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => $"{src.Patient.FirstName} {src.Patient.LastName}"))
                .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business.Name))
                .ForMember(dest => dest.AgentName, opt => opt.MapFrom(src => src.Agent == null ? "" : $"{src.Agent.FirstName} {src.Agent.LastName}"))
                .ForMember(dest => dest.DriverName, opt => opt.MapFrom(src => src.Driver == null ? "" : $"{src.Driver.FirstName} {src.Driver.LastName}"));



            CreateMap<Feedback, FeedbackModel>()
               //.IncludeMembers(d => d.Items)
               .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.UserType.GetEnumDescription()))
               .ForMember(dest => dest.Commenter, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"));
        }
    }
}