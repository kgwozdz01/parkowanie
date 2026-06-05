using Microsoft.AspNetCore.Identity;
using ParkingSystem.AppCore.Entities;
using ParkingSystem.Infrastructure;
using ParkingSystem.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. Rejestracja całego silnika modułu Infrastruktury (EF Core, Identity, JWT, Polityki)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// 2. Automatyczne uruchomienie seedera bazy danych przy starcie aplikacji
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ParkingSystemDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    
    await DatabaseSeeder.SeedAsync(context, userManager, roleManager);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 3. Bardzo ważna kolejność potoków (Middleware) – najpierw Autentykacja, potem Autoryzacja
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();