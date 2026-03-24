using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using PlaywrightTests.Helpers;

namespace PlaywrightTests.Tests;

/// <summary>
/// Quick start tests using BasePlaywrightTest for common functionality.
/// Perfect for beginners learning Playwright with C#.
/// </summary>
[Parallelizable(ParallelScope.Self)]
public class QuickStartTests : BasePlaywrightTest
{
    [Test]
    public async Task QuickStart_NavigateAndVerifyTitle()
    {
        // Navigate to website
        await NavigateToAsync();
        
        // Get and verify title
        var title = await GetPageTitleAsync();
        Assert.That(title, Does.Contain("Example"));
    }

    [Test]
    public async Task QuickStart_ClickButtonAndVerifyContent()
    {
        await NavigateToAsync();

        // Click a button
        await Page.ClickAsync("button:has-text('Get Started')");

        // Verify content appeared
        var description = await GetElementTextAsync("h2");
        Assert.That(description, Is.Not.Empty);
    }

    [Test]
    public async Task QuickStart_FillAndSubmitSimpleForm()
    {
        await NavigateToAsync("form");

        // Fill input field
        await Page.FillAsync("input[name='email']", "test@example.com");

        // Verify it was filled
        var value = await Page.InputValueAsync("input[name='email']");
        Assert.That(value, Is.EqualTo("test@example.com"));

        // Submit form
        await Page.ClickAsync("button[type='submit']");

        // Wait for success message
        await WaitForElementAsync(".success-message");
    }

    [Test]
    public async Task QuickStart_CheckVisibleElements()
    {
        await NavigateToAsync();

        // Check if multiple elements are visible
        var visible = await IsElementVisibleAsync(".header");
        Assert.That(visible, Is.True);

        var footerVisible = await IsElementVisibleAsync(".footer");
        Assert.That(footerVisible, Is.True);
    }

    [Test]
    public async Task QuickStart_GetAndVerifyText()
    {
        await NavigateToAsync();

        // Get text from element
        var heading = await GetElementTextAsync("h1");
        Assert.That(heading, Does.Contain("Welcome"));
    }

    [Test]
    public async Task QuickStart_InteractWithDropdown()
    {
        await NavigateToAsync("form");

        // Select option from dropdown
        await Page.SelectOptionAsync("select", "option-2");

        // Verify selection
        var selected = await Page.InputValueAsync("select");
        Assert.That(selected, Is.EqualTo("option-2"));
    }

    [Test]
    public async Task QuickStart_CheckAndUncheckCheckbox()
    {
        await NavigateToAsync("form");

        var checkbox = Page.Locator("input[type='checkbox']");

        // Check the checkbox
        await checkbox.CheckAsync();
        await Expect(checkbox).ToBeCheckedAsync();

        // Uncheck the checkbox
        await checkbox.UncheckAsync();
        await Expect(checkbox).Not.ToBeCheckedAsync();
    }

    [Test]
    public async Task QuickStart_CountElements()
    {
        await NavigateToAsync();

        // Count items in list
        var items = Page.Locator(".list-item");
        int count = await items.CountAsync();

        TestResultHelper.LogTestInfo($"Found {count} items");
        Assert.That(count, Is.GreaterThan(0));
    }

    [Test]
    public async Task QuickStart_TypeAndClear()
    {
        await NavigateToAsync("form");

        var input = Page.Locator("input[type='text']");

        // Type text
        await input.TypeAsync("Hello World");
        await Expect(input).ToHaveValueAsync("Hello World");

        // Clear text
        await input.FillAsync("");
        await Expect(input).ToHaveValueAsync("");
    }

    [Test]
    public async Task QuickStart_WaitForElement()
    {
        await NavigateToAsync();

        // Wait for element to appear (with 10 second timeout)
        try
        {
            await WaitForElementAsync(".dynamic-content", 10000);
            TestResultHelper.LogTestInfo("Dynamic content appeared");
        }
        catch
        {
            TestResultHelper.LogTestWarning("Dynamic content did not appear");
        }
    }

    [Test]
    public async Task QuickStart_VerifyPageUrl()
    {
        await NavigateToAsync("about");

        // Verify URL contains expected path
        Assert.That(Page.Url, Does.Contain("/about"));
    }

    [Test]
    public async Task QuickStart_MultipleAssertions()
    {
        await NavigateToAsync();

        var button = Page.Locator("button:first-of-type");

        // Multiple assertions
        await Expect(button).ToBeVisibleAsync();
        await Expect(button).ToBeEnabledAsync();
        await Expect(button).ToContainTextAsync("Click");
    }

    [Test]
    public async Task QuickStart_TakeScreenshotOnSuccess()
    {
        await NavigateToAsync();

        // Do something
        await Page.ClickAsync("button");

        // Take screenshot
        await CaptureScreenshotAsync("success");
    }
}
