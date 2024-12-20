using AutoMapper;
using Web.Data;
using Web.Models;

namespace Web.Helpers
{
    public class AutoMap:Profile
    {
        public AutoMap() { CreateMap<Room, Model_Room>().ReverseMap(); }
    }
}
