# Documentation

This folder contains all documentation assets for the Dental Clinic System project.

## 📚 Quick Links

- **[Images Index](IMAGES_INDEX.md)** - Gallery of all visual assets
- **[Image Capture Guide](IMAGE_CAPTURE_GUIDE.md)** - Step-by-step guide for taking screenshots
- **[Diagrams Source](DIAGRAMS_SOURCE.md)** - Text-based diagrams (Mermaid, PlantUML)
- **[Images Folder](images/)** - All visual assets organized by category

## 📊 Current Status

✅ **8 Images Added** (Architecture, Screenshots, Aspire)  
⏳ **17+ Images Pending** (See [IMAGES_INDEX.md](IMAGES_INDEX.md) for details)

## Folder Structure

```
docs/
├── images/
│   ├── architecture/      # Architecture diagrams and system design
│   ├── screenshots/       # UI screenshots and frontend views
│   ├── diagrams/          # Flow diagrams, sequence diagrams, etc.
│   └── aspire/           # .NET Aspire dashboard screenshots
└── README.md             # This file
```

## Image Guidelines

### Architecture Diagrams (`images/architecture/`)
Place high-level architecture diagrams here:
- System architecture overview
- Microservices architecture diagram
- Domain-Driven Design structure
- Component relationships

**Suggested filenames:**
- `system-architecture.png`
- `microservices-overview.png`
- `ddd-structure.png`
- `api-gateway-flow.png`

### Screenshots (`images/screenshots/`)
Place application UI screenshots here:
- Homepage
- Login/Register pages
- Patient dashboard
- Dentist dashboard
- Booking forms
- Appointment management views

**Suggested filenames:**
- `01-homepage.png`
- `02-login-page.png`
- `03-register-page.png`
- `04-patient-dashboard.png`
- `05-dentist-dashboard.png`
- `06-booking-form.png`
- `07-appointments-list.png`

### Diagrams (`images/diagrams/`)
Place technical diagrams here:
- Sequence diagrams
- Flow diagrams
- Database schema
- JWT authentication flow
- Pub/Sub messaging flow

**Suggested filenames:**
- `booking-flow.png`
- `authentication-flow.png`
- `payment-sequence.png`
- `dapr-pubsub-flow.png`
- `database-schema.png`

### Aspire Dashboard (`images/aspire/`)
Place .NET Aspire dashboard screenshots here:
- Dashboard overview
- Service traces
- Metrics and monitoring
- Logs view
- Resource dependencies

**Suggested filenames:**
- `aspire-dashboard-overview.png`
- `aspire-services-view.png`
- `aspire-traces.png`
- `aspire-metrics.png`
- `aspire-logs.png`
- `aspire-resource-graph.png`

## Image Format Recommendations

- **Format**: PNG (for screenshots and diagrams), SVG (for scalable diagrams)
- **Resolution**: Minimum 1920x1080 for screenshots
- **File Size**: Keep under 500KB when possible (use compression tools)
- **Naming**: Use lowercase with hyphens (kebab-case)

## Adding Images to README

Reference images in the main README.md using relative paths:

```markdown
![System Architecture](docs/images/architecture/system-architecture.png)
![Homepage](docs/images/screenshots/01-homepage.png)
![Aspire Dashboard](docs/images/aspire/aspire-dashboard-overview.png)
```

## Tools for Creating Diagrams

- **Architecture Diagrams**: 
  - [Draw.io](https://app.diagrams.net/)
  - [Excalidraw](https://excalidraw.com/)
  - [PlantUML](https://plantuml.com/)
  
- **Sequence Diagrams**:
  - [SequenceDiagram.org](https://sequencediagram.org/)
  - [Mermaid](https://mermaid.js.org/)
  
- **Screenshot Tools**:
  - Windows Snipping Tool
  - ShareX
  - Lightshot

## Best Practices

1. **Keep images up-to-date**: Update screenshots when UI changes
2. **Use descriptive names**: Make filenames self-explanatory
3. **Optimize file sizes**: Compress large images before committing
4. **Add alt text**: Always provide descriptive alt text in markdown
5. **Version control**: Don't commit temporary or test images
6. **Consistency**: Use consistent style and quality across all images

## License

All documentation assets in this folder are part of the Dental Clinic System project and follow the same license as the main project.
