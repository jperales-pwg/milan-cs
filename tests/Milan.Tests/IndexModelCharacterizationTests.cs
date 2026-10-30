using Milan.Web.Pages;
using Milan.Web.Services;
using Xunit;

namespace Milan.Tests;

public sealed class IndexModelCharacterizationTests
{
    [Fact]
    public void OnGetBuildsBrandAndTypeListsWithExactValuesInOrder()
    {
        var page = new IndexModel(new CatalogService());
        page.OnGet();
        Assert.Equal(new[] { "AeroMILA", "AgroVuelo" }, page.Brands);
        Assert.Equal(new[] { "Fotografía", "Profesional", "Recreativo" }, page.Types);
    }
}
