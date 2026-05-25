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