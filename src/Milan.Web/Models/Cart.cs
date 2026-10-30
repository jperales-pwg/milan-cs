namespace Milan.Web.Models;

public sealed class Cart
{
    private const int MaxQuantity = 10;
    public List<CartItem> Items { get; set; } = [];
    public decimal Total => Items.Sum(item => item.UnitPrice * item.Quantity);

    public void Add(Product product)
    {
        var item = Items.FirstOrDefault(item => item.ProductId == product.Id);
        if (item is null)
            Items.Add(new CartItem(product.Id, product.Name, product.Description, product.ImagePath, product.Price, 1));
        else if (item.Quantity < MaxQuantity)
            item.Quantity++;
    }

    public void SetQuantity(int productId, int quantity)
    {
        if (quantity < 0 || quantity > MaxQuantity) throw new ArgumentOutOfRangeException(nameof(quantity));
        var item = Items.FirstOrDefault(item => item.ProductId == productId);
        if (item is null) return;
        if (quantity == 0) Items.Remove(item);
        else item.Quantity = quantity;
    }

    public void Remove(int productId) => Items.RemoveAll(item => item.ProductId == productId);
    public void Clear() => Items.Clear();
}

public sealed class CartItem(int productId, string name, string description, string imagePath, decimal unitPrice, int quantity)
{
    public int ProductId { get; set; } = productId;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public string ImagePath { get; set; } = imagePath;
    public decimal UnitPrice { get; set; } = unitPrice;
    public int Quantity { get; set; } = quantity;
    public decimal Subtotal => UnitPrice * Quantity;
}
