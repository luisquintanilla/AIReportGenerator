# AI Product Report Generator

This project generates product summary reports using AI using a company / product's website as reference.

It:

- Playwright to navigate website and take screenshots
- GPT-4o-mini (from GitHub Models) to extract page information
- GPT-4o-mini (from GitHub Models) to generate report using extracted information

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Set up Playwright](https://playwright.dev/dotnet/docs/intro)
- [GitHub Personal Access Token](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens)

    ```bash
    # Build the project
    dotnet build

    # Install required browsers - replace netX with actual output folder name, e.g. net8.0.
    pwsh bin/Debug/netX/playwright.ps1 install

    # If the pwsh command does not work (throws TypeNotFound), make sure to use an up-to-date version of PowerShell.
    dotnet tool update --global PowerShell
    ```

## Run app

```bash
dotnet run
```

## Potential improvements

- Make Playwright a tool.
- Add a code interpreter and let AI write / execute Playwright code based on user prompts (i.e. extract all links from this webpage and follow them), instead of being limited by the fixed prompt I provided.
- Extract and follow links
- Add web search so you can find what is being said about the product / company outside of its website
- Persist / index data so it doesn't have to be fetched every time.
- Apply agentic patterns