#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PlaywrightTests.Helpers;

/// <summary>
/// Base test class providing common setup and teardown functionality.
/// Inherits from PageTest to get Page fixture automatically.
/// </summary>
public abstract class BasePlaywrightTest : PageTest
{
    protected string BaseUrl { get; set; } = "https://example.com";
    protected IBrowserContext? BrowserContext { get; set; }

    /// <summary>
    /// Runs before each test. Override for custom setup.
    /// </summary>
    [SetUp]
    public virtual async Task InitializeTest()
    {
        // Log test name
        TestContext.WriteLine($"Starting test: {TestContext.CurrentContext.Test.Name}");
        
        // Can add additional setup here
        await Task.CompletedTask;
    }

    /// <summary>
    /// Runs after each test. Override for custom cleanup.
    /// </summary>
    [TearDown]
    public virtual async Task CleanupTest()
    {
        TestContext.WriteLine($"Test completed: {TestContext.CurrentContext.Test.Name}");

        // Capture screenshot on failure
        if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            await CaptureScreenshotAsync("failure");
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Captures a screenshot and saves it to the output directory.
    /// </summary>
    protected async Task CaptureScreenshotAsync(string suffix = "")
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var testName = TestContext.CurrentContext.Test.Name;
        var screenshotName = string.IsNullOrEmpty(suffix)
            ? $"{testName}_{timestamp}.png"
            : $"{testName}_{suffix}_{timestamp}.png";

        var screenshotPath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "Screenshots",
            screenshotName
        );

        Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });

        TestContext.WriteLine($"Screenshot saved: {screenshotPath}");
    }

    /// <summary>
    /// Navigates to a URL and waits for page to load.
    /// </summary>
    protected async Task NavigateToAsync(string path = "")
    {
        var url = string.IsNullOrEmpty(path) ? BaseUrl : $"{BaseUrl}/{path.TrimStart('/')}";
        await Page.GotoAsync(url);
    }

    /// <summary>
    /// Waits for an element and scrolls it into view.
    /// </summary>
    protected async Task ScrollToElementAsync(string selector)
    {
        var element = Page.Locator(selector);
        await element.ScrollIntoViewIfNeededAsync();
    }

    /// <summary>
    /// Sets up common wait conditions.
    /// </summary>
    protected async Task WaitForLoadingToCompleteAsync()
    {
        await Page.WaitForSelectorAsync(".loading-spinner", 
            new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden });
    }

    /// <summary>
    /// Gets page title.
    /// </summary>
    protected async Task<string> GetPageTitleAsync()
    {
        return await Page.TitleAsync();
    }

    /// <summary>
    /// Waits for element and gets its text content.
    /// </summary>
    protected async Task<string?> GetElementTextAsync(string selector)
    {
        var element = Page.Locator(selector);
        await element.WaitForAsync();
        return await element.TextContentAsync();
    }

    /// <summary>
    /// Verifies element is visible and contains expected text.
    /// </summary>
    protected async Task VerifyElementTextAsync(string selector, string expectedText)
    {
        var element = Page.Locator(selector);
        await Expect(element).ToContainTextAsync(expectedText);
    }

    /// <summary>
    /// Executes JavaScript on the page and converts result to specified type.
    /// Note: Returns JsonElement from Playwright - may need manual conversion
    /// </summary>
    protected async Task<object?> ExecuteScriptAsync(string script, object? arg = null)
    {
        try
        {
            return arg == null
                ? await Page.EvaluateAsync(script)
                : await Page.EvaluateAsync(script, arg);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Clicks on an element and waits for navigation if specified.
    /// </summary>
    protected async Task ClickAndWaitForNavigationAsync(string selector)
    {
        var navigationTask = Page.WaitForNavigationAsync();
        await Page.ClickAsync(selector);
        await navigationTask;
    }

    /// <summary>
    /// Gets all visible text from the page.
    /// </summary>
    protected async Task<string> GetPageTextAsync()
    {
        return await Page.TextContentAsync("body") ?? string.Empty;
    }

    /// <summary>
    /// Checks if element exists and is visible.
    /// </summary>
    protected async Task<bool> IsElementVisibleAsync(string selector)
    {
        try
        {
            return await Page.Locator(selector).IsVisibleAsync();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Waits for element with custom timeout.
    /// </summary>
    protected async Task WaitForElementAsync(string selector, int timeoutMs = 5000)
    {
        await Page.WaitForSelectorAsync(selector, 
            new PageWaitForSelectorOptions { Timeout = timeoutMs });
    }
}
