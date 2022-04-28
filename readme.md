# dotnet-link-tool

Updates article links in one or more markdown files within the `net/` folder of the [dotnet/docs-desktop](https://github.com/dotnet/docs-desktop) repo.

You can run the program on a single markdown file, or on any folder that contains markdown files. For the latter case, every markdown file at any nested level is processed.

The program fixes stale article links for which a redirect exists. Each link is validated to ensure that the linked article exists.

Don't run this tool on .NET Framework articles unless you want their article links updated to the latest .NET articles.

## How to run the tool

Call the program by passing in a single argument that's either:

- A markdown file path (for example, `dotnet-link-tool.exe attached-properties-overview.md`), or
- A folder path that contains markdown files at any nested level (for example, `dotnet-link-tool.exe C:\Repos\dotnet\docs-desktop\dotnet-desktop-guide\net`).

## Redirect file assumptions

- No redirect double-hops (target URL of one redirect is the source URL of another).
- No conflicting target URLs for the same source URL.
- The target URL article has the same or higher .NET version as the source URL article.
- The target URL article has a .NET 5/6 version.

## Link handling

- Checks and updates article links as needed based on `definitions.json` redirect entries.
- Relativizes paths to articles with a .NET 5/6 version, because those links and the article containing them exist under the `net/` folder.
- Removes query parameters for relative links. For example: `../net/wpf/data/123.md?view=netdesktop-5.0&preserve-view=true` => `../net/wpf/data/123.md`).
- Removes the redundant `./` from the beginning of paths. For example: `./data/123.md` => `data/123.md`)
- Removes redundant path segments. For example: for article path `/framework/wpf/advanced/123.md` and link path `../../../framework/wpf/controls/123.md`, change the relative path to `../controls/123.md`).
- For readability, adds the `index` file to links that omit it. For example: `/dotnet/desktop/wpf/xaml/` => `/dotnet/desktop/wpf/xaml/index`).
- Prints a detailed report of each link's transformations.

## Report output

Here's an example of how an updated redirect is reported:

```
76. Link in article: '\net\wpf\properties\custom-dependency-properties.md' *
  ORIGINAL-LINK: /dotnet/desktop/wpf/advanced/dependency-property-metadata?view=netframeworkdesktop-4.8&preserve-view=true#adding-a-class-as-an-owner-of-an-existing-dependency-property
    REMOVE-FRAGMENT: /dotnet/desktop/wpf/advanced/dependency-property-metadata?view=netframeworkdesktop-4.8&preserve-view=true
      REMOVE-QUERY: /dotnet/desktop/wpf/advanced/dependency-property-metadata
        REDIRECT-TO: /dotnet/desktop/wpf/properties/dependency-property-metadata?view=netdesktop-6.0
          REMOVE-NEW-QUERY: /dotnet/desktop/wpf/properties/dependency-property-metadata
            RELATIVE-PATH: dependency-property-metadata.md
              RESTORE-FRAGMENT: dependency-property-metadata.md#adding-a-class-as-an-owner-of-an-existing-dependency-property
```

Here's an example of how an updated non-redirect issue is reported:

```
78. Link in article: '\net\wpf\properties\xaml-loading-and-dependency-properties.md' *
  ORIGINAL-LINK: /dotnet/desktop/wpf/properties/dependency-property-callbacks-and-validation?preserve-view=true#property-changed-callbacks
    REMOVE-FRAGMENT: /dotnet/desktop/wpf/properties/dependency-property-callbacks-and-validation?preserve-view=true
      REMOVE-QUERY: /dotnet/desktop/wpf/properties/dependency-property-callbacks-and-validation
        REDIRECT-TO: no redirect
          RELATIVE-PATH: dependency-property-callbacks-and-validation.md
            RESTORE-FRAGMENT: dependency-property-callbacks-and-validation.md#property-changed-callbacks
```

## Miscellaneous notes

### Repo folder structure

```
dotnet-desktop-guide/
    framework/
        winforms/
            advanced/
            controls/
        wpf/
            advanced/
            app-development/
            controls/
            data/
            getting-started/
            graphics-multimedia/
            media/
    net/
        winforms/
            controls/
            forms/
            get-started/
            input-keyboard/
            input-mouse/
            media/
            migration/
            overview/
            whats-new/
        wpf/
            controls/
            data/
            documents/
            events/
            get-started/
            migration/
            overview/
            properties/
            systems/
            windows/
            xaml/
    xaml-services/
```

A more intuitive name for the `net/` folder would be `core/` since both .NET Framework and .NET Core are .NET technologies, and the `net` folder only contains .NET Core articles.