## UPDATED PER SESSION WITH NOTES (if not within code) with OpenCode!

# 5-24
Backend Service layer Refactoring

MythicalCreatures.Server
├── Controllers/
│   └── CreaturesController.cs     ← slimmed down, delegates to service
├── Services/
│   ├── Interfaces/
│   │   └── ICreatureService.cs    ← defines the contract
│   └── Implementations/
│       └── CreatureService.cs     ← contains the business logic
├── DTOs/                          ← stays the same
├── Models/                        ← stays the same
├── Data/                          ← stays the same

What we're moving:
- Entity Frameowkr logic moves from within the controller into the service
- ICreatureService defines the method signature
- CreaturesController calls methods from ICreatureService 
-  - help keep structure clean, controllers only handle http concerns
-  - controller doesnt care how method works, just retuns results
- Regiser ICreatureService -> CreatureService in Program.cs
-  - meaning give this service a lifetime, tell the DI container "When asking for a ICreatureService, give them a CreatureService"
-  - better for testing and swapping implementations



# 5-25
Where we are at now!
Remaining steps:
1. Finish CreatureService — move all 4 remaining methods from controller ← you're doing this now
2. Slim down CreaturesController — inject ICreatureService, replace logic with service calls
3. Register in Program.cs — builder.Services.AddScoped<ICreatureService, CreatureService>()
4. Add ILogger to CreatureService — inject it via constructor, log key events and errors
5. SQL Server Database Project — add .sqlproj to solution, write table .sql files, build DACPAC
6. Background Services — Service 1 (every minute), Service 2 (even/odd day)
7. CORS config — so your React frontend can call the API

