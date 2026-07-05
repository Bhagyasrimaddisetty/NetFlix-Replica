# 📚 Library Inventory & Loan Management API

A REST API that models how a real library actually runs — tracking copies, enforcing loan limits, calculating late fees, and catching bad requests before they corrupt state. Built with C# and ASP.NET Core using a clean layered architecture.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![C#](https://img.shields.io/badge/C%23-ASP.NET_Core-239120)
![Tests](https://img.shields.io/badge/xUnit-13_tests-brightgreen)
![CI](https://img.shields.io/badge/CI-GitHub_Actions-2088FF)

## Why this project

Most CRUD demos stop at "create, read, update, delete." This one goes a step further and encodes actual business rules — the kind that show up in real systems and in interview questions:

- What happens when two people try to check out the last copy of a book at the same time?
- How do you stop someone from returning a book they already returned?
- How do you calculate a late fee fairly, and what happens right at the boundary (due today vs. overdue)?

Those edge cases are where this project lives.

## The rules it actually enforces

| Rule | Behavior |
|---|---|
| **Copy tracking** | Every book has `TotalCopies` and `AvailableCopies`; checkout/return keeps them in sync |
| **Checkout limit** | A member can't have more than **5** active loans at once |
| **Loan period** | Every checkout gets a **14-day** due date automatically |
| **Late fees** | Overdue returns are charged per day late, calculated on return *or* on demand for still-active loans |
| **No double returns** | Returning an already-returned loan is rejected, not silently ignored |
| **Not-found handling** | Unknown book/member/loan IDs return a proper 404, not a crash |

## Architecture

```
Controllers  →  Services (business rules)  →  Models
   ↓                                            ↑
 DTOs  ←──────────── mapped in both directions ─┘
```

- **Controllers** stay thin — they just translate HTTP ↔ service calls and map exceptions to status codes (400 for business-rule violations, 404 for not-found).
- **LibraryService** owns every rule in one place, so the logic is testable without spinning up HTTP at all.
- **Custom exceptions** (`BusinessRuleException`, `NotFoundException`) keep error handling explicit instead of relying on magic strings.

## Try it yourself

```bash
dotnet run --project src/LibraryInventoryApi.Api
```

```bash
# Add a book with 2 copies
curl -X POST http://localhost:5299/api/books \
  -H "Content-Type: application/json" \
  -d '{"title":"Clean Code","author":"Robert Martin","isbn":"9780132350884","totalCopies":2}'

# Register a member
curl -X POST http://localhost:5299/api/members \
  -H "Content-Type: application/json" \
  -d '{"name":"Sri","email":"sri@example.com"}'

# Check it out
curl -X POST http://localhost:5299/api/loans/checkout \
  -H "Content-Type: application/json" \
  -d '{"bookId":1,"memberId":1}'
```

Try checking it out a third time (when only 2 copies exist) and watch it correctly reject with a 400. Try returning the same loan twice — same story.

## Test coverage

13 xUnit tests on `LibraryService`, covering the happy paths and — more importantly — the rule violations: max-loan enforcement, zero-availability checkout, double-return, unknown-ID lookups, and late-fee math at the day boundary.

```bash
dotnet test
```

CI runs this automatically via GitHub Actions on every push — see the badge above (or the Actions tab) for current status.

## API Reference

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/books` | List all books |
| `GET` | `/api/books/{id}` | Get a book |
| `POST` | `/api/books` | Add a book |
| `POST` | `/api/members` | Register a member |
| `POST` | `/api/loans/checkout` | Checkout a book |
| `POST` | `/api/loans/{loanId}/return` | Return a book |
| `GET` | `/api/loans/overdue` | List overdue loans |
| `GET` | `/api/loans/member/{memberId}` | List a member's active loans |

## Stack

C# · ASP.NET Core Web API · xUnit · GitHub Actions
