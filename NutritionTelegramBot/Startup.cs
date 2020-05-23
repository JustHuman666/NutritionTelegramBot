
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NutritionTelegramBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<DiaryDatabaseSettings>(
                Configuration.GetSection(nameof(DiaryDatabaseSettings)));

            services.AddSingleton<IDiaryDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<DiaryDatabaseSettings>>().Value);
            services.AddSingleton<Commands>();
             
        }

       
    }
}
