# Project Overview

## Result

This repository is a small ASP.NET Core 9 library management solution split into 4 projects: API, MVC client, business logic, and data access. The main implemented features appear to be authentication, book management, and inventory copy management.

## Structure

### `LibraryManagement.API`

- Hosts the REST API.
- Configures SQL Server via EF Core.
- Registers `AuthService`, `AuthenRepository`, and JWT bearer authentication.
- Current visible API surface is centered on authentication.

Key files:

- `LibraryManagement.API/Program.cs`
- `LibraryManagement.API/Controllers/Auth/AuthController.cs`

### `LibraryManagement.BLL`

- Contains business logic and DTOs.
- `AuthService` handles:
  - login
  - register
  - change password
  - password hashing
  - JWT generation

Key files:

- `LibraryManagement.BLL/Services/AuthService.cs`
- `LibraryManagement.BLL/DTO/Auth/*`

### `LibraryManagement.DAL`

- Contains EF Core data access, entities, migrations, and repositories.
- `ApplicationDbContext` defines the database model and relationships.
- Includes global soft-delete filtering for entities derived from `BaseEntity`.
- Repositories currently include:
  - `AuthenRepository`
  - `BookRepository`
  - `InventoryRepository`

Key files:

- `LibraryManagement.DAL/Data/ApplicationDbContext.cs`
- `LibraryManagement.DAL/Repositories/AuthenRepository.cs`
- `LibraryManagement.DAL/Repositories/BookRepository.cs`
- `LibraryManagement.DAL/Repositories/InventoryRepository.cs`

### `LibraryManagement.Client`

- ASP.NET Core MVC client application.
- Uses cookie authentication and session storage.
- Calls the API over HTTP for authentication.
- Handles book and inventory UI flows through MVC controllers.

Key files:

- `LibraryManagement.Client/Program.cs`
- `LibraryManagement.Client/Controllers/Auth/AuthController.cs`
- `LibraryManagement.Client/Controllers/Books/BooksController.cs`
- `LibraryManagement.Client/Controllers/Inventory/InventoryController.cs`
- `LibraryManagement.Client/Controllers/HomeController.cs`

## Main Flows

### Authentication flow

1. The MVC client posts login/register/change-password requests to the API.
2. The API controller forwards those requests to `IAuthService`.
3. `AuthService` validates credentials, hashes/verifies passwords, and generates JWTs.
4. The client stores the returned token and user info in session.
5. The client signs the user in locally using cookie authentication and role claims.

Relevant files:

- `LibraryManagement.Client/Controllers/Auth/AuthController.cs`
- `LibraryManagement.API/Controllers/Auth/AuthController.cs`
- `LibraryManagement.BLL/Services/AuthService.cs`
- `LibraryManagement.DAL/Repositories/AuthenRepository.cs`

### Book management flow

- The MVC client directly uses `BookRepository`.
- Features include:
  - book listing with filtering, sorting, and paging
  - details view
  - add book
  - update book
  - toggle active status
  - cover image upload to `wwwroot/uploads/books`

Relevant files:

- `LibraryManagement.Client/Controllers/Books/BooksController.cs`
- `LibraryManagement.DAL/Repositories/BookRepository.cs`

### Inventory flow

- The MVC client directly uses `InventoryRepository`.
- Features include:
  - inventory listing with filtering and paging
  - manage copies for a selected book
  - add copies with generated barcodes
  - update copy status, condition, and location

Relevant files:

- `LibraryManagement.Client/Controllers/Inventory/InventoryController.cs`
- `LibraryManagement.DAL/Repositories/InventoryRepository.cs`

## Data Model

The database model includes:

- users, roles, and user-role mapping
- authors, categories, publishers
- books and physical book copies
- borrow transactions and borrow details
- reservations
- payments and payment details
- book reviews
- AI request logs and AI summary entities
- notifications

Important `ApplicationDbContext` behavior:

- enum values are stored as integers
- selected decimal fields use explicit precision
- several uniqueness indexes are defined
- soft delete is implemented with a global query filter and delete interception

## Architectural Notes

- The architecture is layered, but not fully consistent across features.
- Authentication follows a cleaner API -> BLL -> DAL flow.
- Books and inventory currently bypass the API and use DAL repositories directly from the MVC client.
- This means the solution is partly service-oriented and partly server-side MVC with direct data access.

## Current Observations

- The client project references `LibraryManagement.DAL`.
- The client auth controller uses `ApiSettings:BaseUrl` and calls the API at `http://localhost:5070`.
- The API is configured with SQL Server and JWT settings in `LibraryManagement.API/appsettings.json`.
- The repository includes packages and config for Google auth, email, VNPay, Quartz, EPPlus, and OpenAI, but those integrations do not appear fully exposed in the currently visible application flow.

## Potential Gaps

- `LibraryManagement.Client` controllers for books and inventory depend on repositories, but the visible client startup file does not register:
  - `ApplicationDbContext`
  - `BookRepository`
  - `InventoryRepository`
- That may mean:
  - the project is still in progress
  - DI setup is incomplete
  - or part of the configuration exists outside the files reviewed

## Summary

This project is best understood as a library management solution with:

- a JWT-based auth API
- an MVC frontend with cookie/session auth
- EF Core + SQL Server persistence
- implemented book and inventory management
- broader planned features for borrowing, payments, notifications, AI, and external integrations
