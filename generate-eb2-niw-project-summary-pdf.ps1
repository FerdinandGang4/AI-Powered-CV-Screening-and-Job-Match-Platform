$outputPath = Join-Path $PSScriptRoot "EB2-NIW-Project-Summary.pdf"

$rawLines = @(
    "EB-2 NIW Project Summary",
    "",
    "Proposed Endeavor Statement",
    "",
    "My proposed endeavor is to design, develop, and advance AI-assisted software systems that improve workforce screening, candidate-job matching, and hiring efficiency in the United States. My work focuses on practical web-based tools that help recruiters and employers process job descriptions and resumes more efficiently, extract relevant qualifications, rank candidates against role requirements, and generate clear explanations of strengths and gaps. Through this endeavor, I seek to apply software engineering, AI-enabled document analysis, and scalable application architecture to reduce manual recruiting burdens, improve consistency in candidate evaluation, and support more effective talent selection across U.S. organizations.",
    "",
    "This endeavor has substantial merit because it addresses operational inefficiencies in recruitment and hiring, which affect productivity, workforce placement, and business performance. It has national importance because scalable improvements in hiring workflows can benefit employers across industries, support labor-market efficiency, and strengthen the ability of U.S. organizations to identify qualified talent more effectively.",
    "",
    "Project Evidence Section",
    "",
    "As evidence of concrete progress toward my proposed endeavor, I developed an AI-powered CV screening and job matching platform using ASP.NET for backend services and React for the frontend interface. The system enables recruiters to provide a job description, upload multiple CVs, parse candidate information, extract skills and experience, evaluate applicants against role requirements, rank candidates, and generate recruiter-friendly explanations of candidate strengths and gaps.",
    "",
    "The project includes a structured backend architecture, authentication workflow, file upload pipeline, PDF resume handling, ranking logic, OpenAI-assisted evaluation support, and a recruiter-facing interface for screening and reviewing candidates. I also produced supporting technical documentation, including architecture diagrams, class diagrams, and workflow diagrams, demonstrating organized system design and implementation maturity.",
    "",
    "This project shows that I have already moved beyond a conceptual idea and have taken meaningful steps to implement a practical solution aligned with my proposed endeavor. It demonstrates technical capability, applied software engineering skill, and the ability to translate an important workforce-related problem into a functioning technology platform.",
    "",
    "Personal Statement Linking Background",
    "",
    "My academic and technical background has prepared me to advance this endeavor in a meaningful and practical way. Through my training and software development experience, I have built skills in backend engineering, frontend development, database-driven systems, scalable application design, and applied AI integration. These skills directly support my ability to create software tools that address real operational needs in hiring and workforce systems.",
    "",
    "The AI-powered CV screening and job matching platform reflects the intersection of my technical background and my long-term professional goal: building practical, intelligent systems that improve business processes and decision support. My work on this platform demonstrates not only my software engineering capability, but also my readiness to continue developing solutions with broader value to U.S. employers and institutions. Because I have already designed and implemented a working version of this type of system, I am well positioned to continue advancing this endeavor in the United States.",
    "",
    "Suggested Supporting Evidence",
    "",
    "1. GitHub repository link and commit history",
    "2. Screenshots of the working system",
    "3. Architecture, class, and sequence diagrams",
    "4. Demo video or hosted demo if available",
    "5. Testing results and sample ranking outputs",
    "6. Letters from recruiters, professors, engineers, or employers",
    "7. Roadmap showing future deployment and scale",
    "8. Proof of user interest, pilot interest, or intended organizational use",
    "",
    "USCIS Reference Points",
    "",
    "USCIS evaluates NIW petitions under the Matter of Dhanasar framework. Helpful official sources include the USCIS Policy Manual, Volume 6, Part F, Chapter 5, the USCIS EB-2 page, and the USCIS update on EB-2 NIW petitions dated January 15, 2025."
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
