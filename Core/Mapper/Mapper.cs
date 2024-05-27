using Core.DTO.AdminComment;
using Core.DTO.ChatSession;
using Core.DTO.Message;
using Core.DTO.Role;
using Core.DTO.User;
using Core.Entities;
using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Mapper
{
    public class Mapper : AutoMapper.Profile
    {
        public Mapper()
        {
            CreateMap<UserDTO, UserEntity>().ReverseMap();
            CreateMap<UserEntity, UserDTO>().ReverseMap();
            CreateMap<CreateUserDTO, UserEntity>().ReverseMap();
            CreateMap<UserEntity, CreateUserDTO>().ReverseMap();
            CreateMap<UpdateUserDTO, UserEntity>().ReverseMap();
            CreateMap<UserEntity, UpdateUserDTO>().ReverseMap();
            CreateMap<UserRoleEntity, UserRoleDTO>();
            CreateMap<RoleEntity, RoleDTO>().ReverseMap();

            CreateMap<Message, MessageDTO>().ReverseMap();
            CreateMap<Message, CreateMessageDTO>().ReverseMap();
            CreateMap<MessageDTO, Message>().ReverseMap();
            CreateMap<ChatSession, ChatSessionDTO>().ReverseMap();
            CreateMap<ChatSession, CreateChatSessionDTO>().ReverseMap();
            CreateMap<ChatSessionDTO, ChatSession>().ReverseMap();

            CreateMap<AdminComment, AdminCommentDTO>();
            CreateMap<CreateAdminCommentDTO, AdminComment>();
        }
    }
}
