using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SSBOL;
using SSDAL;

namespace SSWebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Step-5 Applying at application level
            var authPol = new AuthorizationPolicyBuilder()
                               .AddAuthenticationSchemes(new string[] {
                                   JwtBearerDefaults.AuthenticationScheme })
                                .RequireAuthenticatedUser()
                                .Build();


            services.AddMvc(x => x.Filters.Add(new AuthorizeFilter(authPol)));

            services.AddCors();
            services.AddDbContext<SSDbContext>(x => x.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            
            services.AddIdentity<SSUser, IdentityRole>()
                    .AddEntityFrameworkStores<SSDbContext>()
                    .AddDefaultTokenProviders();

            services.AddTransient<IStoryDb, StoryDb>();
            //Step-1: Create signingKey from Secretkey
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this-is-my-secret-key-for-angular8-app"));

            //Step-2:Create Validation Parameters using signingKey
            var tokenValidationParameters = new TokenValidationParameters()
            {
                IssuerSigningKey = signingKey,
                ValidateIssuer = false,
                ValidateAudience = false
            };

            //Step-3: Set Authentication Type as JwtBearer
            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                    //Step-4: Set Validation Parameter created above
                    .AddJwtBearer(jwt =>
                    {
                        jwt.TokenValidationParameters = tokenValidationParameters;
                    }).AddGoogle("Google", options =>
                    {
                        options.CallbackPath = new PathString("/g-callback");
                        options.ClientId = "23952772306-gcn8hcccb0m2oabfg097ck1jqf10t6ur.apps.googleusercontent.com";
                        options.ClientSecret = "Cp6KbKwE0FKd31GPd4PbHlAw";
                    });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(opt => opt.AllowAnyHeader()
                                 .AllowAnyMethod()
                                 .AllowAnyOrigin()
                                 .AllowCredentials());

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
