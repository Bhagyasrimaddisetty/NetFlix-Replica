namespace LibraryInventoryApi.Api.DTOs;

public record BookCreateRequest(string Title, string Author, string Isbn, int TotalCopies);

public record CheckoutRequest(int BookId, int MemberId);

public record LoanResponse(
    int LoanId,
    int BookId,
    string BookTitle,
    int MemberId,
    DateTime CheckoutDate,
    DateTime DueDate,
    DateTime? ReturnDate,
    decimal LateFee
);
