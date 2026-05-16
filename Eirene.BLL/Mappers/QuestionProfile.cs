
using AutoMapper;
using Eirene.BLL.ModelVMs.Treatment;
using Eirene.DAL.Entities.Treatment;

namespace Eirene.BLL.Mappers;

internal class QuestionProfile : Profile
{
    public QuestionProfile()
    {
        CreateMap<AddQuestion, Question>().ReverseMap();
        CreateMap<EditQuestion, Question>().ReverseMap();
        CreateMap<Question, QuestionDTO>().ReverseMap();
    }
}
