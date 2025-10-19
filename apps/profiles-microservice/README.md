

0. Teknologistak

  .NET 8 / ASP.NET Core Web API
  EasyNetQ (RabbitMQ)
  xUnit + Moq
  Docker / Docker Compose
  Clean Architecture (DDD-lite principper)

1. Projekt Struktur:
  Lagdelt design inspireret af Domain-Driven Design (DDD):
  apps/
    profiles-microservice/
      src/
        ProfileService.Api/            # Web API + DI + message client-registrering
        ProfileService.Application/    # Use-case services, DTO’er, mappings
        ProfileService.Domain/         # Entities, Value Objects, Domain Events, Interfaces
        ProfileService.Infrastructure/ # In-memory repository (kan erstattes med rigtig DB senere)
        ProfileService.Contracts/      # Delte integrationskontrakter (DTO’er + events)
      tests/
        ProfileService.UnitTests/        # Unit tests for Domain + Application (xUnit + Moq)
        ProfileService.IntegrationTests/ # In-memory API tests (WebApplicationFactory + Fake bus)
      Dockerfile
      ProfileService.sln
  Designovervejelser

  * Separation of concerns: Hvert lag har et klart ansvar og ingen cirkulære afhængigheder.
  * Testbarhed: Domain og Application kan testes uden database eller netværk.
  * Fleksibilitet: Persistence, messaging og API kan ændres uafhængigt.
  * Skalerbarhed: Hvert lag og hver service kan udvides eller distribueres separat.

  Domain & Application 
  Implementererkernekomponenter

  * Entity: UserProfile	Repræsenterer en brugerprofil med identitet og livscyklus.
  * Value Object: Email	Uforanderlig og valideret emailadresse.
  * Domain Events: ProfileCreated, ProfileUpdated	bruges til at vise ændringer til andre systemer.
  * Repository Interface: IUserProfileRepository	Definerer persistensgrænseflade.
  * Service: UserProfileService	Håndterer create/update/get/list logik.
  * Infrastructure: In-memory repository	Bruges til test, kan erstattes af database.

2. REST API 

  Base path: /api/v1/profiles

  POST	/	{ username, displayName, email, bio? }	201 Created + ProfileSharedDto + event profiles.created
  GET	/{id}	—	200 OK + ProfileSharedDto eller 404
  GET	/	—	200 OK + liste af profiler
  PUT	/{id}	{ displayName, email, bio?, avatarUrl? }	200 OK + opdateret ProfileSharedDto + event profiles.updated
  DELETE	/{id}	—	204 NoContent eller 404

  Detaljer: 
  * Controller kun ansvarlig for HTTP-handling og validering.
  * Forretningslogik håndteres i IUserProfileService.
  * Integrationshændelser publiceres via IMessageClient (RabbitMQ).
  * Inputvalidering med [Required], [EmailAddress], [MinLength].
  * HTTP returnerer status koder: (201, 200, 204, 400, 404, 409).

3. Messaging (RabbitMQ via EasyNetQ)

  For at understøtte kommunikation mellem mikrotjenester implementerede jeg et simpelt messaging-lag oven på RabbitMQ via EasyNetQ.
  I stedet for at lade API’et kommunikere direkte med RabbitMQ, byggede jeg en abstraktion (IMessageClient) med metoderne PublishAsync<T> og Subscribe<T>.

  Implementeringen (EasyNetQMessageClient) skjuler alle detaljer om broker-konfiguration og gør det muligt senere at udskifte RabbitMQ uden at ændre forretningskoden.
  Tjenesten publicerer integrationshændelser som profiles.created og profiles.updated, som andre mikrotjenester (fx Notifications) kan reagere på.

  Fordele:

  * Løs kobling: API’et kender ikke til RabbitMQ direkte – Afhænger kun af en interface.
  * Udvidelsesmulighed: Broker eller messaging-framework kan skiftes uden kodeændringer i Domain/Application.
  * Skalerbarhed: Hændelsesdrevet arkitektur gør det muligt for flere services at reagere på ændringer uden at kende hinanden.

4. Docker
  For at gøre tjenesten nem at køre, teste og distribuere implementerede jeg en komplet containeriseringsløsning med Docker og Docker Compose.
  Målet var at skabe et miljø, hvor både API’et og eksterne afhængigheder (som RabbitMQ) kan køre isoleret, men stadig samarbejde problemfrit.

  Hvad jeg har prøvet på:
  * Oprettede en multi-stage Dockerfile:
    Build stage: Bruger .NET SDK til at genskabe dependencies, bygge og publicere i Release-mode.
    Runtime stage: Bruger et letvægts .NET ASP.NET Runtime image, som kun indeholder det nødvendige for at køre API’et.

  * Tilføjede et docker-compose.yml, der starter både:
    profiles-api 
    rabbitmq:3-management
  
  * Implementerede health check på RabbitMQ for at undgå race conditions ved opstart.

  overvejelser:

  * Reproducerbarhed: Hele miljøet bør kunne startes med ét enkelt kommando (docker compose up --build).
  * Isolering: API’et, broker og tests kører i separate containere uden lokale konflikter.
  * Sikkerhed & effektivitet: Multi-stage build reducerer image-størrelse og fjerner build-værktøjer fra runtime.
  * Portabilitet: Kan nemt køre lokalt, i CI/CD pipelines eller i cloud-miljøer uden ændringer.

5. Update profile:
    Jeg havde allerede lavet en simpel update, men som den første 'feature' refaktorerede jeg den efter den givet userstory.
  Hvad blev gjort:
  * I Domain-laget blev metoden UserProfile.Update tilføjet.
    Den sammenligner de nye værdier med de eksisterende og tracker hvilke felter der faktisk er ændret. Kun hvis der er ændringer, udløses et ProfileUpdated domain event.
  * I Applikation-laget håndteres use-casen via UserProfileService.UpdateAsync(), som opdaterer profilen gennem repository og returnerer de ændrede felter.
  * I API-laget blev PUT /profiles/{id}-endpointet tilføjet i ProfilesController.
      Validerer input med DataAnnotations.
      Kalder IUserProfileService.UpdateAsync().
  * Publicerer et ProfileUpdated integrations-event via IMessageClient, som indeholder ChangedFields[].
  * Unit tests til at teste business logic
  * Integrations test til at teste API’et opdaterer in-memory repository’et og publicerer et ProfileUpdated event korrekt.
* Eventet sker kun når der rent faktisk er en ændring. Ikke hvis noget opdaterer uden at ændrer noget.

