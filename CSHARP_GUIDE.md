# Playwright C# Testing Guide

A comprehensive guide for creating and running Playwright tests using C# and NUnit.

## Project Structure

```
PlaywrightTests/
├── PlaywrightTests.csproj          # Project configuration
├── Tests/
│   ├── BasicTests.cs               # Basic element interactions
│   ├── FormTests.cs                # Form testing examples
│   ├── NavigationTests.cs          # Navigation and page state
│   ├── NetworkTests.cs             # Network and API testing
│   └── PageObjectModelTests.cs    # Tests using Page Object Model
├── PageObjects/
│   ├── LoginPageObject.cs         # Login page abstraction
│   └── ProductPageObject.cs       # Product listing abstraction
├── Helpers/
│   └── BasePlaywrightTest.cs      # Base test class with utilities
└── README.md
```

## Prerequisites

- .NET 8.0 SDK or later
- Visual Studio Code or Visual Studio 2022

## Installation & Setup

### 1. Install Dependencies

Run this from the project directory:

```bash
dotnet restore
```

### 2. Install Playwright Browsers

```bash
dotnet exec bin/Debug/net8.0/playwright.ps1 install
```

Or use:

```powershell
pwsh bin/Debug/net8.0/playwright.ps1 install
```

### 3. Verify Installation

```bash
dotnet test --no-build -v minimal --list-tests   # List all tests
```

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~PlaywrightTests.Tests.BasicTests"
```

### Run Specific Test
```bash
dotnet test --filter "Name=ShouldNavigateToPageAndVerifyTitle"
```

### Run Tests with Parallelization
```bash
dotnet test /p:MaxCpuCount=4
```

### Run Tests in Verbose Mode
```bash
dotnet test -v d
```

### Run Tests with Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Debug Tests in VS Code
1. Open test file
2. Click on "Debug Test" (appears above each test method)
3. Or press Ctrl+F5 to debug

## Key Testing Patterns

### 1. Basic Element Interaction
```csharp
[Test]
public async Task ShouldClickButton()
{
    await Page.GotoAsync("https://example.com");
    await Page.ClickAsync("button");
    await Expect(Page.Locator(".result")).ToBeVisibleAsync();
}
```

### 2. Form Filling
```csharp
[Test]
public async Task ShouldFillForm()
{
    await Page.FillAsync("input[name='username']", "testuser");
    await Page.SelectOptionAsync("select[name='category']", "option1");
    await Page.CheckAsync("input[type='checkbox']");
}
```

### 3. Using Locators
```csharp
// CSS Selector
var button = Page.Locator("button");

// Text selector
var link = Page.Locator("text=Dashboard");

// Combined selector
var item = Page.Locator(".item:has-text('Important')");

// XPath
var element = Page.Locator("//div[@id='main']");

// nth element
var firstButton = Page.Locator("button").First;
var lastButton = Page.Locator("button").Last;
```

### 4. Assertions
```csharp
// Element assertions
await Expect(locator).ToBeVisibleAsync();
await Expect(locator).ToBeEnabledAsync();
await Expect(locator).ToContainTextAsync("Expected text");
await Expect(locator).ToHaveValueAsync("value");
await Expect(locator).ToHaveClassAsync("active");

// Page assertions
await Expect(page).ToHaveTitleAsync("Page Title");
await Expect(page).ToHaveURLAsync(new RegexPatternMatcher(new Regex(".*/page")));
```

### 5. Waiting Strategies
```csharp
// Wait for navigation
await Page.WaitForNavigationAsync();

// Wait for selector
await Page.WaitForSelectorAsync(".element");

// Wait for function/condition
await Page.WaitForFunctionAsync("() => document.title === 'Loaded'");

// Wait for response
var response = await Page.WaitForResponseAsync(r => r.Url.Contains("/api"));
```

### 6. Page Object Model
```csharp
public class ProductPageObject
{
    private readonly IPage _page;
    private ILocator ProductName => _page.Locator(".product-name");

    public async Task SearchAsync(string term)
    {
        await _page.FillAsync("input[name='search']", term);
        await _page.ClickAsync("button:has-text('Search')");
    }
}

[Test]
public async Task ShouldSearchProducts()
{
    var productPage = new ProductPageObject(Page);
    await productPage.SearchAsync("laptop");
}
```

### 7. Network Interception
```csharp
[Test]
public async Task ShouldInterceptApi()
{
    await Page.RouteAsync("**/api/**", async route =>
    {
        var response = await route.FetchAsync();
        await route.ContinueAsync();
    });
    
    await Page.GotoAsync("https://example.com");
}
```

### 8. Screenshots & Traces
```csharp
// Take screenshot
await Page.ScreenshotAsync(new PageScreenshotOptions 
{ 
    Path = "screenshot.png" 
});

// Full page screenshot
await Page.ScreenshotAsync(new PageScreenshotOptions 
{ 
    Path = "fullpage.png", 
    FullPage = true 
});
```

## NUnit Attributes

```csharp
[TestFixture]           // Marks class as test class
[Test]                  // Marks method as test
[SetUp]                 // Runs before each test
[TearDown]              // Runs after each test
[OneTimeSetUp]          // Runs once before all tests
[OneTimeTearDown]       // Runs once after all tests
[Ignore("reason")]      // Skip test
[Category("smoke")]     // Tag tests
[Parallelizable]        // Allow parallel execution
```

## Best Practices

1. **Use Page Object Model** - Encapsulate UI interactions
2. **Explicit Waits** - Use `WaitForAsync()` instead of hardcoded delays
3. **Test Independence** - Each test should be self-contained
4. **Meaningful Names** - Test names should describe what they verify
5. **Single Responsibility** - One assertion concept per test
6. **Reuse Setup** - Use BasePlaywrightTest for common functionality
7. **Capture Failures** - Screenshot on test failure
8. **Error Handling** - Use try-catch for expected exceptions

## Project File Options

The `.csproj` file is already configured with:
- NUnit framework
- Playwright NUnit integration
- Microsoft.NET.Test.Sdk for test discovery
- NUnit3TestAdapter for test execution

## Debugging

### Debug a Specific Test
```bash
dotnet test --filter "Name=ShouldNavigateToPageAndVerifyTitle" --logger "console;verbosity=detailed"
```

### Enable Playwright Debug Mode
Set environment variable before running tests:
```powershell
$env:DEBUG = "pw:api"
dotnet test
```

### View Test Output
Screenshots and logs are saved to the `Screenshots` folder (configured in BasePlaywrightTest).

## Common Issues

### Browsers Not Found
```bash
# Reinstall browsers
dotnet exec bin/Debug/net8.0/playwright.ps1 install
```

### Timeout Errors
- Increase timeout in Page.GotoAsync or WaitFor* methods
- Check internet connection for slow tests
- Ensure selectors are correct

### Navigation Fails
- Verify URL is correct
- Check if page requires authentication
- Add proper wait conditions

## Running in CI/CD

### GitHub Actions Example
```yaml
- name: Install dependencies
  run: dotnet restore

- name: Install Playwright
  run: dotnet exec bin/Debug/net8.0/playwright.ps1 install

- name: Run tests
  run: dotnet test --configuration Release --no-build --logger "console;verbosity=detailed"
```

### Azure DevOps Example
```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: '**/PlaywrightTests.csproj'
    arguments: '--configuration Release --logger "console;verbosity=detailed"'
```

## Test Report Generation

NUnit can generate test reports:

```bash
dotnet test --logger "trx;logfilename=test-results.trx"
```

Generate HTML report from TRX file using additional tools.

## Resources

- [Playwright .NET Documentation](https://playwright.dev/dotnet/)
- [NUnit Documentation](https://docs.nunit.org/)
- [Playwright Best Practices](https://playwright.dev/dotnet/docs/best-practices)
- [Page Object Model Pattern](https://www.selenium.dev/documentation/test_practices/encouraged/page_object_models/)

## Tips & Tricks

### Wait for URL Changes
```csharp
await Expect(Page).ToHaveURLAsync(new RegexPatternMatcher(new Regex(".*/dashboard")));
```

### Handle Multiple Popups
```csharp
Page.Popup += async (_, popup) =>
{
    await popup.CloseAsync();
};
```

### Get Page HTML
```csharp
string html = await Page.ContentAsync();
```

### Execute JavaScript
```csharp
var result = await Page.EvaluateAsync<string>("() => window.location.href");
```

### Switch Between Tabs/Frames
```csharp
var frame = Page.Frames.First(f => f.Name == "iframeId");
await frame.ClickAsync("button");
```

## Notes

- Update base URL in test files to match your application
- Tests run headless by default (browser not visible)
- Add `--headed` flag to `dotnet test` to see browser
- All test files should be in `Tests/` directory
- Page objects should be in `PageObjects/` directory
- Helper classes should be in `Helpers/` directory
