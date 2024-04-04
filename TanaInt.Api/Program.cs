using TanaInt.Domain.WallChanger;
using TanaInt.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IGCalService, GCalService>();
builder.Services.AddScoped<IBannerChangerService, BannerChangerService>();
builder.Services.AddScoped<ICalendarHelperService, CalendarHelperService>();
builder.Services.AddScoped<IFsrsService, FsrsService>();
builder.Services.AddCors(o =>
{
    o.AddPolicy("Tana", builder =>
    {
        builder.WithOrigins("https://app.tana.inc")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("Tana");
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();