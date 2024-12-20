
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Cho phép mọi nguồn chạy
            builder.Services.AddCors(Options=> Options.AddDefaultPolicy(policy=>policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()));

            builder.Services.AddDbContext<DbContextRoom>(option => 
            {
                option.UseSqlServer(builder.Configuration.GetConnectionString("Connecting"));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
