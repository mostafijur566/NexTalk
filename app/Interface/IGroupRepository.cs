using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using app.Dtos.Group;

namespace app.Interface
{
    public interface IGroupRepository
    {
        Task<GroupResponseDto> CreateGroupAsync(CreateGroupDto dto, Guid creatorId);
        Task<List<GroupResponseDto>> GetMyGroupsAsync(Guid userId);
        Task<List<GroupMemberResponseDto>> GetGroupMembersAsync(Guid groupId);
        Task<bool> AddMemberAsync(Guid groupId, AddMemberDto dto, Guid requesterId);
        Task<bool> RemoveMemberAsync(Guid groupId, Guid userId, Guid requesterId);
    }
}