using Milan.Web.Models;
using Milan.Web.Services;
using Xunit;

namespace Milan.Tests;

public sealed class CommerceTests
{
    private readonly CatalogService _catalog = new();

    [Fact]
    public void CatalogContainsTwelveOriginalProducts()
    {
        var products = _catalog.GetAll();
        Assert.Equal(12, products.Count);
        Assert.Equal("AeroMILA Halcón P2", products[0].Name);
        Assert.Equal(19.50m, products[0].Price);
        Assert.EndsWith("/images/products/1.png", products[0].ImagePath);
        Assert.Null(_catalog.Find(999));
    }

    [Fact]
    public void CartAddsMultipleUnitsAndCalculatesSubtotalsAndTotal()
    {
        var cart = new Cart();
        var product = _catalog.Find(1)!;
        cart.Add(product);
        cart.Add(product);
        cart.Add(_catalog.Find(2)!);
        Assert.Equal(2, cart.Items.Single(item => item.ProductId == 1).Quantity);
        Assert.Equal(39.00m, cart.Items.Single(item => item.ProductId == 1).Subtotal);
        Assert.Equal(47.50m, cart.Total);
    }

    [Fact]
    public void CartUpdatesQuantityAndRemovesItemAtZero()
    {
        var cart = new Cart();
        cart.Add(_catalog.Find(1)!);
        cart.SetQuantity(1, 4);
        Assert.Equal(78m, cart.Total);
        cart.SetQuantity(1, 0);
        Assert.Empty(cart.Items);
        Assert.Equal(0m, cart.Total);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    [InlineData(100)]
    public void CartRejectsInvalidQuantities(int quantity)
    {
        var cart = new Cart();
        cart.Add(_catalog.Find(1)!);
        Assert.Throws<ArgumentOutOfRangeException>(() => cart.SetQuantity(1, quantity));
    }

    [Fact]
    public void CartAddKeepsQuantityAtTenWhenLimitIsReached()
    {
        var cart = new Cart();
        var product = _catalog.Find(1)!;
        for (var i = 0; i < 10; i++) cart.Add(product);
        Assert.Equal(10, cart.Items.Single().Quantity);
        cart.Add(product);
        Assert.Equal(10, cart.Items.Single().Quantity);
    }

    [Fact]
    public void CartAcceptsQuantityOfTen()
    {
        var cart = new Cart();
        cart.Add(_catalog.Find(1)!);
        cart.SetQuantity(1, 10);
        Assert.Equal(10, cart.Items.Single().Quantity);
    }

    [Fact]
    public void CartRejectsElevenAndKeepsPreviousQuantity()
    {
        var cart = new Cart();
        cart.Add(_catalog.Find(1)!);
        cart.SetQuantity(1, 4);
        Assert.Throws<ArgumentOutOfRangeException>(() => cart.SetQuantity(1, 11));
        Assert.Equal(4, cart.Items.Single().Quantity);
    }

    [Fact]
    public void CannotCreateAnOrderForAnEmptyCart()
    {
        var service = new OrderService();
        Assert.Throws<InvalidOperationException>(() => service.Create("cliente-1", new Cart(), "empty-key"));
    }

    [Fact]
    public void OrderKeepsPriceAndLineDetailsAsTheyWereAtPurchase()
    {
        var service = new OrderService();
        var cart = new Cart();
        cart.Add(_catalog.Find(2)!);
        cart.SetQuantity(2, 3);
        var order = service.Create("cliente-1", cart, "history-key");
        cart.Clear();
        Assert.Equal(25.50m, order.Total);
        Assert.Equal(8.50m, order.Items.Single().UnitPrice);
        Assert.Equal(3, order.Items.Single().Quantity);
        Assert.Equal("Pagado (simulado)", order.Status);
        Assert.Equal(order.Id, service.FindForCustomer(order.Id, "cliente-1")!.Id);
    }

    [Fact]
    public void RepeatingTheSameConfirmationDoesNotDuplicateTheOrder()
    {
        var service = new OrderService();
        var cart = new Cart();
        cart.Add(_catalog.Find(1)!);
        var first = service.Create("cliente-1", cart, "once");
        cart.Clear();
        var repeated = service.Create("cliente-1", cart, "once");
        Assert.Equal(first.Id, repeated.Id);
        Assert.Single(service.GetForCustomer("cliente-1"));
    }

    [Fact]
    public void CustomersCanOnlyListOrFindTheirOwnOrders()
    {
        var service = new OrderService();
        var cart = new Cart();
        cart.Add(_catalog.Find(3)!);
        var order = service.Create("cliente-1", cart, "owner-key");
        Assert.Single(service.GetForCustomer("cliente-1"));
        Assert.Empty(service.GetForCustomer("cliente-2"));
        Assert.Null(service.FindForCustomer(order.Id, "cliente-2"));
    }

    [Fact]
    public void OnlyTheTwoDocumentedDemoCustomersCanSignIn()
    {
        var accounts = new DemoAccountService();
        Assert.Equal("cliente-1", accounts.Validate("CLIENTE@doceolab.com", "Pass@word1")!.Id);
        Assert.Equal("cliente-2", accounts.Validate("admin@doceolab.com", "Pass@word1")!.Id);
        Assert.Null(accounts.Validate("admin@doceolab.com", "admin123"));
    }
}
