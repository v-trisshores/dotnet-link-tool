# dotnet-link-tool

Updates article links in one or more markdown files within the [dotnet\docs-desktop](https://github.com/dotnet/docs-desktop) repo.

You can run the program on a single markdown file, or on any folder that contains markdown files. For the latter case, every markdown file at any nested level is processed.

The program fixes stale article links for which a redirect exists. Each link is validated to ensure that the linked article exists.

## How to run

Call the program by passing in a single argument that's either:

- A markdown file path (e.g. `dotnet-link-tool.exe attached-properties-overview.md`), or
- A folder path that contains markdown files at any nested level (e.g. `dotnet-link-tool.exe C:\Repos\dotnet\docs-desktop\dotnet-desktop-guide`).

## Redirect file assumptions

- No redirect double-hops.
- No conflicting target urls for the same source url.
- Target url is a same or higher version than source url.

## Link handling

- Checks and if necessary updates links based on `definitions.json` redirect entries.
- Removes query parameters for relative links (e.g. `../net/wpf/data/123.md?view=netdesktop-5.0&preserve-view=true` => `../net/wpf/data/123.md`).
- Removes `./` from the beginning of paths (e.g. `./data/123.md` => `data/123.md`)
- Removes redundant path segments (e.g. for article path `\framework\wpf\advanced\123.md` and link path `../../../framework/wpf/controls/123.md`, change the relative path to `../controls/123.md`).
- For readability, adds the `index` file to links that omit it (e.g. `/dotnet/desktop/wpf/xaml/` => `/dotnet/desktop/wpf/xaml/index`).
- Prints out a detailed configurable report of link transformations.

## Report output

Here's an example of how an updated redirect is reported:

```
51. Link in article: '\net\wpf\properties\framework-property-metadata.md'
  ORIGINAL-LINK: /dotnet/desktop/wpf/advanced/property-value-inheritance?view=netframeworkdesktop-4.8&preserve-view=true
    REMOVE-QUERY: /dotnet/desktop/wpf/advanced/property-value-inheritance
      REDIRECT-TO: /dotnet/desktop/wpf/properties/property-value-inheritance?view=netdesktop-6.0
        REMOVE-NEW-QUERY: /dotnet/desktop/wpf/properties/property-value-inheritance
          RESTORE-QUERY: /dotnet/desktop/wpf/properties/property-value-inheritance?view=netdesktop-6.0&preserve-view=true
```

Here's an example of how an updated non-redirect issue is reported:

```
4. Link in article: '\net\wpf\properties\dependency-properties-overview.md'
  ORIGINAL-LINK: /dotnet/desktop/wpf/xaml/
    REDIRECT-TO: no redirect
      COMPLETED-PATH: /dotnet/desktop/wpf/xaml/index
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

### Base alias links found in /dotnet/desktop/framework/ articles

```
/dotnet/desktop/wpf/get-started/...
/dotnet/desktop/xaml-services/...
```

### Base alias links found in /dotnet/desktop/net/ articles

```
/dotnet/desktop/wpf/advanced/...
/dotnet/desktop/wpf/app-development/...
/dotnet/desktop/wpf/controls/...
/dotnet/desktop/wpf/data/...
/dotnet/desktop/wpf/get-started/...
/dotnet/desktop/wpf/graphics-multimedia/...
/dotnet/desktop/wpf/properties/...
/dotnet/desktop/wpf/systems/...
/dotnet/desktop/wpf/xaml/...
/dotnet/desktop/xaml-services/...
```

### Base alias links found in /dotnet/desktop/xaml-services/ articles

none
