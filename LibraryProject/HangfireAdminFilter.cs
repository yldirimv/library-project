using Hangfire.Dashboard;

namespace LibraryProject
{
    public class HangfireAdminFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
            => context.GetHttpContext().User.IsInRole("Admin");
    }
}