

0. Teknologistak

  .NET 8 / ASP.NET Core Web API
  EasyNetQ (RabbitMQ)
  xUnit + Moq
  Docker / Docker Compose
  Clean Architecture (DDD-lite principper)

  Brugte hjælpemidler:
  * Stack overflow
  * ChatGPT, Github Copilot
  * Eksempler fra undervisningen

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
  
5. Data persistence

  * Lige nu bruger projektet kun brug af in-memory repository til at gemme brugerprofiler. Det er for at gøre udviklingen hurtig og mere enkel. Imen serciven kører gemmes data i de to Dictonaries: _byId, _byUsername
  i klassen: InMemoryUserProfileRepository. Der er oprettet et interface så man forholdsvis nemt bør kunne skife til en database-understøttet implementering (EF eller PostgreSQL) uden at skulle ændre business logic eller controllers. 

6. Update profile:
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

7. Kør med docker:

  - Byg og start containere:
    * docker compose up -d --build
  - Tjek at alt er oppe:
    * docker compose ps
    Du bør se både profiles-api og rabbitmq som running / healthy.
  - Åbn API’en i browseren:
    Gå til: http://localhost:8080/swagger
    Her kan du se og teste alle endpoints direkte via Swagger UI.
  - Stop containere igen:
    * docker compose down

//---------------------------------------------//
Uge 12: API- gateway

  1. API Gateway Routing
  * Der blev oprettet et nyt projekt apps/api-gateway, som fungerer som systemets eksterne indgangspunkt.
  Her blev Ocelot gjort brug af og routing blev konfigureret i ocelot.json.
  
  Eksempel på en route:
  
    * Upstream: /profiles/{everything}
    * Downstream: /api/v1/Profiles/{everything} → Profile Service (localhost:7206)
  
  API Gateway videresender alle klientkald til ProfileService via de definerede Ocelot-ruter.
  
  2. Authentication, Authorization
  
    Autentifikation udføres udelukkende i API Gateway med JWT Bearer. JWT er sat op med: issuer, audience og symmetrisk nøgle.
    
    Route-beskyttelse via Ocelots AuthenticationOptions (kræver scopes).
  
  Claim-forwarding fra gateway → microservice via custom headers:
  
    * X-UserId (sub claim)
    * X-UserRole (role claim) (Som et eksempel kræver Get profiles kaldet nu en rolle som admin. 
  
  Microservices foretager autorisation baseret på disse headers.
  
  3. Overvejelser
  
  
    Single point of authentication:
    Kun gatewayen validere tokens for at holde det simplet for microservices at håndtere
    
    Minimal coupling:
    Microservices Kender ikke til JWT-formatet. Ved at sende claims som headers opnås en løs kobling.
    
    Udvidelsesmuligheder:
    Ocelot-konfigurationen gør det nemt at tilføje flere services, scopes eller sikkerhedsregler uden at ændre microservices.
    
    Sikkerhedsrisici:
    Forwardede headers kan potentielt misbruges, så det er vigtigt at microservices kun stoler på gatewayen (trusted network).

Uge 13: Reliability

  1. Potentielle Fejlpunkter & Overvejelser
  
* 1. Claim-forwarding fra API Gateway

    ProfileService modtager brugeroplysninger som HTTP-headers (X-UserId, X-UserRole).
    Hvis JWT-tokenet ændrer struktur, eller mapping i Ocelot fejler, kommer disse værdier ind som null.
    Det betyder, at controlleren kan træffe forkerte autorisationsbeslutninger – uden at selve microservicen er nede.
    
    Overvejelse:
    Microservicen håndtere ikke selve JWT'en, så på nogen kald kan de give mening for den ikke at fejle hårdt hvis claims-forwarding svigter. Man kunne implementere "graceful degradition" i nogle tilfælde hvor f.eks en 'gæst' har mulighed for at se. Det kunne gøres ved en fallbakc værdi, som så kunne bruges til at separere kritiske funktioner fra andre.
    
    Mitigation:
    Fallback-værdier og mild håndtering af manglende claims.
    
    var role = HttpContext.Request.Headers["X-UserRole"].FirstOrDefault() ?? "Guest";
    Herefter ville man kunne bruge guest til at checke om det er nogle funktioner som er så tilgængelige at det ville være okay for alle at se. 

* 2. Sårbarhed over for gateway-/netværksfejl

  Hvis API Gateway eller netværket har kortvarige problemer, opfatter klienten ofte dette som fejl i selve ProfileService. Tjenesten har heller ingen egen beskyttelse mod upstream-fejl.
  
  Overvejelse:
  Selvom servicen  ikke selv kalder eksterne API’er, er det vigtigt at tænke over.. Microservices skal robuste mod midlertidige fejl i omgivelserne (gateway, load balancer, netværk).
  
  Mitigation:
  Anvendelse af circuit breaker-mønstre på steder hvor ProfileService har eksterne afhængigheder (fx database, messaging).

* 3. Database-adgang som sårbart punkt

  Databasekald kan fejle midlertidigt: timeouts, forbindelsesdrops eller deadlocks.
  Standardadfærden i .NET er at fejlene bobler op og straks kaster en exception, hvilket giver en unødigt ustabil brugeroplevelse.

  Overvejelse:
  Databasefejl er ofte transiente og løser sig selv inden for millisekunder. Det giver derfor mening at opfange dem og prøve igen, så tjenesten virker stabil selv når databasen har en upser.

2. Implementeret Resiliency-Policy: Retry (with backoff)

  Som en del af opgaven blev der implementeret en resiliency-policy omkring databasekaldet i controlleren:
  
 private static readonly AsyncRetryPolicy<IReadOnlyList<UserProfileDto>> RetryPolicy =
    Policy<IReadOnlyList<UserProfileDto>>
        .Handle<Exception>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * attempt)
        );

// Fallback: Hvis retry fejler 3 gange returneres end tom liste
private static readonly AsyncFallbackPolicy<IReadOnlyList<UserProfileDto>> FallbackPolicy =
    Policy<IReadOnlyList<UserProfileDto>>
        .Handle<Exception>()
        .FallbackAsync(
            fallbackAction: ct =>
            {
                return Task.FromResult<IReadOnlyList<UserProfileDto>>(Array.Empty<UserProfileDto>());
            }
        );

// kombineret resiliency policy
private static readonly IAsyncPolicy<IReadOnlyList<UserProfileDto>> ResiliencyPolicy =
    Policy.WrapAsync(FallbackPolicy, RetryPolicy);
  
  Hvorfor?
  
  Operationen er idempotent (GET)
  
  Fejlen er ofte transients
  
  Det forbedrer stabiliteten markant uden at påvirke funktionalitet.

  Hvis den fejler mere end 3 gange er der en fallback på en tom liste. Man kan argumenterer for om det er en god ide. Måske tror brugeren at der bare ikke er nogen profiler- Men jeg har oplevet nogle programmer hvor det bruges som en default værdi hvis noget går galt for at sørge for at fejlen ikke bobler op.

  Arkitekturmæssige overvejelser:
  
  For øvelsens skyld blev retry-loggen placeret direkte i controlleren, tæt på det konkrete fejlpunkt.
  Men i et rigtigt system hører denne form for logik hjemme i infrastrukturlaget, fx:
  
  i repository-implementeringen
  
  som en decorator omkring dataAccess
  
  eller som globale resiliency-policies registreret i Program.cs
  
  Controlleren bør ikke vide noget om retries eller circuit breakers. Det er en del af grænsen mod upålidelige eksterne ressourcer.

Uge 14:
    1. HashiCorp Vault Integration
       Formål
      Tidligere lå følsomme oplysninger såsom RabbitMQ connection strings i appsettings.json.
      Det skaber problemer ift.:
      
       * risiko for at secrets ender i Git
       * ingen central styring
       * ingen audit trail
       * manglende rotationsmuligheder
      
      Vault løser dette ved at lade microservices hente deres secrets dynamisk ved startup.
      Implementeret i projektet:
      
      * Vault server opsat
      * ører via Docker med egen config.hcl
      * UI på http://localhost:8200
      * Init med unseal keys + root token
      * RabbitMQ secret oprettet i KV2
      * Path: secret/profile-messaging
      * Key: MessagingConnectionString
      
      VaultSharp integration i ProfileService
      Projektet indeholder en VaultHelper + VaultSettings.
      Under startup i Program.cs sker:
      authentication til Vault via token i env-variable
      
      
      direkte injektion i ASP.NET configuration:
      
      builder.Configuration["Messaging:ConnectionString"] = messagingConnectionString;
      
      eksisterende messaging-opsætning bruger nu nøglen fra vault.
    
    * East/West Security (Microservice → Microservice)
    East og West security handler om at microservices også skal være sikre mod hinanden. Alle interne kald gennem gatewayen skal autoriseres.
    Selv intern trafik mellem microservices må ikke være implicit tillid.
    
    
    * Implementeret i projektet
    1. Gateway som central sikkerhedskomponent
    
    API Gateway håndterer alt (Fra tidligere opgave):
    
    * Validating JWT tokens (AddJwtBearer)
    * Scopes- Hvilke der er tilladt.
    * Claim forwarding
    * Route-level authorization
    
    Microservices behøver derfor ikke forstå JWT-formatet.
    
    2. Claim-forwarding til ProfileService
    
      Fra ocelot.json:
      
      "AddHeadersToRequest": {
        "X-UserId": "Claims[sub] > value",
        "X-UserRole": "Claims[role] > value"
      }
      
      
      ProfileService læser derefter:
      
      var role = Request.Headers["X-UserRole"].FirstOrDefault();
      
      
      Dette bruges til intern autorisation (f.eks. Admin vs Service).
      
      3. Intern autorisation i controlleren
      
      ProfileService bruger forwarded claims til at skelne mellem interne roller:
      
      if (role != "Admin")
          return Forbid();
      
      
      På den måde kan kun microservices med korrekt rolle/scope tilgå interne endpoints.
      Derudover har jeg tilføjet et nyt scope og et nyt endpoint som kun er tilladt for dem med det rigtige scope. Det skal simulere nogle endpoints hvor kun internal services snakker:
      ocelot-opsætning:

    <img width="567" height="179" alt="image" src="https://github.com/user-attachments/assets/68d260ec-ac4e-4cf8-a19a-86851a095726" />

        
    
    - Fordele/ulemper
    
        Fordelene ved dette setup:
        
        * Kun én komponent validerer tokens → nemmere at administrere
        * Microservices holdes simple og rene
        * Ingen interne services bliver eksponeret direkte
        * Rollesystemet kan udvides uden ændringer i services
        
        Ulemper:
        * kan give ekstra latency i systemet
        
        * Gateway er et centralt single point of failure
        * Hvis gateway fejler, fejler al east/west trafik også

uge 15 Saga:

Valgt opgave:
" Saga: User upgrades their workspace subscription to premium.

User Management: Validate user eligibility → Revert user eligibility validation
Payment: Charge user payment method → Refund payment upon failure
Profile: Assign premium badge → Remove premium badge upon failure "

Valgte Saga Pattern: Orchestration

Sagaer kan implementeres på to måder:
   Orchestration (det vi bruger)
      * Ét centralt “Saga Orchestrator”-objekt styrer hele workflowet.
      * Orchestratoren sender kommandoer og afventer svar.

Sagt med andre ord: En “director” fortæller alle services hvad de skal gøre.

Fordele i denne case:

* lettere at læse, debugge og logge
* Alle regler og state ligger ét sted


Ulemper:

* Orchestratoren bliver en central afhængighed
* Har tendenser til at kunne ende ud i monolistisk foretningslogik

Anden mulighed: Choreography 
  * Ingen central styring
  * Microservices reagerer på events og sender selv nye events
  * Workflowet "opstår" ud af hændelser
  * Mere "event-driven" stil
  
  Fordele:
  * Ingen single point of orchestration
  * Services er ekstremt løst koblede
  
  Ulemper:
  * Vanskeligere at forstå flowet
  * Debugging og fejlhåndtering mere kompliceret
    

Implementering:
2. Saga Workflow: Premium Upgrade

Saga’en hedder UserUpgradeSaga og simulerer en multi-service proces:

Brugeren opgraderer til premium

Betaling gennemføres

Display name opdateres (“* Premium”) for enden. 

Notifikation sendes

Hvis noget fejler → kompenserende handlinger udføres.

3. Implementerede Handlers (simulerede microservices)

For at undgå afhængigheder til eksterne systemer blev microservices simuleret in-memory:

Service	Handler	Handlinger
Payment	PaymentCommandHandler	Charge / Refund
Profile	ProfilePremiumHandler	Update badge / Revert badge
Notification	NotificationHandler	Send notification

Saga’en kommunikerer med dem vha. Rebus commands og events.

4. Sagaens Flow (Simpel)

  1
    POST /api/v1/profiles/{id}/upgrade-to-premium
    
    → Saga oprettes i Rebus storage
    → Første kommando sendes: ChargeUserPayment
  
  2️ Betaling
    
    Payment-handler svarer med:
    
    PaymentProcessedSuccessfully → fortsæt
    
    PaymentFailed → afslut med fejl
  
  3 Profilopdatering
  
    → UpdateUserProfileToPremium
    Hvis mislykket → kompensation: Refund
  
  4️ Notifikation
  
    → SendUpgradeNotification
    Hvis mislykket → kompensation: RevertProfile + Refund
  
  5️ Saga afsluttes
  
    oprydning af Rebus automatisk,

5. Kompensationslogik (Rollback)

Hvis det fejler håndteres det således

Scenario	Kompensation
Profil-update fejler	Refund betaling
Notification fejler	Revert profil og refund betaling.


Note:
3. Implementerede Handlers (simulerede microservices)

* Jeg har kun en service så jeg simulerede kald og returværdier fra andre 'services'- De er kodet til at returnere true men kan ændres til false for at teste flowet. 

  Service	Handler	Handlinger
  Payment	PaymentCommandHandler	Charge / Refund
  Profile	ProfilePremiumHandler	Update badge / Revert badge
  Notification	NotificationHandler	Send notification
  
  Saga’en kommunikerer med dem vha. Rebus commands og events.
* Jeg tilføjede en masse console.writeline for at holde øje med flowet.



Final notes:
Lige nu kører systemet ikke i docker, da jeg har en bug med at få vault'en til at kommunikere med profiles-servicen.  


