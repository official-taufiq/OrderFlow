# OrderFlow

OrderFlow is an order management REST API built with **ASP.NET Core, C#, Entity Framework Core, and PostgreSQL**.

It supports authentication, role-based authorization, product management, transactional order processing, and concurrency-safe inventory updates.

## Tech Stack

- .NET 10 / ASP.NET Core
- C#
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- BCrypt
- Docker Compose
- xUnit

## Features

- User registration and login
- JWT-based authentication
- Customer and Admin roles
- Product CRUD operations
- Role-based endpoint authorization
- Order creation and order history
- Admin order management
- Transactional stock deduction
- PostgreSQL row-level locking to prevent overselling
- Centralized exception handling
- Structured logging
- Integration tests with an isolated PostgreSQL test database

## API Endpoints

### Authentication

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/auth/register` | Public |
| POST | `/api/auth/login` | Public |

### Products

| Method | Endpoint | Access |
|---|---|---|
| GET | `/api/products` | Public |
| GET | `/api/products/{id}` | Public |
| POST | `/api/products` | Admin |
| PUT | `/api/products/{id}` | Admin |
| DELETE | `/api/products/{id}` | Admin |

### Orders

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/orders` | Authenticated |
| GET | `/api/orders/me` | Authenticated |
| GET | `/api/orders/{id}` | Owner |
| GET | `/api/orders` | Admin |
| PATCH | `/api/orders/{id}/status` | Admin |

## Running Locally

Start PostgreSQL:

```bash
docker compose up -d
```

Apply database migrations:

```bash
dotnet ef database update --project src/OrderFlow.Api
```

Run the API:

```bash
dotnet run --project src/OrderFlow.Api
```

Run tests:

```bash
dotnet test
```

## Order Processing

Order creation runs inside a database transaction. Product rows are locked using PostgreSQL `SELECT ... FOR UPDATE` while stock is validated and deducted, preventing concurrent orders from overselling inventory.

## Project Structure

```text
src/OrderFlow.Api/
├── Controllers/
├── Data/
├── Dtos/
├── Middleware/
├── Migrations/
└── Models/

tests/OrderFlow.Api.Tests/
```