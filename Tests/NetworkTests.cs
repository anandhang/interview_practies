using Microsoft.Playwright;
using NUnit.Framework;
using System.Text.Json;

namespace PlaywrightTests.Tests;

[Parallelizable(ParallelScope.Self)]
public class NetworkTests : PageTest
{
    private const string BaseUrl = "https://example.com";

    [Test]
    public async Task ShouldCaptureNetworkResponse()
    {
        // Arrange
        var responseTask = Page.WaitForResponseAsync(response =>
            response.Url.Contains("/api/data") && response.Status == 200
        );

        // Act
        await Page.GotoAsync(BaseUrl);
        var response = await responseTask;

        // Assert
        Assert.That(response.Status, Is.EqualTo(200));
        var json = await response.JsonAsync();
        Assert.That(json, Is.Not.Null);
    }

    [Test]
    public async Task ShouldInterceptNetworkRequest()
    {
        // Arrange
        await Page.RouteAsync("**/api/users/**", async route =>
        {
            // Modify the request or response
            var response = await route.FetchAsync();
            await route.ContinueAsync();
        });

        // Act & Assert
        await Page.GotoAsync(BaseUrl);
        await Expect(Page.Locator("text=Users loaded")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ShouldAbortNetworkRequest()
    {
        // Arrange
        await Page.RouteAsync("**/*.png", route => route.AbortAsync());

        // Act
        await Page.GotoAsync(BaseUrl);

        // Assert - Images should not load
        var images = Page.Locator("img");
        int count = await images.CountAsync();
        for (int i = 0; i < count; i++)
        {
            var naturalWidth = await images.Nth(i).EvaluateAsync<int>("img => img.naturalWidth");
            Assert.That(naturalWidth, Is.EqualTo(0));
        }
    }

    [Test]
    public async Task ShouldMonitorAllNetworkRequests()
    {
        // Arrange
        var requests = new List<string>();
        Page.Request += (_, request) => requests.Add(request.Url);

        // Act
        await Page.GotoAsync(BaseUrl);

        // Assert
        Assert.That(requests, Is.Not.Empty);
        Assert.That(requests.Any(r => r.Contains("example.com")), Is.True);
    }

    [Test]
    public async Task ShouldMonitorNetworkErrors()
    {
        // Arrange
        var failedRequests = new List<string>();
        Page.RequestFailed += (_, request) => failedRequests.Add(request.Url);

        // Act
        await Page.RouteAsync("**/*.jpg", route => route.AbortAsync("failed"));
        await Page.GotoAsync(BaseUrl);
        await Task.Delay(1000);

        // Assert - May or may not have failed requests depending on page content
        // This is just an example
    }

    [Test]
    public async Task ShouldVerifyResponseHeaders()
    {
        // Arrange
        var responseTask = Page.WaitForResponseAsync(response =>
            response.Url.Contains("example.com") && response.Status == 200
        );

        // Act
        await Page.GotoAsync(BaseUrl);
        var response = await responseTask;

        // Assert
        var headers = await response.AllHeadersAsync();
        Assert.That(headers, Does.ContainKey("content-type"));
    }

    [Test]
    public async Task ShouldMeasureNetworkTiming()
    {
        // Arrange
        IResponse? navigationResponse = null;
        Page.Response += (_, response) =>
        {
            if (response.Url == BaseUrl)
                navigationResponse = response;
        };

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await Page.GotoAsync(BaseUrl);
        stopwatch.Stop();

        // Assert
        Assert.That(stopwatch.ElapsedMilliseconds, Is.GreaterThan(0));
        Assert.That(navigationResponse?.Status, Is.EqualTo(200));
    }

    [Test]
    public async Task ShouldWaitForApiResponse()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);

        // Act
        var responseTask = Page.WaitForResponseAsync(response =>
            response.Url.Contains("/api/search") && response.Status == 200
        );

        await Page.FillAsync("input[name='search']", "test");
        await Page.PressAsync("input[name='search']", "Enter");

        var response = await responseTask;

        // Assert
        var jsonData = await response.JsonAsync() as JsonElement?;
        Assert.That(jsonData, Is.Not.Null);
    }

    [Test]
    public async Task ShouldHandleNetworkTimeout()
    {
        // Arrange
        await Page.RouteAsync("**/*", async route =>
        {
            await Task.Delay(10000); // Simulate slow response
            await route.FetchAsync();
        });

        // Assert - Should timeout waiting for response
        Assert.ThrowsAsync<PlaywrightException>(async () =>
        {
            await Page.GotoAsync(BaseUrl, new PageGotoOptions { Timeout = 2000 });
        });
    }

    [Test]
    public async Task ShouldVerifyJsonResponseContent()
    {
        // Arrange
        var responseTask = Page.WaitForResponseAsync(response =>
            response.Url.Contains("/api/data") && response.Status == 200
        );

        // Act
        await Page.GotoAsync(BaseUrl);
        var response = await responseTask;
        var jsonData = await response.JsonAsync() as JsonElement?;

        // Assert
        Assert.That(jsonData?.ValueKind, Is.EqualTo(JsonValueKind.Object));
    }
}
