using LibraryManagement.DAL.Repositories;
using LibraryManagementDAL.DTO.Dashboard;

namespace LibraryManagement.BLL.Services
{
    public class DashboardService
    {
        private readonly DashboardRepository dashboardRepository;

        public DashboardService(DashboardRepository _dashboardRepository)
        {
            dashboardRepository = _dashboardRepository;
        }

        public async Task<DashboardStatsResult> GetStatsAsync()
        {
            return await dashboardRepository.GetStatsAsync();
        }
    }
}
