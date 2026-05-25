using Microsoft.EntityFrameworkCore;
using MythicalCreatures.Server.Data;
using MythicalCreatures.Server.Services.Implementations;
using MythicalCreatures.Server.Services.Interfaces;
//Program.cs is the entry point to your entire app.

var builder = WebApplication.CreateBuilder(args);
//this creates a 'builder' - you can think of it as a setup object. Here we register all services onto it before the app starts.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//this is the dependency injection container, this is where you tell the app what services exist, here we're using:
//Controllers (API endpoints)
//Swagger (auto-generated API documentation / testing UI)
//Later we will add our DB Context :)

builder.Services.AddDbContext<MythicalCreaturesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
//This is our DB Context!
builder.Services.AddScoped<ICreatureService, CreatureService>(); //injecting our creatures service!

var app = builder.Build();
//once everything is registered, we build the app. After this line we can no longer register services

app.UseDefaultFiles();
app.UseStaticFiles();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");
//This is the middleware pipeline - the order matters here -> every HTTP request follows this flow in order

app.Run();
//this starts the app & keeps it running until it's stopped


//Services explained:
//The term 'service' is used broadly, it basically means any class that provides functionality & is registered into the dependency injection container
//When you call builder.Services.AddSomething() you're saying 'hey app, this thing exsists & i want to be able to use it anywhere'.

//Service Examples are: 
// - DbContext: provides database access
// - background services: runs tasks on a schedule
// - ILogger: built in, provides logging anywhere in the app
// - Custom services that I build myself: like ICreatureService which contains business logic
// - Third party services: like email senders, payment processors

//Types of Services:
//when you register a service you also define its lifetime - how long an instance lives:
//Singleton - AddSingleton<>() - One instance for the entire lifetime of the app.
//Scoped - AddScoped<>() - One instance per HTTP request
//Transient - AddTransient<>() - A new instance every time it's required.


//Middleware pipeline order (offically from Microsoft!):
//app.UseExceptionHandler()       1. catch errors first
//app.UseHsts()                   2. security headers
//app.UseHttpsRedirection()       3. redirect http -> https
//app.UseStaticFiles()            4. serve static files early
//app.UseRouting()                5. figure out which route matches
//app.UseCors()                   6. CORS before auth
//app.UseAuthentication()         7. who are you?
//app.UseAuthorization()          8. what are you allowed to do?
//app.MapControllers()            9. execute the matched route

//Key rule is: each middleware runs in order and can short-circuit the pipeline. For example if UseAuthorization() rejects a request, it never reaches MapControllers()

//MIGRATIONS! 
//after models are built and we are linked to the DB via DI, lets do migrations
//Migrations - how EF core tracks and applies changes to your database schema over time
// - its kinda like version control for your database.
// - EF generates the SQL to apply the changes automatically

//two commands to know:
// Add-Migration <name>    <- creates a new migration file
// Update-Database         <- applies pending migrations to the DB

//How to run them in VS:
// tools -> NuGet Package Manager -> Package Manager Console
// - this opens a console at the bottom of VS, ensure the default project at top is set to MythicalCreatures.Server
// - then run Add-Migration InitialCreate

// that creates a Migrations folder with two files
// [timestamp]_InitialCreate.cs
// MythicalCreaturesDbContextModelSnapshot.cs


//Lets read this [timestamp] file
// Up() Method
// - This runs when you apply the migration - it creates all your tables. Notice:
// - Tables are created in the right order (CreatureTypes and Regions first since other tables depend on them)
// - .Annotation("SqlServer:Identity", "1, 1") - this means the Id column auto increments (1, 2, 3...)
// - table.ForeignKey(...) - EF generated all your foreign key contraints automatically
// - onDelete: ReferentialAction.Cascade - if a Creatre is deleted, its Abilities are automatically deleted too

//Down() method
// - This is the rolback - if you ever need to undo this migration it drops all tables in reverse order

//CreateIndex
// - EF autmatically created indexes on all your foreign key columns - this speeds up join queries


//now lets run Update-Database
//This creates the database! It also adds all foreign keys, and tables in the right order

//dbo.__EFMigrationsHistory
// - this tracks which migrations have already been applied, so when Update-Database is ran EF checks this table first & only runs migrations that aren't recorded there yet.
// - This is what makes migrations safe for existing data.



//Lets talk DTOs!

//Why not return Models directly?
//1. Over-exposing data: Your Creature model might have fields you dont want the front end to see
//2. Circular references: Creature has List<Abilitiy>, Ability has Creature back - JSON serialization crashes
//3. Tight coupling: If your DB schema changes, your API response changes too - breaking the front end!
//4. Shape mismatch: The frontend might need data shaped differently than how its stored in the DB

//The pattern:

//You'll have two kinds of DTOs for each resource:
// - Response DTO - what you send to the frontend on GET requests
// - Request DTO - what you accept from the frontend on POST/PUT requests

//ex:

//what FE sends to backend:
//public class CreateCreatureDto {
//  public string Name { get; set; }
//  public int CreatureTypeId { get; set; }
//  public double PowerLevel { get; set; }
//  public bool IsDangerous { get; set; }
//  public DateTime DateFirstSighted { get; set; }
//}

//what API sends back
//public class CreatureResponseDto {
//  public int Id { get; set; }
//  public string Name { get; set; }
//  public string CreatureTypeName { get; set; } -flattened from CreatureType!
//  public double PowerLevel { get; set; }
//  public bool IsDangerous { get; set; }
//  public DateTime DateFirstSighted { get; set; }
//  public List<string> Abilities { get; set; } -simplified list of names
//}


