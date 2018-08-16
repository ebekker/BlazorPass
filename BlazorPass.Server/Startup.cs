using BlazorPass.Server.MFA;
using Microsoft.AspNetCore.Blazor.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Net.Mime;
using Unosquare.PassCore.Common;
using Zyborg.PassCore.PasswordProvider.LDAP;

namespace BlazorPass.Server
{
    public class Startup
    {
        private ILogger _logger;

        public IConfiguration AppConfig { get; }

        public Startup(ILogger<Startup> logger, IConfiguration config)
        {
            _logger = logger;
            AppConfig = config;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            //services.AddCors();
            services.AddMvc();

            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    MediaTypeNames.Application.Octet,
                    WasmMediaTypeNames.Application.Wasm,
                });
            });

            var pstOptions = AppConfig.GetSection(nameof(PresetOptions));
            var pwdOptions = AppConfig.GetSection(nameof(LdapPasswordChangeOptions));
            var mfaOptions = AppConfig.GetSection("MFA");

            services.Configure<PresetOptions>(pstOptions);
            services.Configure<MfaOptions>(mfaOptions);
            services.AddSingleton<MfaResolver>();
            services.Configure<LdapPasswordChangeOptions>(pwdOptions);
            services.AddSingleton<IPasswordChangeProvider, LdapPasswordChangeProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseResponseCompression();
            app.UseCors(builder =>
            {
                builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowAnyOrigin();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller}/{action}/{id?}");
            });

            app.UseBlazor<Client.Program>();
        }
    }
}
