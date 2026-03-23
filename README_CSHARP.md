# Playwright C# Sample Test Cases

Complete, production-ready Playwright testing framework using C# and NUnit.

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- Visual Studio Code or Visual Studio 2022

### Installation

1. **Restore dependencies:**
```bash
dotnet restore
```

2. **Install Playwright browsers:**
```bash
dotnet exec bin/Debug/net8.0/playwright.ps1 install
```

3. **Run tests:**
```bash
dotnet test
```

## Project Structure

```
PlaywrightTests/
├── PlaywrightTests.csproj              # Project configuration
│
├── Tests/
│   ├── BasicTests.cs                   # Basic interactions (navigation, clicks, etc.)
│   ├── FormTests.cs                    # Form filling, validation, submission
│   ├── NavigationTests.cs              # Page navigation and URL verification
│   ├── NetworkTests.cs                 # API interception, network monitoring
│   ├── AdvancedTests.cs               # Popups, downloads, storage, performance
│   ├── PageObjectModelTests.cs        # Tests using Page Object pattern
│   └── QuickStartTests.cs             # Beginner-friendly examples
│
├── PageObjects/
│   ├── LoginPageObject.cs             # Login page abstraction
│   └── ProductPageObject.cs           # Product listing abstraction
│
├── Helpers/
│   ├── BasePlaywrightTest.cs          # Base test class with utilities
│   └── TestConfig.cs                  # Configuration and helpers
│
├── README.md                           # This file
├── CSHARP_GUIDE.md                    # Detailed C# Playwright guide
└── PlaywrightTests.csproj             # Project file
```

## Running Tests

### All Tests
```bash
dotnet test
```

### Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~PlaywrightTests.Tests.BasicTests"
```

### Specific Test Method
```bash
dotnet test --filter "Name=QuickStart_NavigateAndVerifyTitle"
```

### With Browser Visible
```bash
# Set environment variable then run
SET HEADLESS=false
dotnet test
```

### Parallel Execution
```bash
dotnet test /p:MaxCpuCount=4
```

### Verbose Output
```bash
dotnet test -v d
```

## Test Examples

### Basic Navigation
```csharp
[Test]
public async Task ShouldNavigateToPageAndVerifyTitle()
{
    await Page.GotoAsync("https://example.com");
    await Expect(Page).ToHaveTitleAsync(new RegexPatternMatcher(new Regex("Example")));
}
```

### Form Filling
```csharp
[Test]
public async Task ShouldFillAndSubmitForm()
{
    await Page.GotoAsync("https://example.com/form");
    await Page.FillAsync("input[name='username']", "testuser");
    await Page.ClickAsync("button[type='submit']");
    await Expect(Page.Locator("text=Success")).ToBeVisibleAsync();
}
```

### Using Page Object Model
```csharp
[Test]
public async Task ShouldSearchProducts()
{
    var productPage = new ProductPageObject(Page);
    await productPage.NavigateAsync();
    await productPage.SearchProductAsync("laptop");
    
    int count = await productPage.GetProductCountAsync();
    Assert.That(count, Is.GreaterThan(0));
}
```

### Network Interception
```csharp
[Test]
public async Task ShouldCaptureApiResponse()
{
    var responseTask = Page.WaitForResponseAsync(response =>
        response.Url.Contains("/api/data") && response.Status == 200
    );
    
    await Page.GotoAsync("https://example.com");
    var response = await responseTask;
    var json = await response.JsonAsync();
    
    Assert.That(json, Is.Not.Null);
}
```

### Using Base Test Class
```csharp
[Test]
public async Task QuickStart_CheckVisibleElements()
{
    await NavigateToAsync();  // Uses BaseUrl
    var visible = await IsElementVisibleAsync(".header");
    Assert.That(visible, Is.True);
    await CaptureScreenshotAsync("success");
}
```

## Key Features

**Test Organization**
- Organized by test type (Basic, Form, Navigation, Network, Advanced)
- Page Object Model for maintainability
- Base test class for common functionality

**Browser Support**
- Chromium (default)
- Firefox
- WebKit (Safari)

**Locator Strategies**
- CSS selectors: `button`
- Text selectors: `text=Click me`
- Attribute selectors: `input[name='email']`
- Combined: `button:has-text('Submit')`
- XPath: `//div[@id='main']`

**Assertions**
- Element visibility, state, content
- URL and page title verification
- Value and attribute checking
- Custom matchers

**Advanced Features**
- Network interception and monitoring
- File download handling
- Popup and dialog handling
- Storage (localStorage, sessionStorage)
- Console and error listeners
- Performance metrics
- Screenshot and trace capture

## Configuration

Environment variables:
```
BASE_URL=https://example.com
BROWSER_TYPE=chromium
HEADLESS=true
```

Or modify in `TestConfig.cs`:
```csharp
public static string GetBaseUrl()
{
    return Environment.GetEnvironmentVariable("BASE_URL") ?? "https://example.com";
}
```

## Locator Examples

```csharp
// By CSS
Page.Locator(".btn-primary")
Page.Locator("button[id='submit']")

// By text
Page.Locator("text=Login")
Page.Locator("text=/exact match/i")

// Combine strategies
Page.Locator(".list-item:has-text('Active')")
Page.Locator("button:has(svg)")

// Nth element
Page.Locator("button").First
Page.Locator("button").Last
Page.Locator("button").Nth(2)

// XPath
Page.Locator("//button[@class='primary']")
```

## Common Patterns

### Wait for Navigation
```csharp
var navigationTask = Page.WaitForNavigationAsync();
await Page.ClickAsync("a[href='/about']");
await navigationTask;
```

### Handle Popups
```csharp
Page.Popup += async (_, popup) =>
{
    await popup.CloseAsync();
};
```

### Take Screenshots
```csharp
await Page.ScreenshotAsync(new PageScreenshotOptions 
{ 
    Path = "screenshot.png",
    FullPage = true
});
```

### Execute JavaScript
```csharp
var result = await Page.EvaluateAsync<string>(
    "() => document.title"
);
```

### Wait Conditions
```csharp
// Wait for selector
await Page.WaitForSelectorAsync(".content");

// Wait for function
await Page.WaitForFunctionAsync("() => document.readyState === 'complete'");

// Wait for specific timeout (will throw on failure)
await Page.WaitForSelectorAsync(".element", new() { Timeout = 30000 });
```

## Best Practices

1. **Use Page Object Model** - Encapsulate UI interactions
2. **Explicit Waits** - Rely on Playwright's auto-wait, not hardcoded delays
3. **Test Independence** - Each test should be self-contained
4. **Meaningful Names** - `ShouldSearchProductsAndVerifyResults` not `test1`
5. **Single Concept** - One primary assertion per test
6. **Reuse Code** - Use helper methods and base classes
7. **Capture Failures** - Automatic screenshot on failure
8. **Parallel Tests** - Mark as `[Parallelizable]` for concurrent execution

## Debugging

### Debug Specific Test
```bash
dotnet test --filter "Name=QuickStart_NavigateAndVerifyTitle" --logger "console;verbosity=detailed"
```

### Enable Debug Logging
```powershell
$env:DEBUG = "pw:api"
dotnet test
```

### Step Through in IDE
- Click "Debug Test" in Visual Studio/VS Code
- Or set breakpoints and use test explorer

### View Traces
Traces are automatically captured on failures and saved to `test-traces/` folder.

## Troubleshooting

**CLR Error**
```
The type initializer for 'DebuggerNonUserCodeAttribute' threw an exception.
```
Solution: Update .NET SDK

**Timeout Issues**
- Increase timeout: `new() { Timeout = 30000 }`
- Check if selector is correct
- Verify page has loaded completely

**Element Not Found**
- Verify selector is correct: Use browser DevTools
- Ensure element is visible/rendered
- Add explicit wait: `await Page.WaitForSelectorAsync(selector)`

**Network Errors**
- Check internet connectivity
- Verify base URL in test
- Check if site requires authentication

## CI/CD Integration

### GitHub Actions
```yaml
- name: Install Playwright
  run: dotnet exec bin/Debug/net8.0/playwright.ps1 install

- name: Run Tests
  run: dotnet test --configuration Release
```

### Azure DevOps
```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: '**/*.csproj'
```

## Report Generation

Generate test report:
```bash
dotnet test --logger "trx;logfilename=test-results.trx"
```

## NUnit Attributes

```csharp
[TestFixture]              // Mark class as test
[Test]                     // Mark method as test
[SetUp]                    // Run before each test
[TearDown]                 // Run after each test
[OneTimeSetUp]             // Run once before all
[OneTimeTearDown]          // Run once after all
[Ignore("reason")]         // Skip test
[Category("smoke")]        // Tag test
[Parallelizable]           // Allow parallel run
```

## Resources

- [Playwright .NET Docs](https://playwright.dev/dotnet/)
- [NUnit Documentation](https://docs.nunit.org/)
- [Page Object Model Pattern](https://www.selenium.dev/documentation/test_practices/encouraged/page_object_models/)
- [CSS Selectors Cheat Sheet](https://www.w3schools.com/cssref/css_selectors.asp)

## Tips & Tricks

### Multiple Tabs
```csharp
var pages = new List<IPage> { Page };
Page.Context.Page += (_, page) => pages.Add(page);
```

### Local Storage
```csharp
await Page.EvaluateAsync("() => localStorage.setItem('key', 'value')");
```

### Get All Network Requests
```csharp
var requests = new List<string>();
Page.Request += (_, req) => requests.Add(req.Url);
```

### Slow Motion
```csharp
var browser = await chromium.LaunchAsync(new() { SlowMo = 100 });
```

### Mobile Testing
```csharp
var context = await browser.NewContextAsync(TestConfig.GetMobileContextOptions());
```

## Next Steps

1. Update base URLs to your application
2. Review test examples for your use case
3. Create page objects for your pages
4. Run tests with `-v d` to understand behavior
5. Set up CI/CD integration
6. Configure test reports

---

For detailed guide on all features, see [CSHARP_GUIDE.md](CSHARP_GUIDE.md)
