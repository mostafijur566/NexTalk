using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using app.Data;
using app.Dtos.Group;
using app.Interface;
using app.Models;
using Microsoft.EntityFrameworkCore;

namespace app.Repository
{
    public class GroupRepository : IGroupRepository
    {
        private readonly ApplicationDbContext _context;

        public GroupRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GroupResponseDto> CreateGroupAsync(CreateGroupDto dto, Guid creatorId)
        {
            var group = new Group
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow
            };

            _context.Groups.Add(group);

            // creator is automatically admin
            var member = new GroupMember
            {
                GroupId = group.Id,
                UserId = creatorId,
                IsAdmin = true,
                JoinedAt = DateTime.UtcNow
            };

            _context.GroupMembers.Add(member);
            await _context.SaveChangesAsync();

            return new GroupResponseDto
            {
                Id = group.Id,
                Name = group.Name,
                MemberCount = 1,
                CreatedAt = group.CreatedAt
            };
        }

        public async Task<List<GroupResponseDto>> GetMyGroupsAsync(Guid userId)
        {
            return await _context.GroupMembers
                .Where(gm => gm.UserId == userId)
                .Include(gm => gm.Group)
                    .ThenInclude(g => g.Members)
                .Select(gm => new GroupResponseDto
                {
                    Id = gm.Group.Id,
                    Name = gm.Group.Name,
                    MemberCount = gm.Group.Members.Count,
                    CreatedAt = gm.Group.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<GroupMemberResponseDto>> GetGroupMembersAsync(Guid groupId)
        {
            return await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId)
                .Include(gm => gm.User)
                .Select(gm => new GroupMemberResponseDto
                {
                    UserId = gm.UserId,
                    Username = gm.User.Username,
                    Email = gm.User.Email,
                    IsAdmin = gm.IsAdmin,
                    JoinedAt = gm.JoinedAt
                })
                .ToListAsync();
        }

        public async Task<bool> AddMemberAsync(Guid groupId, AddMemberDto dto, Guid requesterId)
        {
            // only admin can add members
            var isAdmin = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == requesterId && gm.IsAdmin);

            if (!isAdmin)
                throw new Exception("Only admins can add members.");

            // check already a member
            var alreadyMember = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == dto.UserId);

            if (alreadyMember)
                throw new Exception("User is already a member.");

            _context.GroupMembers.Add(new GroupMember
            {
                GroupId = groupId,
                UserId = dto.UserId,
                IsAdmin = false,
                JoinedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveMemberAsync(Guid groupId, Guid userId, Guid requesterId)
        {
            // only admin can remove members
            var isAdmin = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == requesterId && gm.IsAdmin);

            if (!isAdmin)
                throw new Exception("Only admins can remove members.");

            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

            if (member == null)
                throw new Exception("Member not found.");

            _context.GroupMembers.Remove(member);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}