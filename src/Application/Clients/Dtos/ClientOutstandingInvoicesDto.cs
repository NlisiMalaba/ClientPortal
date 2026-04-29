namespace Application.Clients.Dtos;

public sealed record ClientOutstandingInvoicesDto(
    int Count,
    decimal TotalAmount);
