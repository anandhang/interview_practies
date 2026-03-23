using Microsoft.Playwright;

namespace PlaywrightTests.PageObjects;

/// <summary>
/// Page Object Model for login page.
/// Encapsulates all interactions with the login page.
/// </summary>
public class LoginPageObject
{
    private readonly IPage _page;
    private const string BaseUrl = "https://example.com";

    // Locators
    private ILocator UsernameInput => _page.Locator("input[name='username']");
    private ILocator PasswordInput => _page.Locator("input[name='password']");
    private ILocator LoginButton => _page.Locator("button:has-text('Login')");
    private ILocator ErrorMessage => _page.Locator(".error-message");
    private ILocator WelcomeMessage => _page.Locator("text=Welcome");

    public LoginPageObject(IPage page)
    {
        _page = page;
    }

    public async Task NavigateAsync()
    {
        await _page.GotoAsync($"{BaseUrl}/login");
    }

    public async Task EnterUsernameAsync(string username)
    {
        await UsernameInput.FillAsync(username);
    }

    public async Task EnterPasswordAsync(string password)
    {
        await PasswordInput.FillAsync(password);
    }

    public async Task ClickLoginButtonAsync()
    {
        await LoginButton.ClickAsync();
    }

    public async Task LoginAsync(string username, string password)
    {
        await EnterUsernameAsync(username);
        await EnterPasswordAsync(password);
        await ClickLoginButtonAsync();
    }

    public async Task<string?> GetErrorMessageAsync()
    {
        return await ErrorMessage.TextContentAsync();
    }

    public async Task<bool> IsErrorMessageVisibleAsync()
    {
        return await ErrorMessage.IsVisibleAsync();
    }

    public async Task<bool> IsWelcomeMessageVisibleAsync()
    {
        return await WelcomeMessage.IsVisibleAsync();
    }

    public async Task<bool> IsLoginButtonEnabledAsync()
    {
        return await LoginButton.IsEnabledAsync();
    }

    public IPage GetPage()
    {
        return _page;
    }
}
