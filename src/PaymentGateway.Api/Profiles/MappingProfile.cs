using AutoMapper;

using PaymentGateway.Api.ApiClient.Models.Request;
using PaymentGateway.Api.ApiClient.Models.Response;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Profiles;

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