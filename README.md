# HospitalMS — Enterprise Hospital Management System

Industrial-grade ASP.NET Core 8 MVC application with modern aesthetic UI, live theme switching, and smooth animations.

## Features

### Clinical Modules
- **Patient Management** — Full registration, demographics, medical history, allergies, search & pagination
- **Appointments** — Scheduling with doctor assignment, time slots, status workflow (Scheduled → Confirmed → In Progress → Completed)
- **Beds & Wards** — Real-time bed board with ward types (ICU, CCU, General, Pediatric, Maternity, Private), assign/discharge workflow

### Finance
- **Billing** — Multi-line invoices (consultation, lab, medicine, room), partial payments, payment methods, balance tracking

### Administration
- **Staff & Users** — Role-based access (Admin, Doctor, Nurse, Receptionist, Accountant)
- **Identity** — ASP.NET Core Identity with secure password policy, account lockout, last-login tracking

### UI / UX (Industrial Grade)
- **Live Theme** — Instant dark/light mode toggle with system preference detection & localStorage persistence
- **Live Animations** — Staggered table rows, count-up stats, fade-in page transitions, pulse indicators, hover micro-interactions
- **Live Clock** — Real-time clock in header with live status dot
- **Responsive** — Mobile-first sidebar with overlay, collapsible navigation
- **Design System** — CSS custom properties, consistent spacing, professional typography (Inter + Plus Jakarta Sans)

## Quick Start

```bash
cd HospitalMS
dotnet restore
dotnet run
```

Open https://localhost:****

### Demo Accounts

| Role         | Email                        | Password        |
|--------------|------------------------------|-----------------|
| Admin        | admin@hospitalms.com         | Admin@12345     |
| Doctor       | dr.smith@hospitalms.com      | Doctor@123      |
| Receptionist | reception@hospitalms.com     | Reception@123   |

## Architecture

```
HospitalMS/
├── Controllers/     # MVC controllers with authorization policies
├── Data/            # EF Core DbContext + seed initializer
├── Models/          # Domain entities + view models
├── Views/           # Razor views (Account, Patients, Appointments, Beds, Billing, Users, Home)
├── wwwroot/
│   ├── css/site.css # Complete design system (light + dark)
│   └── js/site.js   # Theme engine, animations, UX helpers
├── Program.cs       # DI, Identity, middleware pipeline
└── HospitalMS.csproj
```

## Production Notes

- Switch `UseInMemoryDatabase` → `UseSqlServer` in `Program.cs` for production
- Connection string is in `appsettings.json`
- HTTPS, HSTS, and anti-forgery tokens are enabled
- Role-based authorization policies protect admin routes
- Soft-delete pattern used for patients (IsActive flag)

## Tech Stack

- ASP.NET Core 8 MVC
- Entity Framework Core 8 (InMemory / SQL Server)
- ASP.NET Core Identity
- Custom CSS design system (no Bootstrap CSS dependency for layout)
- Bootstrap Icons
- Vanilla JS (zero framework overhead for theme & animation)
