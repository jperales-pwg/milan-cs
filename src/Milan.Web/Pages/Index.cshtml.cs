using Milan.Web.Models;
using Milan.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Milan.Web.Pages;
public sealed class IndexModel(CatalogService catalog) : PageModel
{
    public IReadOnlyList<Product> Products { get; private set; } = [];
    public IReadOnlyList<string> Brands { get; private set; } = [];
    public IReadOnlyList<string> Types { get; private set; } = [];
    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)] public string Brand { get; set; } = "";
    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)] public string Type { get; set; } = "";

    public void OnGet()
    {
        var allProducts = catalog.GetAll();
        Brands = DistinctSorted(allProducts, product => product.Brand);
        Types = DistinctSorted(allProducts, product => product.Type);
        Products = allProducts
            .Where(product => string.IsNullOrEmpty(Brand) || string.Equals(product.Brand, Brand, StringComparison.OrdinalIgnoreCase))
            .Where(product => string.IsNullOrEmpty(Type) || string.Equals(product.Type, Type, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    private static string[] DistinctSorted(IEnumerable<Product> products, Func<Product, string> selector) => products.Select(selector).Distinct().OrderBy(value => value).ToArray();
}
