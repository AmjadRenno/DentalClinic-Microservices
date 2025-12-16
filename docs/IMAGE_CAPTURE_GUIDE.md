# Image Capture Guide

This guide will help you capture and organize screenshots and diagrams for the project documentation.

## 📸 Capturing Screenshots

### Windows Tools

#### 1. Snipping Tool (Built-in)
- Press `Win + Shift + S`
- Select area to capture
- Save as PNG in appropriate folder

#### 2. ShareX (Recommended - Free)
- Download: https://getsharex.com/
- Features: Auto-upload, annotations, region capture
- Shortcut: Configure custom hotkeys

#### 3. Lightshot
- Download: https://app.prntscr.com/
- Quick editing and sharing
- Shortcut: `PrtScn`

### Browser DevTools (for UI screenshots)
1. Open your application in browser
2. Press `F12` to open DevTools
3. Press `Ctrl + Shift + P` (Command Palette)
4. Type "screenshot" and select:
   - **Capture full size screenshot** - for entire page
   - **Capture screenshot** - for visible area
   - **Capture node screenshot** - for specific element

### Recommended Screenshot Settings
- **Format**: PNG (lossless)
- **Resolution**: 1920x1080 or higher
- **Browser**: Chrome/Edge (for consistency)
- **Window size**: Maximize or use fixed size (1920x1080)

## 🎨 Screenshot Checklist

### Before Capturing UI Screenshots:

- [ ] Clear browser cache and cookies
- [ ] Use realistic sample data (not "test test")
- [ ] Hide/remove development tools
- [ ] Check for spelling errors
- [ ] Ensure UI is fully loaded
- [ ] Use consistent window size
- [ ] Zoom to 100%
- [ ] Hide personal information

### Good Practices:
✅ Use realistic names: "Ahmed Ali", "Sara Mohammed"  
✅ Use realistic dates: Current date or near future  
✅ Fill forms completely  
✅ Show successful states  
✅ Use consistent theme/styling  

### Avoid:
❌ Empty tables or lists  
❌ Lorem ipsum text  
❌ "test" or "asdf" data  
❌ Broken images  
❌ Console errors visible  

## 📋 Step-by-Step: Capturing Each Section

### 1. Homepage Screenshot

```bash
# 1. Navigate to homepage
http://localhost:5000

# 2. Clear any logged-in sessions
# 3. Take full-page screenshot
# 4. Save as: docs/images/screenshots/01-homepage.png
```

### 2. Login Page

```bash
# 1. Navigate to login
http://localhost:5000/login

# 2. Show empty form (or filled with sample data)
# 3. Capture
# 4. Save as: docs/images/screenshots/02-login-page.png
```

### 3. Patient Dashboard

```bash
# 1. Login as patient
# 2. Navigate to dashboard
# 3. Ensure appointments are visible
# 4. Capture
# 5. Save as: docs/images/screenshots/04-patient-dashboard.png
```

### 4. Dentist Dashboard

```bash
# 1. Login as dentist
# 2. Navigate to dentist dashboard
# 3. Show list of appointments
# 4. Capture
# 5. Save as: docs/images/screenshots/08-dentist-dashboard.png
```

### 5. Aspire Dashboard

```bash
# 1. Run: dotnet run --project DentalClinic.AppHost
# 2. Open Aspire dashboard (check console for URL, usually http://localhost:15xxx)
# 3. Wait for all services to start
# 4. Capture different views:
#    - Main dashboard: aspire-dashboard-overview.png
#    - Resources list: aspire-resources-list.png
#    - Traces: aspire-traces.png
#    - Logs: aspire-logs.png
#    - Metrics: aspire-metrics.png
```

## 🎯 Creating Architecture Diagrams

### Using Draw.io

1. Go to https://app.diagrams.net/
2. Select "Create New Diagram"
3. Choose template or start blank
4. Create your diagram following the text descriptions in `DIAGRAMS_SOURCE.md`
5. Export as PNG (300 DPI)
6. Save to `docs/images/architecture/`

### Using Mermaid (in VS Code)

1. Install "Markdown Preview Mermaid Support" extension
2. Copy mermaid code from `DIAGRAMS_SOURCE.md`
3. Create new `.md` file with mermaid code
4. Right-click diagram in preview → "Copy Image"
5. Paste into image editor and save as PNG

### Using PlantUML

1. Install "PlantUML" extension in VS Code
2. Copy PlantUML code from `DIAGRAMS_SOURCE.md`
3. Create `.puml` file
4. Press `Alt + D` to preview
5. Right-click diagram → Export as PNG
6. Save to appropriate folder

## 📊 Diagram Recommendations

### System Architecture Diagram Should Show:
- All microservices (Auth, Booking, Payment, Gateway, CMS, Orchestrator)
- API Gateway as entry point
- Databases (SQLite)
- Dapr runtime
- Client applications (Web Browser)
- Communication lines (HTTP, Pub/Sub)

### Flow Diagrams Should Include:
- Clear start and end points
- Decision points (if/else)
- Different actors (Patient, Dentist, System)
- Success and error paths
- Data flow direction

## 🗂️ Organizing Your Images

After capturing all images, organize them:

```
docs/images/
├── architecture/
│   ├── system-architecture.png          # Overall system
│   ├── microservices-overview.png       # Services detail
│   ├── ddd-structure.png                # DDD layers
│   └── service-communication.png        # How services talk
├── screenshots/
│   ├── 01-homepage.png
│   ├── 02-login-page.png
│   ├── 03-register-page.png
│   ├── 04-patient-dashboard.png
│   ├── 05-my-appointments.png
│   ├── 06-booking-form.png
│   ├── 07-booking-confirmation.png
│   ├── 08-dentist-dashboard.png
│   ├── 09-appointments-management.png
│   └── 10-umbraco-cms.png
├── diagrams/
│   ├── booking-flow.png
│   ├── authentication-flow.png
│   ├── payment-sequence.png
│   ├── dapr-pubsub-flow.png
│   └── database-schema.png
└── aspire/
    ├── aspire-dashboard-overview.png
    ├── aspire-resources-list.png
    ├── aspire-traces.png
    ├── aspire-logs.png
    └── aspire-metrics.png
```

## 🔄 Updating README.md

After adding images, update the main README.md to reference them:

```markdown
## Screenshots

### System Architecture
![System Architecture](docs/images/architecture/system-architecture.png)

### Application
![Homepage](docs/images/screenshots/01-homepage.png)
![Patient Dashboard](docs/images/screenshots/04-patient-dashboard.png)

### Monitoring
![Aspire Dashboard](docs/images/aspire/aspire-dashboard-overview.png)
```

## ✅ Final Checklist

Before committing images to Git:

- [ ] All images are in PNG format
- [ ] File sizes are reasonable (<500KB each)
- [ ] Filenames follow naming convention (lowercase, hyphens)
- [ ] Images are placed in correct folders
- [ ] No personal or sensitive information visible
- [ ] Images are clear and readable
- [ ] README.md updated with image references
- [ ] Tested that images display correctly on GitHub

## 🔧 Image Optimization Tools

If your images are too large:

- **TinyPNG**: https://tinypng.com/
- **Compressor.io**: https://compressor.io/
- **ImageOptim** (Mac): https://imageoptim.com/
- **PngOptimizer** (Windows): https://psydk.org/pngoptimizer

## 📝 Notes

- GitHub markdown automatically sizes images to fit
- You can specify image size: `![Alt](image.png){width=500px}`
- Use tables for side-by-side images
- Add descriptive alt text for accessibility
- Consider creating a GIF for complex flows

## 🎥 Optional: Screen Recordings

For complex interactions, consider adding GIFs:

1. Use **ScreenToGif** (Windows): https://www.screentogif.com/
2. Record the interaction (keep under 10 seconds)
3. Edit and optimize
4. Save to `docs/images/demos/`
5. Reference in README: `![Booking Demo](docs/images/demos/booking-flow.gif)`

---

**Remember**: Good documentation with clear visuals makes your project more professional and easier for others to understand!
