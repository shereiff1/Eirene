
using AutoMapper;
using BLL.ModelVMs.Treatment;
using DAL.Entities.Treatment;

namespace BLL.Mappers;

internal class QuestionProfile : Profile
{
    public QuestionProfile()
    {
        CreateMap<AddQuestion, Question>().ReverseMap();
        CreateMap<EditQuestion, Question>().ReverseMap();
        CreateMap<Question, QuestionDTO>().ReverseMap();
    }
}
