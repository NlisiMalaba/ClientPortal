using Domain;

namespace Domain.Tests;

public sealed class PaymentTests
{
    [Fact]
    public void Create_SetsFields_AndNormalizesValues()
    {
        DateTime paidAt = DateTime.UtcNow.AddMinutes(-2);

        Payment payment = Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            1500.457m,
            "zar",
            "eft",
            " ref-001 ",
            paidAt,
            "  partial payment ");

        Assert.Equal(1500.46m, payment.Amount);
        Assert.Equal("ZAR", payment.Currency);
        Assert.Equal("eft", payment.Method);
        Assert.Equal("ref-001", payment.Reference);
        Assert.Equal("partial payment", payment.Notes);
        Assert.Equal(paidAt, payment.PaidAt);
    }

    [Fact]
    public void Create_WithEmptyInvoiceId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => Payment.Create(
                Guid.NewGuid(),
                Guid.Empty,
                100m,
                "USD",
                "card",
                "abc-123",
                DateTime.UtcNow.AddMinutes(-1)));
    }
}
