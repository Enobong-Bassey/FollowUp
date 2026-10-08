# Church Hospitality App

A responsive web app for a church Hospitality Unit to record new guests and make sure every guest is followed up.

Guests scan a **QR code**, fill a short web form, and their details land in a **SQL Server** database. Two follow-up calls (week 1 and week 2 after the visit) are created automatically. Admins assign each call to a team member, members log the call details, and admins can see who has done their assigned follow-ups.

## Features

- **Public guest form** reached by QR code (mobile-first, no login)
- **Login** with JWT authentication, two roles: `Admin` and `User`
- **Member onboarding:** an admin enters name, email and phone; the member receives an email to confirm their email, set a password and confirm their phone number
- **Auto-created follow-ups:** week 1 (visit + 7 days) and week 2 (visit + 14 days) for every guest
- **Assignment:** admins assign follow-ups to members
- **Call log:** members record date, outcome and notes in a table; admins see all logs for accountability
- **Dashboard:** pending, completed and missed follow-up counts (admins can switch between "mine" and "whole team")
- **Profile page:** update name, phone and profile photo
- **Admin tools:** member management (activate/deactivate), downloadable QR code

## Tech stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 9 Web API, ASP.NET Core Identity, JWT bearer auth |
| Database | SQL Server via Entity Framework Core 9 |
| Email | MailKit (any SMTP server) |
| QR codes | QRCoder |
| Frontend | Plain HTML, CSS and JavaScript, served from `wwwroot` |

## Project structure

```
ChurchHospitalityApp/
├── ChurchHospitalityApp.sln
├── database/schema.sql                  # reference schema
└── src/ChurchHospitalityApp/
    ├── Controllers/                     # Auth, Users, Guests, FollowUps, Dashboard, Profile, QrCode
    ├── Data/                            # DbContext + seeder (roles, default admin)
    ├── DTOs/
    ├── Models/
    ├── Services/                        # Email (MailKit), JWT token service
    ├── wwwroot/                         # HTML pages, css/, js/, img/, uploads/
    ├── appsettings.json
    └── Program.cs
```

## Getting started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (LocalDB, Express, Docker, or a full instance)
- An SMTP account for sending invitation emails (Office 365, Gmail app password, or a dev inbox such as Mailtrap)
- The EF Core CLI: `dotnet tool install --global dotnet-ef`

### 1. Configure settings

Edit `src/ChurchHospitalityApp/appsettings.json`, or better, keep secrets out of Git with user-secrets:

```bash
cd src/ChurchHospitalityApp
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=ChurchHospitalityDb;User Id=sa;Password=<your password>;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:Key" "<a random string of 32+ characters>"
dotnet user-secrets set "Email:Username" "<smtp user>"
dotnet user-secrets set "Email:Password" "<smtp password>"
dotnet user-secrets set "Email:FromAddress" "<from address>"
dotnet user-secrets set "App:DefaultAdmin:Email" "<your admin email>"
dotnet user-secrets set "App:DefaultAdmin:Password" "<a strong password>"
```

Also set these two URLs to wherever the app is hosted (they go into the invitation emails and the QR code):

- `App:FrontendBaseUrl`, for example `https://localhost:5001`
- `App:PublicGuestFormUrl`, for example `https://localhost:5001/guest-form.html`

> The QR code encodes `PublicGuestFormUrl`. Guests' phones must be able to reach it, so use your real public address in production (not `localhost`).

### 2. Create the database

The app applies migrations automatically on startup, but the first migration has to be generated once:

```bash
cd src/ChurchHospitalityApp
dotnet ef migrations add InitialCreate
```

Commit the generated `Migrations/` folder. After that, `dotnet run` creates and updates the database for you. (You can also run `dotnet ef database update` manually.)

### 3. Run

```bash
dotnet run --project src/ChurchHospitalityApp
```

Open the HTTPS address printed in the console. In Development, Swagger is at `/swagger`.

### 4. First login

A default admin is created on first start from `App:DefaultAdmin` (the sample values in `appsettings.json` are `admin@yourchurch.org` / `ChangeMe!123`). **Change these before deploying.**

1. Log in as the admin.
2. **Members → Add Member** to invite your team. Each member gets an email with a registration link.
3. **QR Code** to download and print the guest check-in code.
4. As guests submit the form, use **Assign Follow-Ups** to assign their calls.

## How follow-ups work

| Follow-up | Scheduled for |
|-----------|---------------|
| Week 1 | visit date + 7 days |
| Week 2 | visit date + 14 days |

Members open **My Follow-Ups**, call the guest, and click **Log Call** to record the date, outcome and notes, marking it *Completed* or *Missed*. Admins use the "All Follow-Ups" view on the assign page to see every assignment, who owns it, and what was logged.

## API overview

| Endpoint | Access | Purpose |
|----------|--------|---------|
| `POST /api/auth/login` | Public | Log in, returns JWT |
| `POST /api/auth/complete-registration` | Public (needs emailed token) | Confirm email and set password |
| `POST /api/guests` | Public | Guest form submission |
| `GET /api/guests`, `GET /api/guests/{id}` | Logged in | List and view guests |
| `GET /api/followups?scope=mine\|all\|unassigned` | Logged in (`all`/`unassigned`: Admin) | List follow-ups |
| `PUT /api/followups/{id}/assign` | Admin | Assign a follow-up |
| `PUT /api/followups/{id}/log` | Assignee or Admin | Log a call |
| `GET /api/dashboard/summary` | Logged in | Dashboard counts |
| `GET/PUT /api/profile`, `POST /api/profile/photo` | Logged in | Own profile |
| `GET/POST /api/users`, `PUT /api/users/{id}/active` | Admin | Manage members |
| `GET /api/qrcode` | Admin | QR code PNG |

## Deployment notes

- Serve over **HTTPS** and set a long, random `Jwt:Key`.
- Don't commit real secrets. Use user-secrets locally and environment variables or a secret store in production (for example `Jwt__Key`, `ConnectionStrings__DefaultConnection`).
- Profile photos are stored on disk under `wwwroot/uploads/profile-photos`. Persist that folder (or move to blob storage) if you deploy to a container or ephemeral host.
- The public guest endpoint has no rate limiting or CAPTCHA. Consider adding one before putting the QR code in a public place.

## Possible next steps

- Reminder emails for overdue follow-ups
- Password reset / "forgot password" flow
- Resend-invitation button for members whose email failed or expired
- Export guests and follow-up logs to CSV
- Automated tests (xUnit) for the controllers

## License

Choose a license before publishing (MIT is a common choice for portfolio projects).
