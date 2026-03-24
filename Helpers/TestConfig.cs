#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace PlaywrightTests.Helpers;

/// <summary>
/// Test configuration and utilities.
/// </summary>
public static class TestConfig
{
    public static class Browser
    {
        public const string Chromium = "chromium";
        public const string Firefox = "firefox";
        public const string WebKit = "webkit";
    }

    public static class Timeout
    {
        public const int Short = 5000;
        public const int Medium = 15000;
        public const int Long = 30000;
    }

    public static class Viewport
    {
        public const int Desktop_Width = 1920;
        public const int Desktop_Height = 1200;
        public const int Tablet_Width = 768;
        public const int Tablet_Height = 1024;
        public const int Mobile_Width = 375;
        public const int Mobile_Height = 667;
    }

    /// <summary>
    /// Gets base URL from environment or default.
    /// </summary>
    public static string GetBaseUrl()
    {
        return Environment.GetEnvironmentVariable("BASE_URL") ?? "https://example.com";
    }

    /// <summary>
    /// Gets browser type from environment or default to Chromium.
    /// </summary>
    public static string GetBrowserType()
    {
        return Environment.GetEnvironmentVariable("BROWSER_TYPE") ?? Browser.Chromium;
    }

    /// <summary>
    /// Gets headless mode setting from environment or default to true.
    /// </summary>
    public static bool GetHeadlessMode()
    {
        var value = Environment.GetEnvironmentVariable("HEADLESS");
        return string.IsNullOrEmpty(value) || value.ToLower() != "false";
    }

    /// <summary>
    /// Gets screenshot directory.
    /// </summary>
    public static string GetScreenshotDirectory()
    {
        var directory = Path.Combine(
            Directory.GetCurrentDirectory(),
            "test-screenshots"
        );
        Directory.CreateDirectory(directory);
        return directory;
    }

    /// <summary>
    /// Gets trace directory.
    /// </summary>
    public static string GetTraceDirectory()
    {
        var directory = Path.Combine(
            Directory.GetCurrentDirectory(),
            "test-traces"
        );
        Directory.CreateDirectory(directory);
        return directory;
    }

    /// <summary>
    /// Gets browser launch options.
    /// </summary>
    public static BrowserTypeLaunchOptions GetLaunchOptions()
    {
        return new BrowserTypeLaunchOptions
        {
            Headless = GetHeadlessMode(),
            SlowMo = GetHeadlessMode() ? 0 : 100, // Slow down headless mode
        };
    }

    /// <summary>
    /// Creates browser context options for desktop testing.
    /// </summary>
    public static BrowserNewContextOptions GetDesktopContextOptions()
    {
        return new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = Viewport.Desktop_Width, Height = Viewport.Desktop_Height },
            IgnoreHTTPSErrors = true,
            Locale = "en-US",
            TimezoneId = "America/New_York",
        };
    }

    /// <summary>
    /// Creates browser context options for mobile testing.
    /// </summary>
    public static BrowserNewContextOptions GetMobileContextOptions()
    {
        return new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = Viewport.Mobile_Width, Height = Viewport.Mobile_Height },
            IsMobile = true,
            HasTouch = true,
            DeviceScaleFactor = 2,
            IgnoreHTTPSErrors = true,
        };
    }

    /// <summary>
    /// Creates browser context options for tablet testing.
    /// </summary>
    public static BrowserNewContextOptions GetTabletContextOptions()
    {
        return new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = Viewport.Tablet_Width, Height = Viewport.Tablet_Height },
            IsMobile = true,
            HasTouch = true,
            IgnoreHTTPSErrors = true,
        };
    }
}

/// <summary>
/// Test result helper for logging and reporting.
/// </summary>
public static class TestResultHelper
{
    public static ITestResult? GetCurrentTestResult()
    {
        try
        {
            return TestContext.CurrentContext?.Result as ITestResult;
        }
        catch
        {
            return null;
        }
    }

    public static bool IsTestPassed()
    {
        try
        {
            var result = GetCurrentTestResult();
            return result != null && result.ResultState?.Status == TestStatus.Passed;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsTestFailed()
    {
        try
        {
            var result = GetCurrentTestResult();
            return result != null && result.ResultState?.Status == TestStatus.Failed;
        }
        catch
        {
            return false;
        }
    }

    public static void LogTestInfo(string message)
    {
        TestContext.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    public static void LogTestWarning(string message)
    {
        TestContext.WriteLine($"[WARNING] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    public static void LogTestError(string message)
    {
        TestContext.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
    }
}

/// <summary>
/// Locator helper methods.
/// </summary>
public static class LocatorExtensions
{
    /// <summary>
    /// Gets element if it exists, otherwise returns null.
    /// </summary>
    public static async Task<ILocator?> GetIfExistsAsync(this ILocator locator)
    {
        try
        {
            var count = await locator.CountAsync();
            return count > 0 ? locator : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Clicks element if visible.
    /// </summary>
    public static async Task ClickIfVisibleAsync(this ILocator locator)
    {
        if (await locator.IsVisibleAsync())
        {
            await locator.ClickAsync();
        }
    }

    /// <summary>
    /// Gets text content safely.
    /// </summary>
    public static async Task<string?> GetSafeTextContentAsync(this ILocator locator)
    {
        try
        {
            return await locator.TextContentAsync();
        }
        catch
        {
            return null;
        }
    }
}
