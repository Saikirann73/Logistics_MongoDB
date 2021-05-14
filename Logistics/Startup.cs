using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Logistics.DAL;
using Logistics.DAL.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Logistics
{
  public class Startup
  {
    private const string connectionString = "mongodb+srv://saikirann73:kiran9009@cluster0.sxadq.mongodb.net";
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllers();
      services.AddSwaggerGen();
      services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
      {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
      }));
      services.AddSingleton<IMongoClient>(x => { return new MongoClient(connectionString); });
      services.AddSingleton<ICitiesDAL, CitiesDAL>();
      services.AddSingleton<IPlanesDAL, PlanesDAL>();
      services.AddSingleton<ICargoDAL, CargoDAL>();
      services.AddSingleton<CargoChangeStream, CargoChangeStream>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, CargoChangeStream cargoChangeStream)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      // Enable middleware to serve generated Swagger as a JSON endpoint.
      app.UseSwagger();

      // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
      // specifying the Swagger JSON endpoint.
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
      });
      app.UseCors("MyPolicy");
      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
      new Thread(() => cargoChangeStream.Init()).Start();
    }
  }
}
