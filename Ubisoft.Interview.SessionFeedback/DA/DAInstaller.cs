using Microsoft.Extensions.DependencyInjection;
using Ubisoft.Interview.SessionFeedback.BL;

namespace Ubisoft.Interview.SessionFeedback.DA
{
    public static class DAInstaller
    {
        public static void Install(IServiceCollection services)
        {
            services.AddTransient<IDynamoDBConnectionFactory,DynamoDBConnectionFactory>();
            services.AddTransient<ISessionFeedbackRepository, SessionFeedbackRepository>();
        }

    }
}
