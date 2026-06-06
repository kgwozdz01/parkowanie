using Microsoft.AspNetCore.Identity;
using ParkingSystem.AppCore.Entities;
using ParkingSystem.Infrastructure;
using ParkingSystem.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Rejestracja podstawowych usług
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Najprostsza możliwa rejestracja Swaggera – bez dotykania obiektów OpenAPI w kodzie C#
builder.Services.AddSwaggerGen();

// Rejestracja konfiguracji z warstwy Infrastruktury (Baza danych, Identity, JWT)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Automatyczne uruchomienie seedera bazy danych przy starcie aplikacji
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ParkingSystemDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    
    await DatabaseSeeder.SeedAsync(context, userManager, roleManager);
}

// Konfiguracja potoku HTTP (Middleware)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Kluczowa kolejność przetwarzania żądań – najpierw Autentykacja, potem Autoryzacja
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();