# Library Inventory & Loan Management API

A REST API for managing a library's book inventory, members, and loans, built with ASP.NET Core (C#) using a layered architecture (Controllers → Services → Models).

## Features
- Book inventory management with copy-count tracking
- Member registration
- Checkout / return workflow with business rules:
  - No checkout when zero copies are available
  - Max 5 active loans per member
  - 14-day loan period
  - Late fee calculation on overdue returns
  - Prevents double-returning the same loan
- Overdue loan and per-member active-loan queries

## Tech Stack
- C# / .NET 8, ASP.NET Core Web API
- xUnit for unit tests (service-layer business rules)
- GitHub Actions CI (build + test on every push/PR)

## Project Structure
```
src/LibraryInventoryApi.Api/
  Controllers/   - BooksController, LoansController, MembersController
  Services/      - ILibraryService / LibraryService (business logic), custom exceptions
  Models/        - Book, Member, Loan
  DTOs/          - request/response records
tests/LibraryInventoryApi.Tests/
  LibraryServiceTests.cs - 13 unit tests covering checkout limits, availability,
                           return rules, late-fee calculation, overdue/active queries
```

## Running locally
```bash
dotnet restore
dotnet run --project src/LibraryInventoryApi.Api
```

## Running tests
```bash
dotnet test
```

## API Endpoints
| Method | Route | Description |
|---|---|---|
| GET | /api/books | List all books |
| GET | /api/books/{id} | Get a book |
| POST | /api/books | Add a book |
| POST | /api/members | Register a member |
| POST | /api/loans/checkout | Checkout a book |
| POST | /api/loans/{loanId}/return | Return a book |
| GET | /api/loans/overdue | List overdue loans |
| GET | /api/loans/member/{memberId} | List a member's active loans |
