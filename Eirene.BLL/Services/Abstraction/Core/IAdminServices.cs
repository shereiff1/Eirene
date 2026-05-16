using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eirene.BLL.Models.Community.Membership;
using Eirene.BLL.Models.Core.Admin;
using Eirene.BLL.Models.Core.Doctor;

namespace Eirene.BLL.Services.Abstraction.Core
{
    public interface IAdminServices : IAdminProfileService, IRoleManagementService, ICommunityModerationService
    {
    }
}
