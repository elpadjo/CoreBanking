using AutoMapper;
using CoreBanking.API.Models.Requests;
using CoreBanking.Application.User.Commands.CreateUser;
using CoreBanking.Application.User.Commands.LoginUser;
using CoreBanking.Application.User.Commands.UpdateUsername;
using CoreBanking.Application.User.Commands.UpdatePassword;
using CoreBanking.Application.User.Commands.UpdateRole;
using CoreBanking.Application.User.Commands.ActivateUser;
using CoreBanking.Application.User.Commands.DeactivateUser;
using CoreBanking.Application.User.Commands.DeleteUser;
using CoreBanking.Application.User.Queries.GetUserById;
using CoreBanking.Application.User.Queries.GetUserByUsername;
using CoreBanking.Application.User.Queries.GetUserByEmail;
using CoreBanking.Application.User.Queries.GetUsersByRole;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.API.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // Request to Command mappings
        CreateMap<CreateUserRequest, CreateUserCommand>();

        CreateMap<LoginUserRequest, LoginUserCommand>();

        CreateMap<UpdateUsernameRequest, UpdateUsernameCommand>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore()); // UserId will be set from route parameter

        CreateMap<UpdatePasswordRequest, UpdatePasswordCommand>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore()); // UserId will be set from route parameter

        CreateMap<UpdateRoleRequest, UpdateRoleCommand>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore()); // UserId will be set from route parameter

        CreateMap<ActivateUserRequest, ActivateUserCommand>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore()); // UserId will be set from route parameter

        CreateMap<DeactivateUserRequest, DeactivateUserCommand>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore()); // UserId will be set from route parameter

        CreateMap<DeleteUserRequest, DeleteUserCommand>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore()); // UserId will be set from route parameter

        // Request to Query mappings
        CreateMap<GetUserByIdRequest, GetUserByIdQuery>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => UserId.Create(src.UserId)));

        CreateMap<GetUserByUsernameRequest, GetUserByUsernameQuery>();

        CreateMap<GetUserByEmailRequest, GetUserByEmailQuery>();

        CreateMap<GetUsersByRoleRequest, GetAllUserQueryRole>()
            .ForMember(dest => dest.pageNumber, opt => opt.MapFrom(src => src.PageNumber))
            .ForMember(dest => dest.pageSize, opt => opt.MapFrom(src => src.PageSize));
    }
}
