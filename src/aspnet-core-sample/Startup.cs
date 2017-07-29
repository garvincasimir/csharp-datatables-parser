using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using websample.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;

namespace websample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
        
            services.AddDbContext<PersonContext>(options => options.UseInMemoryDatabase("aspnet-core-websample"));

            services.AddMvc()
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    });
                    
            return services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            
            var ctx = serviceProvider.GetService<PersonContext>();
            SeedSampleData(ctx);


            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void SeedSampleData(PersonContext context)
        {
            var people = new List<Person>
            {
                new Person
                {
                    FirstName = "James",
                    LastName = "Jamie",
                    BirthDate = DateTime.Parse("5/3/1960"),
                    Children = 5,
                    Height = 5.4M,
                    Weight = 250M
                },
                new Person
                {
                    FirstName = "Tony",
                    LastName = "Tonia",
                    BirthDate = DateTime.Parse("7/3/1961"),
                    Children = 3,
                    Height = 4.4M,
                    Weight = 150M
                },
                new Person
                {
                    FirstName = "Bandy",
                    LastName = "Momin",
                    BirthDate = DateTime.Parse("8/3/1970"),
                    Children = 1,
                    Height = 5.4M,
                    Weight = 250M
                },
                new Person
                {
                    FirstName = "Tannie",
                    LastName = "Tanner",
                    BirthDate = DateTime.Parse("2/3/1950"),
                    Children = 0,
                    Height = 6.4M,
                    Weight = 350M
                },
                new Person
                {
                    FirstName = "Cromie",
                    LastName = "Crammer",
                    BirthDate = DateTime.Parse("9/3/1953"),
                    Children = 15,
                    Height = 6.2M,
                    Weight = 120M
                },
                new Person
                {
                    FirstName = "Xorie",
                    LastName = "Zera",
                    BirthDate = DateTime.Parse("10/3/1974"),
                    Children = 2,
                    Height = 5.9M,
                    Weight = 175M
                }
            };

            context.AddRange(people);
            context.SaveChanges();

        }
    }
}