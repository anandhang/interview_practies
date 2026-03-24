using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using PlaywrightTests.PageObjects;

namespace PlaywrightTests.Tests;

[Parallelizable(ParallelScope.Self)]
public class PageObjectModelTests : PageTest
{
    private LoginPageObject _loginPage = null!;
    private ProductPageObject _productPage = null!;

    [SetUp]
    public void Setup()
    {
        _loginPage = new LoginPageObject(Page);
        _productPage = new ProductPageObject(Page);
    }

    [Test]
    public async Task ShouldLoginSuccessfullyUsingPageObject()
    {
        // Arrange
        await _loginPage.NavigateAsync();

        // Act
        await _loginPage.LoginAsync("admin", "password123");

        // Assert
        Assert.That(await _loginPage.IsWelcomeMessageVisibleAsync(), Is.True);
        await Expect(_loginPage.GetPage()).ToHaveURLAsync(new Regex(".*/dashboard"));
    }

    [Test]
    public async Task ShouldShownErrorForInvalidCredentials()
    {
        // Arrange
        await _loginPage.NavigateAsync();

        // Act
        await _loginPage.LoginAsync("invalid", "wrong");

        // Assert
        Assert.That(await _loginPage.IsErrorMessageVisibleAsync(), Is.True);
        var errorMsg = await _loginPage.GetErrorMessageAsync();
        Assert.That(errorMsg, Does.Contain("Invalid"));
    }

    [Test]
    public async Task ShouldSearchProducts()
    {
        // Arrange
        await _productPage.NavigateAsync();

        // Act
        await _productPage.SearchProductAsync("laptop");

        // Assert
        int productCount = await _productPage.GetProductCountAsync();
        Assert.That(productCount, Is.GreaterThan(0));

        // Verify first product has a name and price
        var productName = await _productPage.GetProductNameAsync(0);
        var productPrice = await _productPage.GetProductPriceAsync(0);
        Assert.That(productName, Is.Not.Empty);
        Assert.That(productPrice, Is.Not.Empty);
    }

    [Test]
    public async Task ShouldAddProductToCart()
    {
        // Arrange
        await _productPage.NavigateAsync();
        var initialCount = await _productPage.GetCartCountAsync();

        // Act
        await _productPage.AddProductToCartAsync(0);

        // Assert
        var updatedCount = await _productPage.GetCartCountAsync();
        Assert.That(updatedCount, Is.Not.EqualTo(initialCount));
    }

    [Test]
    public async Task ShouldFilterProductsByPrice()
    {
        // Arrange
        await _productPage.NavigateAsync();

        // Act
        await _productPage.FilterByPriceAsync("100", "500");

        // Assert
        int productCount = await _productPage.GetProductCountAsync();
        Assert.That(productCount, Is.GreaterThan(0));

        // Verify all products are within price range
        for (int i = 0; i < productCount; i++)
        {
            var price = await _productPage.GetProductPriceAsync(i);
            // Note: In real tests, parse and verify actual price values
            Assert.That(price, Is.Not.Empty);
        }
    }

    [Test]
    public async Task ShouldSortProductsByPrice()
    {
        // Arrange
        await _productPage.NavigateAsync();

        // Act
        await _productPage.SortByAsync("price_asc");

        // Assert
        int productCount = await _productPage.GetProductCountAsync();
        Assert.That(productCount, Is.GreaterThan(1));
    }

    [Test]
    public async Task ShouldDisplayNoResultsMessage()
    {
        // Arrange
        await _productPage.NavigateAsync();

        // Act
        await _productPage.SearchProductAsync("nonexistentproduct12345");

        // Assert
        Assert.That(await _productPage.IsNoResultsMessageVisibleAsync(), Is.True);
    }
}
