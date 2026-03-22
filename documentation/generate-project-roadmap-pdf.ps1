$outputPath = Join-Path $PSScriptRoot "project-roadmap.pdf"

$rawLines = @(
    "AI-Powered CV Screening and Job Match Platform",
    "",
    "Project Implementation Steps",
    "",
    "1. Define scope and success criteria",
    "Confirm the first release features, including job description upload, multiple CV uploads, parsing, candidate evaluation, ranking, and explanation generation.",
    "",
    "2. Design the system architecture",
    "Separate the application into clear modules such as recruiter interface, backend services, CV parsing, matching engine, data storage, and optional AI services.",
    "",
    "3. Choose the ASP.NET project structure",
    "Set up a clean solution using layered or modular architecture with presentation, application, domain, infrastructure, and persistence components.",
    "",
    "4. Model the core entities",
    "Create the core domain models such as JobDescription, Candidate, CVDocument, Skill, Experience, Project, MatchResult, and RankingReport.",
    "",
    "5. Build file upload and storage",
    "Implement secure upload support for job descriptions and CV files, including validation for supported formats such as PDF and DOCX.",
    "",
    "6. Implement CV parsing",
    "Extract raw text from uploaded CV files, normalize the content, and prepare it for structured analysis across different document layouts.",
    "",
    "7. Extract structured candidate information",
    "Identify and capture skills, years of experience, education, certifications, projects, and other useful hiring signals from parsed CV content.",
    "",
    "8. Process job descriptions",
    "Analyze the job description to detect required skills, preferred qualifications, role keywords, experience expectations, and important responsibilities.",
    "",
    "9. Build the matching engine",
    "Compare each candidate profile against job requirements using weighted scoring rules for skills, experience, project relevance, and keyword alignment.",
    "",
    "10. Add ranking explanations",
    "Generate clear explanations showing why a candidate matches or falls short, including strengths, missing skills, and experience gaps.",
    "",
    "11. Create the ranking and insights dashboard",
    "Design a recruiter-facing interface to display ranked candidates, detailed score breakdowns, strengths, gaps, and actionable insights.",
    "",
    "12. Add AI-powered enhancements",
    "Integrate AI capabilities to improve semantic CV understanding, flexible matching, and natural-language explanations beyond simple keyword rules.",
    "",
    "13. Persist results in the database",
    "Store uploaded jobs, CVs, extracted candidate profiles, evaluation results, and ranking reports for future access and comparison.",
    "",
    "14. Add validation, logging, and error handling",
    "Handle invalid uploads, parsing failures, duplicate documents, large batch processing, and operational logging for maintainability.",
    "",
    "15. Test the full workflow",
    "Validate the end-to-end process, including upload, parsing, extraction, scoring accuracy, ranking behavior, and edge-case handling.",
    "",
    "16. Improve scalability and modularity",
    "Prepare the platform for future expansion such as authentication, recruiter management, analytics, interview recommendations, and advanced reporting.",
    "",
    "17. Deploy and monitor the platform",
    "Deploy the application, configure infrastructure, monitor performance, and track system reliability and matching quality over time."
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
    $pageObjectNumber = $pageObjectNumbers[$pageIndex]
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
