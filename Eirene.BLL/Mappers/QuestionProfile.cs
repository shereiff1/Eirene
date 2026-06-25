
using AutoMapper;
using Eirene.BLL.ModelVMs.Treatment;
using Eirene.DAL.Entities.Treatment;

namespace Eirene.BLL.Mappers;

internal class QuestionProfile : Profile
{
    public QuestionProfile()
    {
        CreateMap<AddQuestion, Question>()
            .ForMember(dest => dest.Choices, opt => opt.Ignore());
        CreateMap<EditQuestion, Question>()
            .ForMember(dest => dest.Choices, opt => opt.Ignore());
        CreateMap<Question, QuestionDTO>().ReverseMap();
        CreateMap<QuestionChoice, QuestionChoiceDTO>().ReverseMap();
        CreateMap<AddQuestionChoiceItem, QuestionChoice>();
    }
}
