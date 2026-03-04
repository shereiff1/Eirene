using AutoMapper;
using Eirene.BLL.Models.Tracking;
using Eirene.DAL.Entities.Tracking;
namespace Eirene.BLL.Mappers
{
    internal class JournalProfile : Profile
    {
        public JournalProfile()
        {
            CreateMap<AddJournal, Journal>();
            CreateMap<EditJournal, Journal>();
            CreateMap<Journal, JournalDTO>();
        }
    }
}
