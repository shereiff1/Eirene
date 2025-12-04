using AutoMapper;
using BLL.Models.Tracking;
using DAL.Entities.Tracking;
namespace BLL.Mappers
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
