using Microsoft.AspNetCore.Blazor.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sotsera.Blazor.Toaster.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using static System.Net.WebRequestMethods;

namespace BlazorPass.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddToaster(config =>
            {
                // Using the demo, was able to come up with sensible values:
                //    https://sotsera.github.io/sotsera.blazor.toaster/
                config.PositionClass = Defaults.Classes.Position.TopCenter;
                config.PreventDuplicates = false;
                config.NewestOnTop = false;
                config.ShowTransitionDuration = 500;
                config.ShowStepDuration = 50;
                config.VisibleStateDuration = 5000;
                config.HideTransitionDuration = 500;
                config.HideStepDuration = 100;
                config.ProgressBarStepDuration = 50;
            });
        }

        public void Configure(IBlazorApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
