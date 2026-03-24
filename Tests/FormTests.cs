using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PlaywrightTests.Tests;

[Parallelizable(ParallelScope.Self)]
public class FormTests : PageTest
{
    private const string BaseUrl = "https://example.com";

    [Test]
    public async Task ShouldFillAndSubmitForm()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/form");

        // Act
        await Page.FillAsync("input[name='username']", "testuser");
        await Page.FillAsync("input[name='email']", "test@example.com");
        await Page.FillAsync("textarea[name='message']", "This is a test message");

        // Assert - Verify values filled
        await Expect(Page.Locator("input[name='username']")).ToHaveValueAsync("testuser");
        await Expect(Page.Locator("input[name='email']")).ToHaveValueAsync("test@example.com");

        // Act
        await Page.ClickAsync("button[type='submit']");

        // Assert
        await Expect(Page.Locator("text=Form submitted successfully")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ShouldHandleDropdownSelection()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/form");

        // Act
        await Page.SelectOptionAsync("select[name='category']", "option1");

        // Assert
        await Expect(Page.Locator("select[name='category']")).ToHaveValueAsync("option1");
    }

    [Test]
    public async Task ShouldHandleCheckboxes()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/form");

        var checkbox = Page.Locator("input[name='agree']");

        // Act
        await checkbox.CheckAsync();

        // Assert
        await Expect(checkbox).ToBeCheckedAsync();

        // Act
        await checkbox.UncheckAsync();

        // Assert
        await Expect(checkbox).Not.ToBeCheckedAsync();
    }

    [Test]
    public async Task ShouldHandleRadioButtons()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/form");

        var radioButton = Page.Locator("input[name='gender'][value='male']");

        // Act
        await radioButton.CheckAsync();

        // Assert
        await Expect(radioButton).ToBeCheckedAsync();
    }

    [Test]
    public async Task ShouldValidateFormField()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/form");

        // Act
        await Page.ClickAsync("button[type='submit']");

        // Assert - Validation error should appear
        var errorMessage = Page.Locator("text=This field is required");
        await Expect(errorMessage).ToBeVisibleAsync();
    }

    [Test]
    public async Task ShouldClearFormFields()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/form");

        // Act
        await Page.FillAsync("input[name='username']", "testuser");
        await Page.FillAsync("input[name='email']", "test@example.com");

        // Assert
        await Expect(Page.Locator("input[name='username']")).ToHaveValueAsync("testuser");

        // Act
        await Page.ClickAsync("button:has-text('Clear')");

        // Assert
        await Expect(Page.Locator("input[name='username']")).ToHaveValueAsync("");
        await Expect(Page.Locator("input[name='email']")).ToHaveValueAsync("");
    }

    [Test]
    public async Task ShouldHandleFileUpload()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/upload");

        // Create a test file
        var testFilePath = Path.Combine(Path.GetTempPath(), "test_upload.txt");
        await File.WriteAllTextAsync(testFilePath, "Test file content");

        try
        {
            // Act
            await Page.Locator("input[type='file']").SetInputFilesAsync(testFilePath);

            // Assert
            var fileName = await Page.Locator(".file-name").TextContentAsync();
            Assert.That(fileName, Does.Contain("test_upload.txt"));
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
        }
    }

    [Test]
    public async Task ShouldValidateEmailFormat()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/form");

        // Act - Enter invalid email
        await Page.FillAsync("input[type='email']", "invalid-email");
        await Page.ClickAsync("button[type='submit']");

        // Assert
        await Expect(Page.Locator("text=Please enter a valid email")).ToBeVisibleAsync();

        // Act - Enter valid email
        await Page.FillAsync("input[type='email']", "valid@example.com");

        // Assert - Error should disappear
        await Expect(Page.Locator("text=Please enter a valid email")).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task ShouldDisableSubmitButtonOnLoadingState()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/form");
        var submitButton = Page.Locator("button[type='submit']");

        // Act
        await Page.FillAsync("input[name='username']", "testuser");
        await submitButton.ClickAsync();

        // Assert - Button should be disabled during submission
        await Expect(submitButton).ToBeDisabledAsync();

        // Wait for form submission to complete
        await Page.WaitForFunctionAsync("() => !document.querySelector('button[type=\"submit\"]').disabled");

        // Assert - Button should be enabled after submission
        await Expect(submitButton).ToBeEnabledAsync();
    }
}
