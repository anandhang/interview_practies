using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PlaywrightTests.Tests;

[Parallelizable(ParallelScope.Self)]
public class BasicTests : PageTest
{
    private const string BaseUrl = "https://example.com";

    [TestCase("BT-001", "Navigation", "test1")]
    [Category("sampletest1")]
    [Test]
    public async Task ShouldNavigateToPageAndVerifyTitle()
    {
        // Arrange & Act
        await Page.GotoAsync(BaseUrl);

        // Assert
        await Expect(Page).ToHaveTitleAsync(new Regex("Example"));
    }

    [TestCase("BT-002", "UI Interaction", "test1")]
    [Category("sampletest1")]
    [Test]
    public async Task ShouldDisplayAndInteractWithButton()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);

        // Act
        var button = Page.Locator("button:has-text('Click me')");

        // Assert
        await Expect(button).ToBeVisibleAsync();

        // Act
        await button.ClickAsync();
    }

    [TestCase("BT-003", "Element Verification", "test1")]
    [Category("sampletest1")]
    [Test]
    public async Task ShouldVerifyElementText()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);

        // Act & Assert
        await Expect(Page.Locator("h1")).ToContainTextAsync("Welcome");
    }

    [TestCase("BT-004", "Attribute Verification", "test1")]
    [Category("sampletest1")]
    [Test]
    public async Task ShouldVerifyElementAttributeValue()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);

        // Act & Assert
        await Expect(Page.Locator("a[href='/about']")).ToHaveAttributeAsync("href", "/about");
    }

    [Test]
    public async Task ShouldVerifyMultipleElements()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);

        // Act
        var items = Page.Locator(".item");
        int count = await items.CountAsync();

        // Assert
        Assert.That(count, Is.GreaterThan(0), "At least one item should exist");

        // Verify each item has content
        for (int i = 0; i < count; i++)
        {
            var textContent = await items.Nth(i).TextContentAsync();
            Assert.That(textContent, Is.Not.Empty, $"Item {i} should have text content");
        }
    }

    [Test]
    public async Task ShouldWaitForElementVisibility()
    {
        // Arrange & Act
        await Page.GotoAsync(BaseUrl);

        // Wait for loading spinner to disappear
        await Page.WaitForSelectorAsync(".loading-spinner", 
            new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden });

        var content = Page.Locator("text=Content loaded");

        // Assert
        await Expect(content).ToBeVisibleAsync();
    }

    [Test]
    public async Task ShouldTakeScreenshot()
    {
        // Arrange & Act
        await Page.GotoAsync(BaseUrl);
        
        var screenshotPath = Path.Combine(
            TestContext.CurrentContext.TestDirectory, 
            "Screenshots", 
            "example.png");
        
        Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath));

        // Act
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });

        // Assert
        Assert.That(File.Exists(screenshotPath), "Screenshot should be created");
    }

    [Test]
    public async Task ShouldHandleKeyboardInteractions()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);
        var input = Page.Locator("input[type='text']");

        // Act
        await input.FocusAsync();
        await Page.Keyboard.TypeAsync("Hello World");

        // Assert
        await Expect(input).ToHaveValueAsync("Hello World");

        // Act - Clear using keyboard
        await Page.Keyboard.PressAsync("Control+A");
        await Page.Keyboard.PressAsync("Delete");

        // Assert
        await Expect(input).ToHaveValueAsync("");
    }

    [Test]
    public async Task ShouldHandleMouseInteractions()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);

        // Act - Hover over element
        await Page.Locator("button").HoverAsync();

        var button = Page.Locator("button");

        // Assert
        await Expect(button).ToHaveClassAsync(new Regex("hover"));

        // Act - Double click
        await button.DblClickAsync();
    }

    [Test]
    public async Task ShouldVerifyImageLoaded()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);

        // Act
        var image = Page.Locator("img");

        // Assert - Check if visible
        await Expect(image).ToBeVisibleAsync();

        // Check if image has src attribute
        var src = await image.GetAttributeAsync("src");
        Assert.That(src, Is.Not.Empty.And.Not.Null);
    }
}
