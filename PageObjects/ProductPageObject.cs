#nullable enable
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace PlaywrightTests.PageObjects;

/// <summary>
/// Page Object Model for product listing page.
/// </summary>
public class ProductPageObject
{
    private readonly IPage _page;
    private const string BaseUrl = "https://example.com";

    // Locators
    private ILocator ProductItems => _page.Locator(".product-item");
    private ILocator AddToCartButton => _page.Locator("button:has-text('Add to Cart')");
    private ILocator CartBadge => _page.Locator(".cart-badge");
    private ILocator SearchInput => _page.Locator("input[placeholder='Search products']");
    private ILocator SearchButton => _page.Locator("button:has-text('Search')");
    private ILocator PriceFilter => _page.Locator("input[name='price']");
    private ILocator SortDropdown => _page.Locator("select[name='sort']");
    private ILocator NoResultsMessage => _page.Locator("text=No products found");

    public ProductPageObject(IPage page)
    {
        _page = page;
    }

    public async Task NavigateAsync()
    {
        await _page.GotoAsync($"{BaseUrl}/products");
    }

    public async Task SearchProductAsync(string productName)
    {
        await SearchInput.FillAsync(productName);
        await SearchButton.ClickAsync();
    }

    public async Task<int> GetProductCountAsync()
    {
        return await ProductItems.CountAsync();
    }

    public async Task<string?> GetProductNameAsync(int index)
    {
        return await ProductItems.Nth(index).Locator(".product-name").TextContentAsync();
    }

    public async Task<string?> GetProductPriceAsync(int index)
    {
        return await ProductItems.Nth(index).Locator(".price").TextContentAsync();
    }

    public async Task AddProductToCartAsync(int index)
    {
        var product = ProductItems.Nth(index);
        await product.Locator(AddToCartButton).ClickAsync();
    }

    public async Task<string?> GetCartCountAsync()
    {
        return await CartBadge.TextContentAsync();
    }

    public async Task FilterByPriceAsync(string minPrice, string maxPrice)
    {
        var minInput = _page.Locator("input[name='minPrice']");
        var maxInput = _page.Locator("input[name='maxPrice']");

        await minInput.FillAsync(minPrice);
        await maxInput.FillAsync(maxPrice);
        await _page.Locator("button:has-text('Apply Filter')").ClickAsync();
    }

    public async Task SortByAsync(string sortOption)
    {
        await SortDropdown.SelectOptionAsync(sortOption);
    }

    public async Task<bool> IsNoResultsMessageVisibleAsync()
    {
        return await NoResultsMessage.IsVisibleAsync();
    }

    public async Task ClickProductAsync(int index)
    {
        await ProductItems.Nth(index).ClickAsync();
    }

    public IPage GetPage()
    {
        return _page;
    }
}
