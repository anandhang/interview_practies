#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PlaywrightTests.Tests;

[Parallelizable(ParallelScope.Self)]
public class AdvancedTests : PageTest
{
    private const string BaseUrl = "https://example.com";

    [Test]
    public async Task ShouldHandleMultiplePopups()
    {
        // Arrange
        var popups = new List<IPage>();
        Page.Popup += (_, popup) =>
        {
            popups.Add(popup);
        };

        await Page.GotoAsync(BaseUrl);

        // Act
        await Page.ClickAsync("button:has-text('Open Popup')");
        await Task.Delay(500);

        // Assert
        Assert.That(popups, Has.Count.GreaterThan(0));

        // Act - Close popup
        await popups[0].CloseAsync();
    }

    [Test]
    public async Task ShouldHandleDialogs()
    {
        // Arrange
        string? dialogMessage = null;
        Page.Dialog += async (_, dialog) =>
        {
            dialogMessage = dialog.Message;
            await dialog.AcceptAsync();
        };

        await Page.GotoAsync(BaseUrl);

        // Act
        await Page.ClickAsync("button:has-text('Show Alert')");

        // Assert
        Assert.That(dialogMessage, Is.Not.Empty);
    }

    [Test]
    public async Task ShouldDownloadFile()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);

        // Act - Listen for download
        var downloadTask = Page.WaitForDownloadAsync();
        await Page.ClickAsync("a:has-text('Download PDF')");
        var download = await downloadTask;

        // Save to temp path
        var filePath = Path.Combine(Path.GetTempPath(), download.SuggestedFilename);
        await download.SaveAsAsync(filePath);

        // Assert
        Assert.That(File.Exists(filePath));

        // Cleanup
        File.Delete(filePath);
    }

    [Test]
    public async Task ShouldHandleMultipleWindows()
    {
        // Arrange
        var pages = new List<IPage> { Page };
        Page.Context.Page += (_, page) =>
        {
            pages.Add(page);
        };

        await Page.GotoAsync(BaseUrl);

        // Act
        await Page.EvaluateAsync("window.open('https://example.com/page2', '_blank')");
        await Task.Delay(500);

        // Assert
        Assert.That(pages, Has.Count.EqualTo(2));

        // Act
        if (pages.Count > 1)
        {
            await pages[1].CloseAsync();
        }
    }

    [Test]
    public async Task ShouldHandleIframes()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);

        // Get iframe
        var frameCount = Page.Frames.Count;

        // Act
        var iframe = Page.Frames.FirstOrDefault(f => f.Url.Contains("embedded"));

        // Assert
        Assert.That(iframe, Is.Not.Null, "Iframe should exist");

        // Act - Interact with iframe content
        if (iframe != null)
        {
            var button = iframe.Locator("button:has-text('Click')");
            await Expect(button).ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task ShouldHandleLocalStorage()
    {
        // Arrange & Act
        await Page.GotoAsync(BaseUrl);

        // Set localStorage
        await Page.EvaluateAsync(@"
            () => {
                localStorage.setItem('user', 'testuser');
                localStorage.setItem('theme', 'dark');
            }
        ");

        // Get localStorage
        var user = await Page.EvaluateAsync<string>("() => localStorage.getItem('user')");
        var theme = await Page.EvaluateAsync<string>("() => localStorage.getItem('theme')");

        // Assert
        Assert.That(user, Is.EqualTo("testuser"));
        Assert.That(theme, Is.EqualTo("dark"));
    }

    [Test]
    public async Task ShouldHandleSessionStorage()
    {
        // Arrange & Act
        await Page.GotoAsync(BaseUrl);

        // Set sessionStorage
        await Page.EvaluateAsync(@"
            () => {
                sessionStorage.setItem('sessionId', '12345');
                sessionStorage.setItem('token', 'abc123');
            }
        ");

        // Get sessionStorage
        var sessionId = await Page.EvaluateAsync<string>("() => sessionStorage.getItem('sessionId')");
        var token = await Page.EvaluateAsync<string>("() => sessionStorage.getItem('token')");

        // Assert
        Assert.That(sessionId, Is.EqualTo("12345"));
        Assert.That(token, Is.EqualTo("abc123"));
    }

    [Test]
    public async Task ShouldHandleConsoleMessages()
    {
        // Arrange
        var messages = new List<string>();
        Page.Console += (_, msg) =>
        {
            messages.Add(msg.Text);
        };

        await Page.GotoAsync(BaseUrl);

        // Act
        await Page.EvaluateAsync("() => console.log('Test message')");

        // Assert
        Assert.That(messages, Does.Contain("Test message"));
    }

    [Test]
    public async Task ShouldHandlePageErrors()
    {
        // Arrange
        var errors = new List<string>();
        Page.PageError += (_, exception) =>
        {
            // PageError receives PlaywrightException, not string
            errors.Add(exception.ToString() ?? "Unknown error");
        };

        await Page.GotoAsync(BaseUrl);

        // Act
        await Page.EvaluateAsync("() => throw new Error('Test error')");

        // Assert
        await Task.Delay(500);
        Assert.That(errors, Has.Count.GreaterThan(0));
    }

    // Note: Page.Metrics API is not available in current Playwright version
    // Skipping ShouldMeasurePagePerformance test - Metrics event not supported in this version

    [Test]
    public async Task ShouldHandleAutoComplete()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/search");

        var searchInput = Page.Locator("input[name='search']");

        // Act
        await searchInput.TypeAsync("test");

        // Wait for suggestions to appear
        var suggestions = Page.Locator(".suggestion-item");
        await Expect(suggestions.First).ToBeVisibleAsync();

        // Assert
        int count = await suggestions.CountAsync();
        Assert.That(count, Is.GreaterThan(0));

        // Act
        await suggestions.First.ClickAsync();

        // Assert - Value should be auto-filled
        var value = await searchInput.GetAttributeAsync("value");
        Assert.That(value, Is.Not.Empty);
    }

    [Test]
    public async Task ShouldHandleInfiniteScroll()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/feed");

        var initialItemCount = await Page.Locator(".feed-item").CountAsync();

        // Act - Scroll to bottom
        await Page.EvaluateAsync(@"
            () => {
                const element = document.querySelector('.feed-container');
                element.scrollTop = element.scrollHeight;
            }
        ");

        // Wait for new items to load
        await Page.WaitForFunctionAsync(
            $"() => document.querySelectorAll('.feed-item').length > {initialItemCount}",
            new PageWaitForFunctionOptions { Timeout = 5000 }
        );

        // Assert
        var finalItemCount = await Page.Locator(".feed-item").CountAsync();
        Assert.That(finalItemCount, Is.GreaterThan(initialItemCount));
    }

    [Test]
    public async Task ShouldHandleContextMenuAndRightClick()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);

        var element = Page.Locator("button");

        // Act
        await element.ClickAsync(new LocatorClickOptions { Button = MouseButton.Right });

        // Wait for context menu
        var contextMenu = Page.Locator(".context-menu");
        await Expect(contextMenu).ToBeVisibleAsync();

        // Assert
        var items = contextMenu.Locator(".menu-item");
        int itemCount = await items.CountAsync();
        Assert.That(itemCount, Is.GreaterThan(0));
    }
}
