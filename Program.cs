using Microsoft.Playwright;
using Microsoft.Extensions.AI;
using Azure;
using Azure.AI.Inference;
using ChatRole = Microsoft.Extensions.AI.ChatRole;

var GetData = async () => {
    var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions());
    var page = await browser.NewPageAsync();
    await page.GotoAsync("https://playwright.dev/dotnet");

    // Get the height of the page
    var pageHeight = await page.EvaluateAsync<int>("() => document.body.scrollHeight");

    // Set the viewport height
    int viewportHeight = 1080;
    await page.SetViewportSizeAsync(1920, viewportHeight);

    int scrollPosition = 0;
    int screenshotIndex = 1;
    int previousScrollPosition = -1;

    while (scrollPosition < pageHeight)
    {
        // Scroll to the current position
        await page.EvaluateAsync($"window.scrollTo(0, {scrollPosition});");

        // Check if the scroll position has changed
        if (scrollPosition != previousScrollPosition)
        {
            // Take a screenshot
            await page.ScreenshotAsync(new()
            {
                Path = $"./data/screenshot_{screenshotIndex}.png",
                FullPage = false
            });

            // Update the previous scroll position
            previousScrollPosition = scrollPosition;
            screenshotIndex++;
        }

        // Move to the next position
        scrollPosition += viewportHeight;
    }

    // Optionally, take a final screenshot to capture any remaining part of the page
    await page.ScreenshotAsync(new()
    {
        Path = $"./data/screenshot_{screenshotIndex}.png",
        FullPage = false
    });
};

var ProcessData = async Task<List<PageData>>() => {
    IChatClient client =
        new AzureAIInferenceChatClient(
            new ChatCompletionsClient(
                new Uri("https://models.inference.ai.azure.com"),
                new AzureKeyCredential(Environment.GetEnvironmentVariable("GH_TOKEN"))),
            "gpt-4o-mini");

    var filePaths = Directory.GetFiles("data");

    var systemPrompt = 
        """
        You are an AI assistant that extracts information from web page screenshots.
        """;

    var pageData = filePaths.Select(filePath => new PageData(filePath, String.Empty));

    List<ChatMessage> messages = [
        new ChatMessage(ChatRole.System, systemPrompt),
    ];

    var userMessages = 
        pageData.Select(
            data => 
                new ChatMessage(
                    ChatRole.User, 
                    [new ImageContent(File.ReadAllBytes(data.Path),"image/png")]));

    messages.AddRange(userMessages);

    messages.Add(new ChatMessage(ChatRole.User,"Extract data from the screenshots"));

    var response = await client.CompleteAsync<List<PageData>>(messages);

    return response.Result;
};

var GenerateReport = async (List<PageData> data, string savePath) => {
    
    IChatClient client =
        new AzureAIInferenceChatClient(
            new ChatCompletionsClient(
                new Uri("https://models.inference.ai.azure.com"),
                new AzureKeyCredential(Environment.GetEnvironmentVariable("GH_TOKEN"))),
            "gpt-4o-mini");

    var systemPrompt = 
        """
        You are an AI assistant that uses information extracted from a product or company's website to generate competitive analysis report.

        Focus on the following areas:

        - Product / Service Comparisons
        """;

    List<ChatMessage> messages = [
        new ChatMessage(ChatRole.System, systemPrompt),
    ];

    var userMessages = 
        data.Select(
            data => 
                new ChatMessage(
                    ChatRole.User, 
                    [new TextContent(data.Text)]));

    messages.AddRange(userMessages);

    messages.Add(new ChatMessage(ChatRole.User,"Generate competitive analysis report. Format as markdown."));

    var response = await client.CompleteAsync(messages);

    File.WriteAllText(savePath, response.Message.Text);
};


// Get data
if(Directory.GetFiles("data").Length == 0)
{
    await GetData();
}

// Extract data from screenshots
var data = await ProcessData();

// Generate report
await GenerateReport(data, "report.md");

record PageData(string Path, string Text);