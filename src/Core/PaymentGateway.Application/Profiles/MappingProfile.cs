using AutoMapper;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Domain.ValueObjects;
using PaymentGateway.Shared.Models.ApiClient.Request;
using PaymentGateway.Shared.Models.ApiClient.Response;

namespace PaymentGateway.Application.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<PostPaymentRequest, PostPaymentApiRequest>()
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
}