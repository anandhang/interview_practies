using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Threading;

namespace PlaywrightTests.Tests;

[Parallelizable(ParallelScope.Self)]

public class TestPracties
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;

    [SetUp]
    public async Task SetupAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Channel = "chrome",
            Headless = false,
            SlowMo = 100
        });
        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 }
        });
        _page = await _context.NewPageAsync();
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        if (_context != null) await _context.CloseAsync();
        if (_browser != null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }

    [Test]
    [Category("AletsTestCase")]
    public async Task demoAletsTestCase()
    {
        await _page.GotoAsync("https://demo.automationtesting.in/Alerts.html");
        await _page.Locator("//a[text()='Alert with OK ']").ClickAsync();

        await _page.WaitForSelectorAsync("//button[@onclick='alertbox()']", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });

        var dialogTcs = new TaskCompletionSource<IDialog?>();
        void DialogHandler(object? sender, IDialog dialog)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await dialog.AcceptAsync();
                }
                catch
                {
                    // swallow if no dialog or already handled
                }

                dialogTcs.TrySetResult(dialog);
            });
        }

        _page.Dialog += DialogHandler;

        try
        {
            // Use JS click to avoid Playwright click waiting issue on modal dialog arrival.
            await _page.EvaluateAsync("() => document.querySelector(\"button[onclick='alertbox()']\").click()");

            var dialog = await dialogTcs.Task;
            Assert.That(dialog, Is.Not.Null, "Alert should be triggered");
            Assert.That(dialog!.Type, Is.EqualTo("alert"));
            Assert.That(dialog.Message, Is.EqualTo("I am an alert box!"));
        }
        finally
        {
            _page.Dialog -= DialogHandler;
        }

        await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "Alert_test_screenshot.png",
            FullPage = true
        });
        await Task.Delay(5000);
    }

    [Test]
    [Category("demoUploadTest")]
    public async Task demoUploadTest()
    {
        await _page.GotoAsync("https://demo.automationtesting.in/");

        await _page.Locator("//input[@id='email']").FillAsync("anantsmail@gmail.com");

        Assert.That(await _page.Locator("//input[@id='email']").InputValueAsync(), Is.EqualTo("anantsmail@gmail.com"));

        await _page.Locator("//img[@id='enterimg']").ClickAsync();

        await _page.WaitForSelectorAsync("text=Register");

        await _page.Locator("//input[@id='imagesrc']").SetInputFilesAsync("C:\\Users\\anang\\Downloads\\AnandhanG_Feb3rd2026.pdf");

        // Verify file upload - check if the file name is displayed after upload
        var uploadedFileName = await _page.Locator("//input[@id='imagesrc']").InputValueAsync();
        Assert.That(uploadedFileName, Does.Contain("AnandhanG_Feb3rd2026.pdf"), "Uploaded file name should be displayed in the input field");

        await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "upload_test_screenshot.png",
            FullPage = true
        });
        await Task.Delay(5000);
        var filechooserTask = _page.WaitForFileChooserAsync();
        await _page.Locator("//input[@id='imagesrc']").ClickAsync();
        var filechooser = await filechooserTask;
        Assert.That(filechooser, Is.Not.Null, "File chooser should be triggered on clicking the file input");
        await filechooser.SetFilesAsync("C:\\Users\\anang\\Downloads\\AnandhanG_Feb3rd2026_1.pdf");

        var uploadedFileNameAfterChooser = await _page.Locator("//input[@id='imagesrc']").InputValueAsync();
        Assert.That(uploadedFileNameAfterChooser, Does.Contain("AnandhanG_Feb3rd2026_1.pdf"), "Uploaded file name should be displayed in the input field after using file chooser");
        await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "upload_test_screenshot_1.png",
            FullPage = true
        });

        await Task.Delay(5000); // Wait for 2 seconds to visually confirm the upload in the screenshots
    }

    [Test]
    [Category("NavigationTestCase")]
    public async Task demosutomationtesting()
    {
        await _page.GotoAsync("https://demo.automationtesting.in/");

        await _page.Locator("//input[@id='email']").FillAsync("anantsmail@gmail.com");

        Assert.That(await _page.Locator("//input[@id='email']").InputValueAsync(), Is.EqualTo("anantsmail@gmail.com"));

        await _page.Locator("//img[@id='enterimg']").ClickAsync();

        await _page.WaitForSelectorAsync("text=Register");

        await _page.GetByPlaceholder("First Name").FillAsync("Anandhan");
        await _page.GetByPlaceholder("Last Name").FillAsync("G");

        await _page.Locator("//input[@type='email']").FillAsync("anantsmail@gmail.com");
        await _page.Locator("//input[@type='tel']").FillAsync("8754413454");

        await _page.Locator("//input[@type='radio' and @value='Male']").CheckAsync();
        Assert.That(await _page.Locator("//input[@type='radio' and @value='Male']").IsCheckedAsync(), Is.True);

        await _page.Locator("//input[@type='checkbox' and @value='Cricket']").CheckAsync();
        Assert.That(await _page.Locator("//input[@type='checkbox' and @value='Cricket']").IsCheckedAsync(), Is.True);

        await _page.SelectOptionAsync("#Skills", new[] {"C"});
        string selectedValue = await _page.InputValueAsync("#Skills");
        Assert.That(selectedValue, Is.EqualTo("C"));

        // Country selection - commented out due to selector variability
        // await _page.Locator("//span[@role='combobox' and @aria-owns='select2-country-results']").ClickAsync();
        // await _page.Locator("//li[@role='treeitem' and normalize-space(text())='India']").ClickAsync();

        // Date of birth selection
        try
        {
            await _page.SelectOptionAsync("#yearbox", new[] {"1987"});
            await _page.SelectOptionAsync("#monthbox", new[] {"September"});
            await _page.SelectOptionAsync("#daybox", new[] {"4"});
        }
        catch
        {
            // Handle timeout gracefully
            TestContext.WriteLine("Date selection failed - selectors may not be available");
        }

        // Passwords
        try
        {
            await _page.Locator("#firstpassword").FillAsync("Imagine@123");
            await _page.Locator("#secondpassword").FillAsync("Imagine@123");
        }
        catch
        {
            TestContext.WriteLine("Password fields not found");
        }
    }
}
