# Architecture Diagrams Source

This file contains text-based architecture diagrams that can be rendered using various tools.

## System Architecture (Mermaid)

```mermaid
graph TB
    subgraph "Client Layer"
        Browser[Web Browser]
        Mobile[Mobile App]
    end

    subgraph "API Gateway Layer"
        Gateway[Gateway Service<br/>YARP + JWT Auth]
    end

    subgraph "Microservices Layer"
        Auth[Auth Service<br/>JWT Tokens]
        Booking[Booking Service<br/>DDD Architecture]
        Payment[Payment Service<br/>DDD Architecture]
        Orchestrator[Orchestrator Service<br/>Dapr Workflows]
        CMS[Umbraco CMS]
    end

    subgraph "Infrastructure"
        Dapr[Dapr Runtime<br/>Pub/Sub + State]
        AuthDB[(Auth DB<br/>SQLite)]
        BookingDB[(Booking DB<br/>SQLite)]
        PaymentDB[(Payment DB<br/>SQLite)]
        UmbracoDb[(Umbraco DB<br/>SQLite)]
    end

    Browser --> Gateway
    Mobile --> Gateway
    Gateway --> Auth
    Gateway --> Booking
    Gateway --> Payment
    Gateway --> Orchestrator
    Browser --> CMS

    Auth --> AuthDB
    Booking --> BookingDB
    Booking --> Dapr
    Payment --> PaymentDB
    Payment --> Dapr
    CMS --> UmbracoDb

    style Gateway fill:#ff6b6b
    style Auth fill:#4ecdc4
    style Booking fill:#45b7d1
    style Payment fill:#96ceb4
    style Orchestrator fill:#ffeaa7
    style CMS fill:#dfe6e9
```

## Booking Flow Sequence (Mermaid)

```mermaid
sequenceDiagram
    participant Patient
    participant Gateway
    participant Auth
    participant Booking
    participant Dapr
    participant Payment

    Patient->>Gateway: POST /auth/login
    Gateway->>Auth: Forward login request
    Auth-->>Gateway: Return JWT token
    Gateway-->>Patient: JWT token

    Patient->>Gateway: POST /api/appointments (with JWT)
    Gateway->>Gateway: Validate JWT
    Gateway->>Gateway: Extract userId
    Gateway->>Booking: Forward request + X-UserId header
    Booking->>Booking: Create appointment (Requested status)
    Booking-->>Gateway: Appointment created
    Gateway-->>Patient: Success response

    Patient->>Gateway: POST /api/appointments/confirm
    Gateway->>Booking: Forward confirm request
    Booking->>Booking: Change status to Confirmed
    Booking->>Dapr: Publish "appointments.confirmed" event
    Dapr->>Payment: Deliver event via Pub/Sub
    Payment->>Payment: Create payment record (500 DKK)
    Booking-->>Gateway: Appointment confirmed
    Gateway-->>Patient: Confirmation response
```

## DDD Structure (PlantUML)

```plantuml
@startuml
package "BookingService" {
    package "API Layer" {
        [Controllers]
    }
    
    package "Application Layer" {
        [Application Services]
        [Commands]
        [Queries]
        [Interfaces]
    }
    
    package "Domain Layer" {
        [Entities]
        [Value Objects]
        [Domain Events]
        [Aggregates]
    }
    
    package "Infrastructure Layer" {
        [Repositories]
        [DbContext]
        [External Services]
    }
}

[Controllers] --> [Application Services]
[Application Services] --> [Commands]
[Application Services] --> [Queries]
[Application Services] --> [Interfaces]
[Application Services] --> [Entities]
[Repositories] --> [Entities]
[Repositories] --> [DbContext]
@enduml
```

## JWT Authentication Flow (PlantUML)

```plantuml
@startuml
actor User
participant Browser
participant Gateway
participant AuthService
participant BookingService
database AuthDB

User -> Browser: Enter credentials
Browser -> Gateway: POST /auth/login
Gateway -> AuthService: Forward login
AuthService -> AuthDB: Validate user
AuthDB --> AuthService: User data
AuthService -> AuthService: Generate JWT token
AuthService --> Gateway: Return JWT
Gateway --> Browser: JWT token
Browser -> Browser: Store JWT

Browser -> Gateway: GET /api/appointments/mine\n(Authorization: Bearer <token>)
Gateway -> Gateway: Validate JWT signature
Gateway -> Gateway: Extract userId from claims
Gateway -> BookingService: Forward request\n(X-UserId header)
BookingService -> BookingService: Read X-UserId header
BookingService --> Gateway: Appointments data
Gateway --> Browser: Response
@enduml
```

## Dapr Pub/Sub Flow (Mermaid)

```mermaid
sequenceDiagram
    participant Booking as Booking Service
    participant DaprB as Dapr Sidecar (Booking)
    participant PubSub as Pub/Sub Component
    participant DaprP as Dapr Sidecar (Payment)
    participant Payment as Payment Service

    Booking->>DaprB: PublishEventAsync("appointments.confirmed")
    DaprB->>PubSub: Publish to topic
    PubSub->>DaprP: Notify subscriber
    DaprP->>Payment: POST /appointments/confirmed
    Payment->>Payment: Create payment record
    Payment-->>DaprP: 200 OK
```

## Service Communication Matrix

| Service | Auth | Booking | Payment | Orchestrator | CMS |
|---------|------|---------|---------|--------------|-----|
| **Gateway** | ✅ HTTP | ✅ HTTP | ✅ HTTP | ✅ HTTP | ❌ |
| **Auth** | - | ❌ | ❌ | ❌ | ❌ |
| **Booking** | ❌ | - | 📤 Pub/Sub | ❌ | ❌ |
| **Payment** | ❌ | 📥 Pub/Sub | - | ❌ | ❌ |
| **Orchestrator** | ❌ | ✅ Dapr | ✅ Dapr | - | ❌ |
| **CMS** | ✅ HTTP | ✅ HTTP | ❌ | ❌ | - |

Legend:
- ✅ Direct HTTP communication
- 📤 Publishes events
- 📥 Subscribes to events
- ❌ No direct communication

## How to Render These Diagrams

### Mermaid Diagrams
1. **GitHub**: Mermaid is natively supported in GitHub markdown
2. **VS Code**: Install "Markdown Preview Mermaid Support" extension
3. **Online**: [Mermaid Live Editor](https://mermaid.live/)

### PlantUML Diagrams
1. **VS Code**: Install "PlantUML" extension
2. **Online**: [PlantUML Web Server](http://www.plantuml.com/plantuml/)
3. **Export**: Generate PNG/SVG and place in appropriate folders

## Tips for Creating Diagrams

1. Keep diagrams simple and focused
2. Use consistent colors and styles
3. Add legends when needed
4. Export at high resolution (300 DPI for print)
5. Save source files (.mmd, .puml) for future edits
