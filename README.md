# ClinicHub – Clinic Appointment Management System

ClinicHub is a web-based clinic management system built with ASP.NET Core MVC and SQL Server. It helps manage doctors, patients, medical specialties, working schedules, and appointments in one place. The system provides separate accounts for administrators, doctors, and patients, with different permissions for each role.

## 🔗 GitHub Repository

[ClinicHub Repository](https://github.com/EmranAbdeen/ClinicHub)

## Features

- **Accounts:** User registration and login with session-based authentication and role-based access for Super Admin, Admin, Doctor, and Patient.
- **Super Admin Panel:** Create admin accounts and manage website settings and content, including clinic information and website appearance.
- **Admin Panel:** Manage doctors, patients, specialties, and appointments according to the assigned permissions.
- **Doctor Management:** Manage doctor profiles, specialties, experience, biographies, and active status.
- **Patient Management:** Manage patient profiles and personal information.
- **Specialties:** Organize doctors by medical specialties.
- **Schedules:** Manage doctor availability, working hours, and appointment time slots.
- **Appointments:** Book and manage appointments between patients and doctors, with status tracking.
- **Appointment Status:** Manage appointment workflows, including Pending, Confirmed, Completed, Cancelled, and Rejected.
- **Dashboard:** Provide dashboard pages for clinic management and appointment monitoring.
- **Website Content Management:** Update website settings, clinic information, services, testimonials, and contact details.
- **Authorization:** A custom `AuthFilter` action filter protects routes and restricts access based on user roles.
- **Security:** Passwords are hashed using BCrypt, and session-based authentication identifies logged-in users.
- **API:** RESTful API controllers provide endpoints for doctors, patients, specialties, and appointments.
- **Database:** SQL Server integration using Entity Framework Core and LINQ.
- **User Interface:** Responsive pages built with ASP.NET Core MVC, Razor Views, and Bootstrap.
-

## Data Model

- **User** — stores user information, including `FullName`, `Email`, `PasswordHash`, `Phone`, and `IsActive`. Each user belongs to a `Role` and can have an associated Doctor or Patient profile.
- **Role** — defines user roles and has many `User` records.
- **Doctor** — belongs to a `User` and a `Specialty`. Stores information such as `Biography`, `YearsOfExperience`, `ConsultationInfo`, `ImagePath`, and `IsActive`.
- **Patient** — belongs to a `User` and stores patient information, including `DateOfBirth`, `Gender`, and `ImagePath`.
- **Specialty** — has many `Doctor` records and stores its name, description, image or icon path, and active status.
- **DoctorSchedule** — belongs to a `Doctor` and defines available dates, start and end times, appointment slot duration, and availability.
- **Appointment** — connects a `Patient` with a `Doctor`. Stores the appointment date, time, notes, status, and creation/update timestamps.
- **Service** — stores clinic website service information, including a title, description, and icon or image path.
- **Testimonial** — stores patient testimonials, ratings, visibility, and creation information.
- **ContactMessage** — stores messages submitted through the clinic website.
- **WebsiteSetting** — stores clinic website information, such as the clinic name, address, and contact details.

### Appointment Statuses

Appointments use the `AppointmentStatus` enum:

- `Pending` — the appointment is waiting for a decision.
- `Confirmed` — the appointment has been confirmed.
- `Completed` — the appointment has been completed.
- `Cancelled` — the appointment has been cancelled.
- `Rejected` — the appointment has been rejected.

## Technologies Used

- C#
- ASP.NET Core MVC (.NET 10)
- Entity Framework Core 10
- SQL Server
- LINQ
- HTML5
- CSS3
- JavaScript
- Bootstrap
- REST APIs
- BCrypt.Net-Next

## Getting Started

### Prerequisites

- [.NET 10 SDK]
- SQL Server (local or remote)
- Visual Studio 2022 or later with the ASP.NET and web development workload
- SQL Server Management Studio (SSMS), optional

### Configure the database

Update the `ConnectionClinic` connection string in `appsettings.json` to point to your SQL Server instance.

Example:

```json
"ConnectionStrings": {
  "ConnectionClinic": "Data Source=YOUR_SERVER;Initial Catalog=ClinicDB;Integrated Security=True;TrustServerCertificate=True;"
}
```

Replace `YOUR_SERVER` with your SQL Server instance name.

**Important:** The current project contains a connection string referencing the developer's machine (`EMRAN-ABDEEN`) in both `appsettings.json` and `Models/ClinicDbContext.cs`. Update the connection configuration before running the project on another machine.

The application uses the `ClinicDB` database and the `Clinic` and `Content` schemas.

The database must contain the required tables and initial data, including the roles needed by the application. Check the account registration and authorization logic to confirm the expected role names and any required initial admin account.

### Run the app

Clone the repository and navigate to the directory containing `ClinicHub.csproj`:

```bash
git clone https://github.com/EmranAbdeen/ClinicHub.git
cd ClinicHub
dotnet restore
dotnet run
```

Open the local URL shown in the console.

You can also open `ClinicHub.csproj` in Visual Studio and run the application using **F5**.

**Database note:** No Entity Framework Core migrations were included in the supplied project archive. The database schema must therefore be created or provided before the application can run successfully. The project includes an EF Core database context, but `dotnet ef database update` will not create the required schema without migrations.

## API Endpoints

The project includes API controllers for retrieving doctors, patients, specialties, and appointments.

| **Method** | **Endpoint**             | **Description**               |
| ---------- | ------------------------ | ----------------------------- |
| GET        | `/api/doctors`           | Retrieve all doctors          |
| GET        | `/api/doctors/{id}`      | Retrieve a doctor by ID       |
| GET        | `/api/patients`          | Retrieve all patients         |
| GET        | `/api/patients/{id}`     | Retrieve a patient by ID      |
| GET        | `/api/specialties`       | Retrieve all specialties      |
| GET        | `/api/appointments`      | Retrieve all appointments     |
| GET        | `/api/appointments/{id}` | Retrieve an appointment by ID |

These endpoints reflect the API controller routes in the supplied project. Check the controllers for response formats and any access restrictions.

## Project Structure

```text
ClinicHub/
├── Controllers/
│   ├── APIs/
│   │   ├── AppointmentsController.cs
│   │   ├── DoctorsController.cs
│   │   ├── PatientsController.cs
│   │   └── SpecialtiesController.cs
|	├── Content/
│   │   ├── ContactMessagesController.cs
│   │   ├── ServicesController.cs
│   │   ├── TestimonialsController.cs
│   │   └── WebsiteSettingsController.cs
│   ├── AccountController.cs
│   ├── AdminsController.cs
│   ├── AppointmentsController.cs
│   ├── DashboardController.cs
│   ├── DoctorSchedulesController.cs
│   ├── DoctorsController.cs
│   ├── PatientsController.cs
|	├──	SpecialtiesController
│   └── HomeController.cs
├── DTOs/
│   ├── Account/
│   ├── Admin/
│   ├── Appointment/
│   ├── Doctor/
│   ├── Patient/
│   └── Specialties/
├── Filters/
│   └── AuthFilter.cs
├── Models/
│   ├── Clinic/
│   ├── Content/
│   ├── Enums/
│   ├── ErrorViewModel.cs
│   └── ClinicDbContext.cs
├── Services/
│   ├── Interfaces/
│   └── Implementations/
├── Views/
├── wwwroot/
├── Program.cs
├── appsettings.json
└── ClinicHub.csproj
```

## Screenshots

Add screenshots of the application to demonstrate the main pages and features.

| **Feature**     | **Screenshot**                              |
| --------------- | ------------------------------------------- |
| Home Page       | ![Home Page](/docs/Home.png)                |
| Admin Dashboard | ![Admin Dashboard](/docs/Dashboard.png)     |
| Doctors Page    | ![Doctors Page](/docs/Doctor.png)           |
| DoctorSchedule  | ![DoctorSchedule](/docs/DoctorSchedule.png) |
| Patient Profile | ![Patient Profile](/docs/patient.png)       |
| Appointments    | ![Appointments](/docs/Appointments.png)     |
| Appointments_  | ![Appointments_](/docs/Appointments_.png)    |

Place your screenshots in a folder such as `docs/Screenshots/` and update this section with their paths.

## Notes

- The application uses session-based authentication with session values such as `UserId`, `UserRole`, `UserName`, and `UserEmail`.
- The `AuthFilter` is used to check authentication and role access.
- Passwords are hashed using BCrypt rather than stored as plain text.
- Doctor and patient profiles are associated with user accounts.
- Doctor schedules use dates, start and end times, and configurable appointment slot durations.
- The project uses `DateOnly` and `TimeOnly` for appointment dates and times.

## What was the hardest part?

One of the main challenges was organizing the clinic management system into separate controllers, services, models, and DTOs while keeping the code maintainable.

Another challenge was implementing session-based authentication and role-based authorization, along with managing appointment statuses and connecting doctors, patients, schedules, and appointments through Entity Framework Core.
