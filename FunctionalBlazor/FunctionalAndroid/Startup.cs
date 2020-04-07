using static FunctionalBlazor.Program.Startup;
using static FunctionalBlazor.Caching.Startup;
using static FunctionalBlazor.Composition.Startup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FunctionalXamarin;

namespace FunctionalAndroid
{
    public class Startup
    {
        internal static void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
        {
            services.ConfigureCaching();
            services.ConfigureProgram();
            services.ConfigureViewModels();
            services.ConfigureFunctions();
        }
    }
}