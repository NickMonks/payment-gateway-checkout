using AutoMapper;

using PaymentGateway.Domain.ValueObjects;
using PaymentGateway.Shared.Models.ApiClient.Request;
using PaymentGateway.Shared.Models.ApiClient.Response;
using PaymentGateway.Shared.Models.Controller.Requests;
using PaymentGateway.Shared.Models.Controller.Responses;
using PaymentGateway.Shared.Models.DTO;

namespace PaymentGateway.Application.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        ServiceMappingProfile();
        CreateMap<CreatePaymentRequestDto, PostPaymentApiRequest>()
            .ForMember(dest => dest.CardNumber, 
                opt => 
                    opt.MapFrom(src => src.CardNumber)) 
            .ForMember(dest => dest.ExpiryDate, 
                opt => 
                opt.MapFrom(src => $"{src.ExpiryMonth:D2}/{src.ExpiryYear:D4}"))
            .ForMember(dest => 
                dest.Cvv, opt => 
                opt.MapFrom(src => src.Cvv.ToString()));
        
        CreateMap<PostPaymentApiResponse, PaymentStatus>()
            .ConvertUsing(src => src.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined);
    }

    private void ServiceMappingProfile()
    {
        CreateMap<PostPaymentRequest, CreatePaymentRequestDto>();
        CreateMap<CreatePaymentRequestDto, PostPaymentRequest>();
        
        CreateMap<PostPaymentResponse, CreatePaymentResponseDto>();
        CreateMap<CreatePaymentResponseDto, PostPaymentResponse>();
    }
}