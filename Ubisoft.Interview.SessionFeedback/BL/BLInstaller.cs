using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ubisoft.Interview.SessionFeedback.BL
{
    public class BLInstaller
    {
        public static void Install(IServiceCollection services)
        {
            services.AddSingleton<ILastFeedbacksWriterService, LastFeedbacksWriterService>();
            services.AddHostedService<LastFeedbacksWriterService>();
        }
    }
}
