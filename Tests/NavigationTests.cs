using Microsoft.Playwright;
using NUnit.Framework;

namespace PlaywrightTests.Tests;

[Parallelizable(ParallelScope.Self)]
public class NavigationTests : PageTest
{
    private const string BaseUrl = "https://example.com";

    [Test]
    public async Task ShouldNavigateUsingLinks()
    {
        // Arrange & Act
        await Page.GotoAsync(BaseUrl);

        // Act
        await Page.ClickAsync("a:has-text('About')");

        // Assert
        await Expect(Page).ToHaveURLAsync(new RegexPatternMatcher(new Regex(".*/about")));
    }

    [Test]
    public async Task ShouldNavigateUsingBackButton()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);

        // Act
        await Page.ClickAsync("a:has-text('About')");
        await Page.GoBackAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(BaseUrl);
    }

    [Test]
    public async Task ShouldNavigateUsingForwardButton()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);
        await Page.ClickAsync("a:has-text('About')");

        // Act
        await Page.GoBackAsync();
        await Page.GoForwardAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(new RegexPatternMatcher(new Regex(".*/about")));
    }

    [Test]
    public async Task ShouldWaitForNavigation()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);

        // Act
        var navigationTask = Page.WaitForNavigationAsync();
        await Page.ClickAsync("a:has-text('Product')");
        var response = await navigationTask;

        // Assert
        Assert.That(response?.Status, Is.EqualTo(200));
        await Expect(Page).ToHaveURLAsync(new RegexPatternMatcher(new Regex(".*/product")));
    }

    [Test]
    public async Task ShouldReloadPage()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);
        var initialText = await Page.TextContentAsync("h1");

        // Act
        await Page.ReloadAsync();

        // Assert
        var reloadedText = await Page.TextContentAsync("h1");
        Assert.That(reloadedText, Is.EqualTo(initialText));
    }

    [Test]
    public async Task ShouldHandlePageTimeout()
    {
        // Assert - Should throw timeout exception
        Assert.ThrowsAsync<PlaywrightException>(async () =>
        {
            await Page.GotoAsync("https://invalid-url-that-does-not-exist.example.com", 
                new PageGotoOptions { Timeout = 5000 });
        });
    }

    [Test]
    public async Task ShouldVerifyPageContent()
    {
        // Arrange & Act
        await Page.GotoAsync(BaseUrl);

        // Assert
        var content = await Page.ContentAsync();
        Assert.That(content, Does.Contain("</html>"));
    }

    [Test]
    public async Task ShouldGetPageUrl()
    {
        // Arrange & Act
        await Page.GotoAsync($"{BaseUrl}/about");

        // Assert
        Assert.That(Page.Url, Does.Contain("/about"));
    }

    [Test]
    public async Task ShouldWaitForUrlPattern()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);

        // Act & Assert
        var urlPattern = new RegexPatternMatcher(new Regex("example\\.com"));
        await Expect(Page).ToHaveURLAsync(urlPattern);
    }
}
