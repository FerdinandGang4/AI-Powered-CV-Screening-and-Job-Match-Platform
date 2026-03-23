$outputPath = Join-Path $PSScriptRoot "Project-Libraries-Reference.pdf"

$rawLines = @(
    "Project Libraries Reference",
    "",
    "Overview",
    "",
    "This document lists the main libraries and packages used in the AI-Powered CV Screening and Job Match Platform and explains the role of each one in the project.",
    "",
    "Backend Libraries",
    "",
    "1. ASP.NET Core Web SDK",
    "Source: Microsoft.NET.Sdk.Web",
    "Role: Provides the core ASP.NET backend framework for building the API, handling routing, controllers, dependency injection, configuration, and HTTP request processing.",
    "",
    "2. UglyToad.PdfPig",
    "Version: 1.7.0-custom-5",
    "Role: Used in the backend to extract readable text from uploaded PDF files, especially candidate CV documents. This text is then used for parsing, skill extraction, matching, and ranking.",
    "",
    "Frontend Runtime Libraries",
    "",
    "3. React",
    "Version: 19.2.4",
    "Role: Main frontend UI library used to build reusable components, manage application state, and render the recruiter workflow.",
    "",
    "4. React DOM",
    "Version: 19.2.4",
    "Role: Connects React components to the browser DOM and enables the frontend application to render in the browser.",
    "",
    "Frontend Build And Development Libraries",
    "",
    "5. Vite",
    "Version: 8.0.1",
    "Role: Frontend build tool and development server. It provides fast local development, module bundling, and production builds for the React app.",
    "",
    "6. @vitejs/plugin-react",
    "Version: 6.0.1",
    "Role: Vite plugin that enables React support, including JSX transformation and React-specific development behavior.",
    "",
    "Code Quality And Linting Libraries",
    "",
    "7. ESLint",
    "Version: 9.39.4",
    "Role: Static analysis tool used to detect code issues, maintain code quality, and promote consistent coding practices in the frontend.",
    "",
    "8. @eslint/js",
    "Version: 9.39.4",
    "Role: Provides core JavaScript linting rules used by ESLint.",
    "",
    "9. eslint-plugin-react-hooks",
    "Version: 7.0.1",
    "Role: Ensures correct usage of React Hooks and helps prevent common Hook-related bugs.",
    "",
    "10. eslint-plugin-react-refresh",
    "Version: 0.5.2",
    "Role: Supports linting rules that work well with React Fast Refresh during development.",
    "",
    "11. globals",
    "Version: 17.4.0",
    "Role: Provides predefined global variables for linting environments so frontend code can be validated correctly.",
    "",
    "Type Support Libraries",
    "",
    "12. @types/react",
    "Version: 19.2.14",
    "Role: Provides TypeScript type definitions for React. Even though the current frontend uses JavaScript, this package supports tooling compatibility and future migration.",
    "",
    "13. @types/react-dom",
    "Version: 19.2.3",
    "Role: Provides TypeScript type definitions for React DOM and supports frontend tooling compatibility.",
    "",
    "Supporting Platform Technologies",
    "",
    "14. .NET 10",
    "Role: Target runtime for the backend API project.",
    "",
    "15. OpenAI API",
    "Role: Used for AI-assisted candidate ranking and explanation generation when the backend is configured with a valid API key.",
    "",
    "16. PowerShell",
    "Role: Used in the project for utility scripts such as generating PDF documentation files.",
    "",
    "Summary",
    "",
    "The project uses a focused and relatively lightweight technology stack: ASP.NET Core for backend API services, PdfPig for PDF CV text extraction, React for the frontend interface, Vite for frontend build tooling, ESLint and related plugins for code quality, and OpenAI API for AI-assisted ranking functionality.",
    "",
    "This combination keeps the system modular, understandable, and suitable for an academic, portfolio, or early product prototype."
)

function Wrap-Line {
    param(
        [string]$Text,
        [int]$Width = 92
    )

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return @("")
    }

    $words = $Text -split '\s+'
    $lines = New-Object System.Collections.Generic.List[string]
    $current = ""

    foreach ($word in $words) {
        $candidate = if ($current) { "$current $word" } else { $word }
        if ($candidate.Length -le $Width) {
            $current = $candidate
        } else {
            if ($current) {
                $lines.Add($current)
            }
            $current = $word
        }
    }

    if ($current) {
        $lines.Add($current)
    }

    return $lines.ToArray()
}

function Escape-PdfText {
    param([string]$Text)
    return $Text.Replace('\', '\\').Replace('(', '\(').Replace(')', '\)')
}

$wrappedLines = New-Object System.Collections.Generic.List[string]
foreach ($line in $rawLines) {
    foreach ($wrapped in (Wrap-Line -Text $line)) {
        $wrappedLines.Add($wrapped)
    }
}

$linesPerPage = 42
$pages = @()
for ($i = 0; $i -lt $wrappedLines.Count; $i += $linesPerPage) {
    $remaining = $wrappedLines.Count - $i
    $take = [Math]::Min($linesPerPage, $remaining)
    $pages += ,($wrappedLines.GetRange($i, $take).ToArray())
}

$objects = New-Object System.Collections.Generic.List[string]
$pageObjectNumbers = @()
$contentObjectNumbers = @()

$objects.Add("<< /Type /Catalog /Pages 2 0 R >>")
$objects.Add("")

$nextObjectNumber = 3
foreach ($page in $pages) {
    $pageObjectNumbers += $nextObjectNumber
    $nextObjectNumber++
    $contentObjectNumbers += $nextObjectNumber
    $nextObjectNumber++
}
$fontObjectNumber = $nextObjectNumber

$kids = ($pageObjectNumbers | ForEach-Object { "$_ 0 R" }) -join " "
$objects[1] = "<< /Type /Pages /Kids [ $kids ] /Count $($pages.Count) >>"

for ($pageIndex = 0; $pageIndex -lt $pages.Count; $pageIndex++) {
    $contentObjectNumber = $contentObjectNumbers[$pageIndex]
    $pageObject = "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 $fontObjectNumber 0 R >> >> /Contents $contentObjectNumber 0 R >>"
    $objects.Add($pageObject)

    $contentLines = New-Object System.Collections.Generic.List[string]
    $contentLines.Add("BT")
    $contentLines.Add("/F1 12 Tf")
    $contentLines.Add("72 740 Td")
    $contentLines.Add("16 TL")

    foreach ($line in $pages[$pageIndex]) {
        $escaped = Escape-PdfText $line
        $contentLines.Add("($escaped) Tj")
        $contentLines.Add("T*")
    }

    $contentLines.Add("ET")
    $streamText = ($contentLines -join "`n") + "`n"
    $streamLength = [System.Text.Encoding]::ASCII.GetByteCount($streamText)
    $contentObject = "<< /Length $streamLength >>`nstream`n$streamText" + "endstream"
    $objects.Add($contentObject)
}

$objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>")

$builder = New-Object System.Text.StringBuilder
[void]$builder.Append("%PDF-1.4`n")

$offsets = New-Object System.Collections.Generic.List[int]
for ($index = 0; $index -lt $objects.Count; $index++) {
    $offsets.Add([System.Text.Encoding]::ASCII.GetByteCount($builder.ToString()))
    $objectNumber = $index + 1
    [void]$builder.Append("$objectNumber 0 obj`n")
    [void]$builder.Append($objects[$index])
    [void]$builder.Append("`nendobj`n")
}

$xrefOffset = [System.Text.Encoding]::ASCII.GetByteCount($builder.ToString())
[void]$builder.Append("xref`n")
[void]$builder.Append("0 $($objects.Count + 1)`n")
[void]$builder.Append("0000000000 65535 f `n")
foreach ($offset in $offsets) {
    [void]$builder.Append(("{0:0000000000} 00000 n `n" -f $offset))
}

[void]$builder.Append("trailer`n")
[void]$builder.Append("<< /Size $($objects.Count + 1) /Root 1 0 R >>`n")
[void]$builder.Append("startxref`n")
[void]$builder.Append("$xrefOffset`n")
[void]$builder.Append("%%EOF")

[System.IO.File]::WriteAllBytes($outputPath, [System.Text.Encoding]::ASCII.GetBytes($builder.ToString()))
Write-Output "Created $outputPath"
