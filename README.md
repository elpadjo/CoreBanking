# CoreBanking API

A modern, scalable banking API built with ASP.NET Core for managing customer accounts, transactions, and banking operations.

## Features

- **Account Management**: Create and manage customer accounts
- **Transaction Processing**: Handle deposits, withdrawals, and transfers
- **Balance Inquiry**: Real-time account balance checks
- **Secure Operations**: Built with security best practices
- **RESTful API**: Clean, standardized endpoints

## Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB for development)
- Git

## Solution Architecture
```json
CoreBanking.sln/
|---CoreBanking.API/                    # Presentation Layer
|       |---Controllers/                # Handles HTTP endpoints and routes requests
|
|---CoreBanking.Infrastructure/         # Data Access Layer  
|       |---Repositories/               # Concrete implementations of data interfaces
|
|---CoreBanking.Application/            # Application Layer
|       |---Services/                   # Business use cases and application logic
|
|---CoreBanking.Core/                   # Domain Layer
|       |---Interfaces/                 # Contracts for repositories and services
|       |---Models/                     # Core domain entities and value objects
```

## Installation & Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-org/corebanking-api.git
   cd corebanking-api
   ```
2. **Restore dependencies**
   ```bash
   dotnet restore
   ```
3. **Configure database connection**
  -  Update connection string in `appsettings.Development.json`
  -  Run database migrations: `dotnet ef database update`
4. **Run the application**. The API will be available at `https://localhost:7000` and `http://localhost:5000`
   ```bash
   dotnet run
   ```
## API Endpoints

### Accounts API Endpoints

| Method | Endpoint              | Description           |
|--------|----------------------|---------------------|
| GET    | /api/accounts         | Get all accounts     |
| GET    | /api/accounts/{id}    | Get account by ID    |
| POST   | /api/accounts         | Create new account   |
| PUT    | /api/accounts/{id}    | Update account details |

### Transactions API Endpoints

| Method | Endpoint                           | Description                    |
|--------|-----------------------------------|--------------------------------|
| POST   | /api/transactions/deposit          | Deposit funds                  |
| POST   | /api/transactions/withdraw         | Withdraw funds                 |
| POST   | /api/transactions/transfer         | Transfer between accounts      |
| GET    | /api/transactions/account/{accountId} | Get transaction history        |

### Customers API Endpoints

| Method | Endpoint                        | Description                |
|--------|---------------------------------|----------------------------|
| GET    | /api/customers                   | Get all customers          |
| POST   | /api/customers                   | Create new customer        |
| GET    | /api/customers/{id}/accounts     | Get customer accounts      |

## Development

### Branch Strategy

- main: Production-ready code
- dev: Development integration branch
- feature/*: Feature development branches

## Contributing

- Create a feature branch from dev
- Make your changes
- Submit a pull request to dev

## API Documentation

Once running, access Swagger documentation at:
   ```bash
   https://localhost:7000/swagger
   ```