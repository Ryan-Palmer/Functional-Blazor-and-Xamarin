using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace FunctionalXamarin
{
    public static class Startup
    {
        public static void ConfigureViewModels(this IServiceCollection services)
        {
            services.AddScoped<CounterViewModel>();
        }
    }
}
