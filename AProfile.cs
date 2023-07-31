
using AutoMapper;
using latest.Models;
using latest.Models.Dtos;
using latest.Models.Enums;

class AProfile : Profile {
    public AProfile() {
        CreateMap<CareGiverDto,  CareGiver>()
        .ForMember(c => c.UserName, opt => opt.MapFrom(c => c.Email));
        CreateMap<CourseDto, Course>();
        CreateMap<Course, CourseDto>();
        // .ForPath(c => c.Course.Name, opt => opt.MapFrom(c => c.Course))
        // .ForPath(c => c.Course.Passed, opt => opt.MapFrom(c => c.Passed));
        CreateMap<CareGiver, CareGiverProfileDto>();
        CreateMap<CareGiver, CareGiverChooseDto>()
        .ForMember(c => c.HasImage, opt => opt.MapFrom(c => c.ProfileImage != null))
        .ForMember(c => c.Age, opt => opt.MapFrom(c => (DateTime.MinValue + (DateTime.Now - c.DateOfBirth)).Year-1))
        .ForMember(c => c.AverageStars, opt => opt.Ignore());
        CreateMap<CareConsumerDto, CareConsumer>().ForMember(c => c.UserName, opt => opt.MapFrom(c => c.Email));
        CreateMap<CareRequestDto, CareRequest>().ForMember(c => c.Interval, opt => opt.MapFrom(c => Enum.Parse<Interval>(c.Interval)));
        CreateMap<CareRequest, CareRequestDto>()
        .ForMember(c => c.Interval, opt => opt.MapFrom(c => c.Interval.ToString()));
        // .ForMember(c => c.Appointment, opt => opt.Ignore());
        // .ForMember(c => c.Appointment, opt => opt.MapFrom(c => c.Appointments.FirstOrDefault()));
        CreateMap<CareRequest, CareRequestOneAppointmentDto>()
        .ForMember(c => c.Appointment, opt => opt.Ignore());
        CreateMap<CareUser, ChatMessageCareUserDto>().ForMember(cmcud => cmcud.HasImage, opt => opt.MapFrom(cu => cu.ProfileImage != null));
        CreateMap<ChatMessage, ChatMessageDto>()
        .ForMember(c => c.AmISender, opt => opt.Ignore())
        .ForMember(c => c.CareRequest, opt => opt.MapFrom(c => c.CareRequest))
        .ForMember(c => c.ChatMessageButtonEnum, opt => opt.MapFrom(c => c.ChatMessageButtonEnum.ToString()))
        .ForPath(c => c.CareRequest!.isCareConsumer, opt => opt.Ignore());
        // .ForMember(c => c.CareRequest!.Appointment, opt => opt.MapFrom())
        // .ForMember(c => c.CareRequest, opt => opt.MapFrom(src => src.CareRequest))
        // .ForMember(c => c.CareRequest, opt => opt.MapFrom(src => src.CareRequest ?? new CareRequest {
        //     Task = src.CareRequest!.Task,
        //     Description = src.CareRequest.Description,
        //     Interval = src.CareRequest.Interval,
        //     CareConsumerId = src.CareRequest.CareConsumerId,
        //     Appointments = src.CareRequest.Appointments        
        // }))
        // .ForPath(c => c.CareRequest!.isCareConsumer, opt => opt.Ignore())
        // .ForPath(c => c.c)
        // .ForPath(c =>  c.CareRequest!.Appointment, opt => opt.Ignore())
        // .ForMember(c => c.CareRequest!.Appointment, opt => opt.MapFrom(src => src.CareRequest!.Appointments.FirstOrDefault()));  
        //  c.CareRequest!.Appointments.FirstOrDefault()));
        CreateMap<ChatMessageCreateDto, ChatMessage>()
        .ForMember(c => c.SenderId, opt => opt.Ignore())
        .ForMember(c => c.CareRequest, opt => opt.Ignore());
        CreateMap<Appointment, AppointmentDto>()
        .ForMember(a => a.IsAccepted, opt => opt.MapFrom(a => a.PaymentRequest != null));
        // .ForMember(a => a.Index, opt => opt.MapFrom(a => a))
        CreateMap<PaymentRequest, PaymentRequestDto>();    
        CreateMap<CareGiver, CareUserDto>();    
        CreateMap<ReviewDto, Review>().ForMember(r => r.Stars, opt => opt.MapFrom(c => (StarEnum)c.Stars));
        CreateMap<Review, ReviewOutDto>();
    }
}