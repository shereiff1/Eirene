<div align="center">

<!-- Replace with your actual logo -->
<img src="https://placehold.co/200x200/4A90D9/ffffff?text=Eirene&font=montserrat" alt="Eirene Logo" width="160" />

<h1>Eirene</h1>

<p><em>A safe space to heal. A professional tool to care.</em></p>

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Hangfire](https://img.shields.io/badge/Hangfire-Background_Jobs-2A9D8F?style=for-the-badge)](https://www.hangfire.io/)
[![Cloudinary](https://img.shields.io/badge/Cloudinary-Storage-3448C5?style=for-the-badge&logo=cloudinary&logoColor=white)](https://cloudinary.com/)
[![Railway](https://img.shields.io/badge/Deployed_on-Railway-0B0D0E?style=for-the-badge&logo=railway&logoColor=white)](https://railway.app/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](./LICENSE)

</div>

---

## 📋 Table of Contents

- [About the Project](#-about-the-project)
- [Key Features](#-key-features)
- [Architecture & Tech Stack](#-architecture--tech-stack)
- [Getting Started](#-getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Configuration](#configuration)
  - [Running Migrations](#running-ef-core-migrations)
  - [Running the API](#running-the-api)
- [Contributing](#-contributing)
- [License](#-license)

---

## 💙 About the Project

Mental health is often invisible. The gap between patients who need help and the doctors who can provide it is wider than it should be — and the platforms meant to bridge that gap are frequently clinical, cold, and hard to navigate.

**Eirene** was built to change that.

Named after the Greek goddess of peace, Eirene is a comprehensive, modern healthcare platform designed to genuinely connect patients with verified medical professionals, provide a safe and private space for mental health tracking, and nurture communities where people can support one another through shared experience.

At its core, Eirene has two north stars:

- **For patients** — a compassionate, secure environment to track emotional wellbeing, journal privately, seek guidance from real verified doctors, and find community.
- **For doctors** — a professional, streamlined tool to manage patient supervision, verify credentials, and engage with the wider health community through knowledge-sharing.

We believe that healthcare technology should feel human. Every architectural decision in Eirene — from role-based access controls to the doctor legitimacy verification pipeline — was made with both trust and empathy in mind.

---

## ✨ Key Features

### 👤 User Profiles & Role Management
Eirene supports three distinct roles — **Patient**, **Doctor**, and **Admin** — each with a fully tailored experience. Access controls are enforced at every layer of the application, ensuring users only ever see and do what's appropriate for their role.

### 🏥 Doctor Legitimacy Verification
Before any doctor can supervise patients, they must submit their medical licenses and credentials through Eirene. These documents are securely stored via Cloudinary and reviewed by platform admins. Only verified, approved doctors unlock supervision capabilities — protecting patients from unqualified practitioners.

### 🔗 Patient Supervision
Patients can send supervision requests to verified doctors. Once accepted, the doctor gains a monitored view of the patient's health activity and progress, creating an ongoing, accountable care relationship.

### 🧠 Mental Health Tracking & Journaling
Patients have access to a private mood-tracking system and a personal journal. These tools are built for emotional safety — journals are private by design, and mood history helps both patients and their supervising doctors identify patterns over time.

### 🛡️ Community Moderation
Eirene hosts community groups where patients can connect and share experiences. Admins and designated moderators have robust tools to maintain a safe environment, including the ability to **timeout**, **ban**, and **unban** community members as needed.

### 📝 Blogs & Q&A
A dedicated knowledge-sharing hub where doctors and patients alike can publish medical blogs, ask questions, and share answers — bridging the information gap in an accessible, community-driven way.

---

## 🏗️ Architecture & Tech Stack

Eirene is built on a clean, maintainable **N-Tier Service-Oriented Architecture** following Clean Architecture principles. The codebase is organized into three primary layers, each with a clear and single responsibility:

```
Eirene/
├── Eirene.API/            # Presentation Layer — Controllers, Middleware, DI Registration
├── Eirene.BLL/            # Business Logic Layer — Services, DTOs, Validators, ML Integrations
└── Eirene.DAL/            # Data Access Layer — EF Core DbContext, Repositories, Migrations
```

**Patterns Used:**
- **Repository Pattern** — Abstracts database access, keeping the BLL decoupled from EF Core directly.
- **Unit of Work Pattern** — Coordinates multiple repository operations within a single transaction boundary.
- **Dependency Injection** — Throughout all layers, leveraging ASP.NET Core's built-in DI container.

---

### 🛠️ Full Tech Stack

| Category | Technology |
|---|---|
| **Framework** | ASP.NET Core Web API (.NET 9.0) |
| **Database** | PostgreSQL 16 |
| **ORM** | Entity Framework Core 9 |
| **Authentication** | JWT Bearer Tokens + Google OAuth 2.0 |
| **File Storage** | Cloudinary (ephemeral-safe cloud storage) |
| **Background Jobs** | Hangfire |
| **Email** | SendGrid |
| **ML / AI** | Python.NET (localized ML model integrations) |
| **Deployment** | Railway App Services |

---

## 🚀 Getting Started

Follow the steps below to get a local instance of Eirene up and running for development or contribution.

### Prerequisites

Make sure you have the following installed on your machine:

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/) (v14 or higher recommended)
- [Python](https://www.python.org/downloads/) (if using ML features via Python.NET)
- A [Cloudinary](https://cloudinary.com/) account (free tier works)
- A [SendGrid](https://sendgrid.com/) account (free tier works)
- [Git](https://git-scm.com/)

---

### Installation

**1. Clone the repository**

```bash
git clone https://github.com/your-username/eirene.git
cd eirene
```

**2. Restore dependencies**

```bash
dotnet restore
```

---

### Configuration

Eirene uses `appsettings.json` for configuration. Navigate to the `Eirene.API` project and create an `appsettings.Development.json` (or edit `appsettings.json` directly). Below is the full configuration template you'll need to fill in:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=eirene_db;Username=YOUR_PG_USER;Password=YOUR_PG_PASSWORD"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_STRONG_JWT_SECRET_KEY_HERE",
    "Issuer": "Eirene",
    "Audience": "EireneUsers",
    "ExpiryInMinutes": 60
  },
  "GoogleAuth": {
    "ClientId": "YOUR_GOOGLE_OAUTH_CLIENT_ID",
    "ClientSecret": "YOUR_GOOGLE_OAUTH_CLIENT_SECRET"
  },
  "Cloudinary": {
    "CloudName": "YOUR_CLOUDINARY_CLOUD_NAME",
    "ApiKey": "YOUR_CLOUDINARY_API_KEY",
    "ApiSecret": "YOUR_CLOUDINARY_API_SECRET"
  },
  "SendGrid": {
    "ApiKey": "YOUR_SENDGRID_API_KEY",
    "SenderEmail": "no-reply@youreirene.com",
    "SenderName": "Eirene Health"
  },
  "Hangfire": {
    "DashboardPath": "/hangfire"
  }
}
```

> ⚠️ **Security Note:** Never commit real credentials to source control. Add `appsettings.Development.json` to your `.gitignore`, or better yet, use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development and environment variables in production.

---

### Running EF Core Migrations

Eirene uses **Entity Framework Core** for database schema management. Once your PostgreSQL connection string is configured, apply the existing migrations to create the database schema:

```bash
# Navigate to the DAL project (where the DbContext lives)
cd Eirene.DAL

# Apply all pending migrations
dotnet ef database update --startup-project ../Eirene.API
```

If you've made changes to models and need to create a new migration:

```bash
dotnet ef migrations add YourMigrationName --startup-project ../Eirene.API
dotnet ef database update --startup-project ../Eirene.API
```

> **Note:** Make sure the `dotnet-ef` global tool is installed. If not: `dotnet tool install --global dotnet-ef`

---

### Running the API

```bash
cd Eirene.API
dotnet run
```

By default, the API will be available at:
- **HTTP:** `http://localhost:5000`
- **HTTPS:** `https://localhost:5001`
- **Swagger UI:** `https://localhost:5001/swagger` *(available in Development mode)*
- **Hangfire Dashboard:** `https://localhost:5001/hangfire` *(Admin-only access)*

---

## 🤝 Contributing

Eirene is open to contributions from the community. Whether you're fixing a bug, improving documentation, or proposing a new feature — we'd love to have you involved.

**Here's how to get started:**

1. **Fork** the repository
2. **Create** a feature branch: `git checkout -b feature/your-feature-name`
3. **Commit** your changes with clear, descriptive messages: `git commit -m "feat: add mood trend analytics endpoint"`
4. **Push** to your fork: `git push origin feature/your-feature-name`
5. **Open a Pull Request** against the `main` branch and describe your changes

**A few things we ask:**

- Follow existing code style and architectural conventions (N-Tier, Repository Pattern, etc.)
- Write meaningful commit messages (we follow [Conventional Commits](https://www.conventionalcommits.org/))
- For significant feature additions, please open an Issue first to discuss the design
- Be kind. This project is about healthcare — let's keep the community welcoming and respectful

---

## 📄 License

Distributed under the **MIT License**. See [`LICENSE`](./LICENSE) for full details.

---

<div align="center">

Built with care by passionate developers who believe technology can make healthcare more human. 💙

*Eirene — peace, in every sense of the word.*

</div>
