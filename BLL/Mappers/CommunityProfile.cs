using AutoMapper;
using BLL.Models.Community.Comment;
using BLL.Models.Community.Group;
using BLL.Models.Community.Post;
using BLL.Models.Identity;
using DAL.Entities.Community;
using DAL.Entities.Core;


namespace BLL.Mappers
{
    public class CommunityProfile : Profile
    {
        public CommunityProfile()
        {
            // CommunityGroup Mappings
            CreateMap<CommunityGroup, CommunityGroupDTO>()
                .ForMember(dest => dest.CreatedByUserName,
                    opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.FullName : string.Empty))
                .ForMember(dest => dest.PostsCount,
                    opt => opt.MapFrom(src => src.Posts != null ? src.Posts.Count : 0));

            CreateMap<AddCommunityGroup, CommunityGroup>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Posts, opt => opt.Ignore());

            CreateMap<EditCommunityGroup, CommunityGroup>()
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Posts, opt => opt.Ignore());

            CreateMap<CommunityPost, CommunityPostDTO>()
                 .ForMember(dest => dest.UserName,
                     opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                 .ForMember(dest => dest.CommunityGroupName,
                     opt => opt.MapFrom(src => src.CommunityGroup != null ? src.CommunityGroup.Name : string.Empty))
                 .ForMember(dest => dest.Comments,
                     opt => opt.MapFrom(src => src.Comments
                         .Where(c => !c.IsDeleted)
                         .OrderBy(c => c.PostedOn)))
                 .ForMember(dest => dest.CommentsCount,
                     opt => opt.MapFrom(src => src.Comments.Count(c => !c.IsDeleted)));


            CreateMap<AddCommunityPost, CommunityPost>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PostedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsEdited, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CommentsCount, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.CommunityGroup, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore());

            CreateMap<EditCommunityPost, CommunityPost>()
                .ForMember(dest => dest.CommunityGroupId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.PostedOn, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsEdited, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CommentsCount, opt => opt.Ignore())
                .ForMember(dest => dest.CommunityGroup, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore());

            // CommunityComment Mappings
            CreateMap<CommunityComment, CommunityCommentDTO>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                .ForMember(dest => dest.Replies,
                    opt => opt.MapFrom(src => src.Replies != null && src.Replies.Any()
                        ? src.Replies.Where(r => !r.IsDeleted).ToList()
                        : new List<CommunityComment>()));

            CreateMap<AddCommunityComment, CommunityComment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PostedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsEdited, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.RepliesCount, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.Post, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.ParentComment, opt => opt.Ignore())
                .ForMember(dest => dest.Replies, opt => opt.Ignore());

            CreateMap<EditCommunityComment, CommunityComment>()
                .ForMember(dest => dest.PostId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.PostedOn, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsEdited, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCommentId, opt => opt.Ignore())
                .ForMember(dest => dest.LikesCount, opt => opt.Ignore())
                .ForMember(dest => dest.RepliesCount, opt => opt.Ignore())
                .ForMember(dest => dest.Post, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.ParentComment, opt => opt.Ignore())
                .ForMember(dest => dest.Replies, opt => opt.Ignore());

            // CommunityGroup to CommunityGroupWithDetails
            CreateMap<CommunityGroup, CommunityGroupWithDetails>()
                .ForMember(dest => dest.CreatedOn,
                    opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.Posts,
                    opt => opt.MapFrom(src => src.Posts))
                .ForMember(dest => dest.Members,
                    opt => opt.MapFrom(src => src.Members))
                .ForMember(dest => dest.PostsCount,
                    opt => opt.MapFrom(src => src.Posts != null ? src.Posts.Count : 0));

            // ApplicationUser to UserDTO
            CreateMap<ApplicationUser, UserDTO>()
                .ForMember(dest => dest.Role,
                    opt => opt.Ignore()); 
        }
    }
}