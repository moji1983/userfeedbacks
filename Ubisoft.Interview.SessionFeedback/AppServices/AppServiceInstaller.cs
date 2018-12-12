using Microsoft.Extensions.DependencyInjection;

namespace Ubisoft.Interview.SessionFeedback.AppServices
{
    public class AppServiceInstaller
    {
        public static void Install(IServiceCollection services)
        {
            services.AddSingleton<ISessionFeedbackAppService, SessionFeedbackAppService>();
        }
    }
}
