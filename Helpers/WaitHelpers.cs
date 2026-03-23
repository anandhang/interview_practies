using Microsoft.Playwright;

namespace PlaywrightTests.Helpers;

/// <summary>
/// Custom wait helpers and conditions.
/// </summary>
public static class WaitHelpers
{
    /// <summary>
    /// Waits for element to be clickable (visible and enabled).
    /// </summary>
    public static async Task WaitForClickableAsync(this IPage page, string selector, int timeout = 5000)
    {
        var element = page.Locator(selector);
        await element.WaitForAsync(new LocatorWaitForOptions { Timeout = timeout });
        while (!await element.IsEnabledAsync())
        {
            await Task.Delay(100);
        }
    }

    /// <summary>
    /// Waits for page to load (document ready).
    /// </summary>
    public static async Task WaitForPageLoadAsync(this IPage page, int timeout = 5000)
    {
        await page.WaitForFunctionAsync(
            "() => document.readyState === 'complete'",
            new PageWaitForFunctionOptions { Timeout = timeout }
        );
    }

    /// <summary>
    /// Waits for specific text to appear on page.
    /// </summary>
    public static async Task WaitForTextAsync(this IPage page, string text, int timeout = 5000)
    {
        var selector = $"text={System.Net.WebUtility.HtmlEncode(text)}";
        await page.WaitForSelectorAsync(selector, 
            new PageWaitForSelectorOptions { Timeout = timeout });
    }

    /// <summary>
    /// Waits for element count to match expected.
    /// </summary>
    public static async Task WaitForElementCountAsync(
        this IPage page, 
        string selector, 
        int expectedCount, 
        int timeout = 5000)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        while (stopwatch.ElapsedMilliseconds < timeout)
        {
            var count = await page.Locator(selector).CountAsync();
            if (count == expectedCount)
                return;
            
            await Task.Delay(100);
        }

        throw new TimeoutException($"Expected {expectedCount} elements, timeout after {timeout}ms");
    }

    /// <summary>
    /// Waits for value to be set in input field.
    /// </summary>
    public static async Task WaitForInputValueAsync(this IPage page, string selector, string value, int timeout = 5000)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        while (stopwatch.ElapsedMilliseconds < timeout)
        {
            var currentValue = await page.InputValueAsync(selector);
            if (currentValue == value)
                return;
            
            await Task.Delay(100);
        }

        throw new TimeoutException($"Input value '{value}' not set after {timeout}ms");
    }

    /// <summary>
    /// Waits for element attributes to have specific value.
    /// </summary>
    public static async Task WaitForAttributeAsync(
        this IPage page, 
        string selector, 
        string attributeName, 
        string expectedValue, 
        int timeout = 5000)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var element = page.Locator(selector);
        
        while (stopwatch.ElapsedMilliseconds < timeout)
        {
            var attrValue = await element.GetAttributeAsync(attributeName);
            if (attrValue == expectedValue)
                return;
            
            await Task.Delay(100);
        }

        throw new TimeoutException($"Attribute '{attributeName}' not set to '{expectedValue}' after {timeout}ms");
    }

    /// <summary>
    /// Waits for specific CSS class to be added/removed.
    /// </summary>
    public static async Task WaitForClassAsync(
        this IPage page, 
        string selector, 
        string className, 
        bool shouldHaveClass = true, 
        int timeout = 5000)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var element = page.Locator(selector);
        
        while (stopwatch.ElapsedMilliseconds < timeout)
        {
            var classes = await element.GetAttributeAsync("class");
            bool hasClass = classes?.Contains(className) ?? false;
            
            if (hasClass == shouldHaveClass)
                return;
            
            await Task.Delay(100);
        }

        var action = shouldHaveClass ? "have" : "not have";
        throw new TimeoutException($"Element should {action} class '{className}' after {timeout}ms");
    }

    /// <summary>
    /// Waits for JavaScript condition to be true.
    /// </summary>
    public static async Task WaitForConditionAsync(
        this IPage page, 
        string jsExpression, 
        int timeout = 5000)
    {
        await page.WaitForFunctionAsync(jsExpression, 
            new PageWaitForFunctionOptions { Timeout = timeout });
    }

    /// <summary>
    /// Waits for URL to change.
    /// </summary>
    public static async Task WaitForUrlChangeAsync(this IPage page, string originalUrl, int timeout = 5000)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        while (stopwatch.ElapsedMilliseconds < timeout)
        {
            if (page.Url != originalUrl)
                return;
            
            await Task.Delay(100);
        }

        throw new TimeoutException($"URL did not change from {originalUrl} within {timeout}ms");
    }

    /// <summary>
    /// Waits for element to no longer exist in DOM.
    /// </summary>
    public static async Task WaitForElementNotFoundAsync(this IPage page, string selector, int timeout = 5000)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        while (stopwatch.ElapsedMilliseconds < timeout)
        {
            var count = await page.Locator(selector).CountAsync();
            if (count == 0)
                return;
            
            await Task.Delay(100);
        }

        throw new TimeoutException($"Element '{selector}' still exists after {timeout}ms");
    }

    /// <summary>
    /// Waits for multiple elements.
    /// </summary>
    public static async Task WaitForElementsAsync(this IPage page, string selector, int minCount = 1, int timeout = 5000)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        while (stopwatch.ElapsedMilliseconds < timeout)
        {
            var count = await page.Locator(selector).CountAsync();
            if (count >= minCount)
                return;
            
            await Task.Delay(100);
        }

        throw new TimeoutException($"Expected at least {minCount} elements matching '{selector}' within {timeout}ms");
    }

    /// <summary>
    /// Waits for API response matching criteria.
    /// </summary>
    public static async Task<IResponse> WaitForApiResponseAsync(
        this IPage page, 
        string urlPattern, 
        int timeout = 10000)
    {
        var responseTask = page.WaitForResponseAsync(response =>
            response.Url.Contains(urlPattern) && response.Status == 200,
            new PageWaitForResponseOptions { Timeout = timeout }
        );

        return await responseTask;
    }

    /// <summary>
    /// Waits until network is idle.
    /// </summary>
    public static async Task WaitForNetworkIdleAsync(this IPage page, int timeout = 5000)
    {
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, 
            new PageWaitForLoadStateOptions { Timeout = timeout });
    }
}

/// <summary>
/// Retry helper for flaky tests.
/// </summary>
public static class RetryHelpers
{
    /// <summary>
    /// Executes an async action with retry logic.
    /// </summary>
    public static async Task RetryAsync(
        Func<Task> action, 
        int maxAttempts = 3, 
        int delayMs = 1000)
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await action();
                return;
            }
            catch when (attempt < maxAttempts)
            {
                await Task.Delay(delayMs);
            }
        }

        // Last attempt - let exception throw
        await action();
    }

    /// <summary>
    /// Executes an async function with retry logic and returns result.
    /// </summary>
    public static async Task<T> RetryAsync<T>(
        Func<Task<T>> action, 
        int maxAttempts = 3, 
        int delayMs = 1000)
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await action();
            }
            catch when (attempt < maxAttempts)
            {
                await Task.Delay(delayMs);
            }
        }

        // Last attempt - let exception throw
        return await action();
    }
}

/// <summary>
/// Assertion helpers.
/// </summary>
public static class AssertionHelpers
{
    /// <summary>
    /// Asserts text contains in case-insensitive manner.
    /// </summary>
    public static void ContainsIgnoreCase(string? actual, string expected, string? message = null)
    {
        if (string.IsNullOrEmpty(actual) || !actual.Contains(expected, StringComparison.OrdinalIgnoreCase))
        {
            throw new AssertionException(
                message ?? $"Expected text to contain '{expected}' (case-insensitive), but got '{actual}'");
        }
    }

    /// <summary>
    /// Asserts URL contains path.
    /// </summary>
    public static void UrlContainsPath(string url, string path)
    {
        if (!url.Contains(path, StringComparison.OrdinalIgnoreCase))
        {
            throw new AssertionException($"Expected URL to contain '{path}', but got '{url}'");
        }
    }

    /// <summary>
    /// Asserts number is in range.
    /// </summary>
    public static void InRange(int value, int min, int max, string? message = null)
    {
        if (value < min || value > max)
        {
            throw new AssertionException(
                message ?? $"Expected value {value} to be between {min} and {max}");
        }
    }
}
