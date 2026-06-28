using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Eirene.BLL.Models.Common;
using Eirene.BLL.Models.Community.Membership;
using Eirene.BLL.Services.Abstraction.Core;
using Eirene.DAL.Entities.Community;
using Eirene.DAL.Entities.Core;
using Eirene.DAL.Repository.Abstraction;
using Eirene.DAL.Repository.Abstraction.Community;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Eirene.BLL.Services.Implementation.Core
{
    public class CommunityModerationService : ICommunityModerationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICommunityGroupRepository _communityGroupRepository;
        private readonly IUserCommunityGroupRepository _userCommunityGroupRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CommunityModerationService> _logger;
        private readonly IMapper _mapper;

        public CommunityModerationService(
            UserManager<ApplicationUser> userManager,
            ICommunityGroupRepository communityGroupRepository,
            IUserCommunityGroupRepository userCommunityGroupRepository,
            IUnitOfWork unitOfWork,
            ILogger<CommunityModerationService> logger,
            IMapper mapper)
        {
            _userManager = userManager;
            _communityGroupRepository = communityGroupRepository;
            _userCommunityGroupRepository = userCommunityGroupRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Result> ManageCommunityGroupMembershipAsync(Guid groupId, string userId, bool assign)
        {
            try
            {
                var group = await _communityGroupRepository.GetByIdAsync(groupId);
                if (group == null)
                {
                    _logger.LogWarning("Attempted to manage membership for non-existent group {GroupId}.", groupId);
                    return Result.Failure("Community group not found.");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Attempted to manage membership for non-existent user {UserId}.", userId);
                    return Result.Failure("User not found.");
                }

                var membership = await _userCommunityGroupRepository.GetByGroupAndUserAsync(groupId, userId);

                if (assign && membership != null && membership.IsBanned)
                {
                    _logger.LogWarning("Banned user {UserId} attempted to rejoin group {GroupId}.", userId, groupId);
                    return Result.Failure("You are banned from this community group and cannot rejoin.");
                }

                if (assign && membership == null)
                {
                    await _userCommunityGroupRepository.AddAsync(new UserCommunityGroup
                    {
                        CommunityGroupId = groupId,
                        UserId = userId
                    });

                    group.MemberCount++;
                    _logger.LogInformation("Added user {UserId} to group {GroupId}.", userId, groupId);
                }
                else if (!assign && membership != null)
                {
                    await _userCommunityGroupRepository.DeleteAsync(membership);
                    if (group.MemberCount > 0)
                    {
                        group.MemberCount--;
                    }

                    _logger.LogInformation("Removed user {UserId} from group {GroupId}.", userId, groupId);
                }
                else
                {
                    return Result.Success();
                }

                await _communityGroupRepository.UpdateAsync(group);
                var unitResult = await _unitOfWork.SaveChangesAsync();
                return unitResult > 0 ? Result.Success() : Result.Failure("Failed to save membership changes.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while managing membership for user {UserId} in group {GroupId}. Assign: {Assign}", userId, groupId, assign);
                return Result.Failure("An error occurred while managing group membership.");
            }
        }

        public async Task<Result> BanUserFromGroupAsync(Guid groupId, string userId)
        {
            try
            {
                var membership = await _userCommunityGroupRepository.GetByGroupAndUserAsync(groupId, userId);
                if (membership == null)
                {
                    _logger.LogWarning("Cannot ban user {UserId} from group {GroupId}: membership not found.", userId, groupId);
                    return Result.Failure("User is not a member of this community group.");
                }
                if (membership.IsBanned)
                {
                    return Result.Failure("User is already banned from this community group.");
                }

                membership.Ban();

                await _userCommunityGroupRepository.UpdateAsync(membership);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User {UserId} was banned from group {GroupId}.", userId, groupId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while banning user {UserId} from group {GroupId}.", userId, groupId);
                return Result.Failure("An error occurred while banning the user.");
            }
        }

        public async Task<Result> UnbanUserFromGroupAsync(Guid groupId, string userId)
        {
            try
            {
                var membership = await _userCommunityGroupRepository.GetByGroupAndUserAsync(groupId, userId);
                if (membership == null)
                {
                    _logger.LogWarning("Cannot unban user {UserId} from group {GroupId}: membership not found.", userId, groupId);
                    return Result.Failure("User is not a member of this community group.");
                }

                if (!membership.IsBanned)
                {
                    return Result.Failure("User is not banned from this community group.");
                }

                membership.Unban();

                await _userCommunityGroupRepository.UpdateAsync(membership);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User {UserId} was unbanned from group {GroupId}.", userId, groupId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while unbanning user {UserId} from group {GroupId}.", userId, groupId);
                return Result.Failure("An error occurred while removing the ban.");
            }
        }

        public async Task<Result> TimeoutUserInGroupAsync(Guid groupId, string userId, DateTime timeoutUntil)
        {
            try
            {
                if (timeoutUntil <= DateTime.UtcNow)
                {
                    return Result.Failure("Timeout end date must be in the future.");
                }

                var membership = await _userCommunityGroupRepository.GetByGroupAndUserAsync(groupId, userId);
                if (membership == null)
                {
                    _logger.LogWarning("Cannot timeout user {UserId} in group {GroupId}: membership not found.", userId, groupId);
                    return Result.Failure("User is not a member of this community group.");
                }

                membership.Timeout(timeoutUntil);

                await _userCommunityGroupRepository.UpdateAsync(membership);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User {UserId} was timed out in group {GroupId} until {TimeoutUntil}.", userId, groupId, membership.TimeoutUntil);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while applying a timeout to user {UserId} in group {GroupId}.", userId, groupId);
                return Result.Failure("An error occurred while applying the timeout.");
            }
        }

        public async Task<Result> RemoveTimeoutUserInGroupAsync(Guid groupId, string userId)
        {
            try
            {
                var membership = await _userCommunityGroupRepository.GetByGroupAndUserAsync(groupId, userId);
                if (membership == null)
                {
                    _logger.LogWarning("Cannot remove timeout for user {UserId} in group {GroupId}: membership not found.", userId, groupId);
                    return Result.Failure("User is not a member of this community group.");
                }

                if (!membership.TimeoutUntil.HasValue)
                {
                    return Result.Failure("User does not have an active timeout in this community group.");
                }

                membership.RemoveTimeout();

                await _userCommunityGroupRepository.UpdateAsync(membership);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Timeout removed for user {UserId} in group {GroupId}.", userId, groupId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing timeout for user {UserId} in group {GroupId}.", userId, groupId);
                return Result.Failure("An error occurred while removing the timeout.");
            }
        }

        public async Task<Result<List<CommunityGroupMembershipDTO>>> GetBannedUsersByGroupAsync(Guid groupId)
        {
            try
            {
                var bannedMemberships = await _userCommunityGroupRepository.GetBannedUsersByGroupAsync(groupId);
                var dtos = _mapper.Map<List<CommunityGroupMembershipDTO>>(bannedMemberships);
                return Result.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching banned users for group {GroupId}", groupId);
                return Result.Failure<List<CommunityGroupMembershipDTO>>("An error occurred while fetching banned users.");
            }
        }

        public async Task<Result<List<CommunityGroupMembershipDTO>>> GetTimedOutUsersByGroupAsync(Guid groupId)
        {
            try
            {
                var timedOutMemberships = await _userCommunityGroupRepository.GetTimedOutUsersByGroupAsync(groupId);
                var dtos = _mapper.Map<List<CommunityGroupMembershipDTO>>(timedOutMemberships);
                return Result.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching timed out users for group {GroupId}", groupId);
                return Result.Failure<List<CommunityGroupMembershipDTO>>("An error occurred while fetching timed out users.");
            }
        }
    }
}
