using AutoMapper;
using Web.Models.Class.User;
using Web.Models.ClassModel;

namespace Web.Helpers
{
    public class AutoMapper: Profile
    {
        public AutoMapper()
        {
            CreateMap<Usercs, User_Model>().ReverseMap();
        }
    }
}
