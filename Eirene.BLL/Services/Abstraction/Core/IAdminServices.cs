using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eirene.BLL.Models.Community.Membership;
using Eirene.BLL.Models.Core.Admin;
using Eirene.BLL.Models.Core.Doctor;

namespace Eirene.BLL.Services.Abstraction.Core
{
    public interface IAdminServices
    {
        Task<(bool IsSuccess, List<AdminModel>? Admins)> GetAllAsync();
        Task<(bool IsSuccess, AdminModel? Admin)> GetByIdAsync(string adminId);
        Task<(bool IsSuccess, string? Error, AdminModel? Admin)> CreateAdminProfileAsync(string userId);
        Task<bool> AssignRoleAsync(string adminId, string userId, string role);
        Task<bool> ManageCommunityGroupMembershipAsync(Guid groupId, string userId, bool assign);
        Task<(bool IsSuccess, string Message)> BanUserFromGroupAsync(Guid groupId, string userId);
        Task<(bool IsSuccess, string Message)> UnbanUserFromGroupAsync(Guid groupId, string userId);
        Task<(bool IsSuccess, string Message)> TimeoutUserInGroupAsync(Guid groupId, string userId, DateTime timeoutUntil);
        Task<(bool IsSuccess, string Message)> RemoveTimeoutUserInGroupAsync(Guid groupId, string userId);
        Task<List<CommunityGroupMembershipDTO>> GetBannedUsersByGroupAsync(Guid groupId);
        Task<List<CommunityGroupMembershipDTO>> GetTimedOutUsersByGroupAsync(Guid groupId);
        Task<(bool IsSuccess, List<DoctorModel>? Doctors)> GetPendingDoctorsAsync();
        Task<(bool IsSuccess, string Message)> ApproveDoctorAsync(string doctorId);
    }
}
