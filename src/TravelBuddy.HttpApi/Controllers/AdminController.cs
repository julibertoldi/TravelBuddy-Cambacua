using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelBuddy.Admin;
using TravelBuddy.Permissions;
using Volo.Abp;

namespace TravelBuddy.Controllers
{
    [Authorize(TravelBuddyPermissions.Admin.Default)]
    [RemoteService]
    [Route("api/app/admin")]
    public class AdminController : TravelBuddyController, IAdminAppService
    {
        private readonly IAdminAppService _adminAppService;

        public AdminController(IAdminAppService adminAppService)
        {
            _adminAppService = adminAppService;
        }

        [HttpGet("dashboard-stats")]
        public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync()
        {
            return await _adminAppService.GetDashboardStatsAsync();
        }
    }
}