$outputPath = Join-Path $PSScriptRoot "Recruiter-Interview-Project-Explanation.pdf"

$rawLines = @(
    "Recruiter Interview Project Explanation",
    "",
    "Project Introduction",
    "",
    "This project is an AI-Powered CV Screening and Job Match Platform built with an ASP.NET backend and a React frontend. The purpose of the system is to help recruiters upload a job description, submit multiple CVs, automatically analyze candidates, rank them against the role, and explain the reasons behind the ranking.",
    "",
    "Simple Interview Script",
    "",
    "1. Problem Statement",
    "I built this project to solve a common recruiting problem. Recruiters often spend a lot of time manually reviewing large numbers of resumes, and that process can be slow, inconsistent, and difficult to scale. My project automates that workflow by allowing recruiters to upload a job description and multiple CVs, then automatically score and rank the candidates.",
    "",
    "2. Frontend Explanation",
    "The frontend is built with React and provides a simple recruiter workflow. First, the recruiter logs into the system. Then they provide a job description either by uploading a file or pasting the text directly. After that, they upload multiple candidate CVs in one batch. Once screening starts, the UI shows progress stages such as file upload, parsing, scoring, and ranking. Finally, the results page displays ranked candidates, match percentages, strengths, gaps, and explanations.",
    "",
    "I designed the frontend to stay simple and professional. Instead of overwhelming the user with too many controls, I focused on a clear step-by-step flow: provide the job description, upload CVs, run screening, and review ranked results.",
    "",
    "Key frontend points:",
    "React was used to build reusable UI components.",
    "State management is handled through React hooks.",
    "The UI supports authentication, upload forms, progress feedback, ranking results, and reset behavior.",
    "The navbar navigation scrolls users to the correct sections.",
    "The interface is restricted so recruiters must log in before using the main workflow.",
    "",
    "3. Backend Explanation",
    "The backend is built with ASP.NET Core as a Web API. It handles authentication, file uploads, CV parsing, candidate evaluation, and ranking generation. The backend exposes endpoints for sign-up, login, logout, screening requests, and ranking report retrieval. I structured the backend so that different responsibilities are separated into services, controllers, request models, and response models.",
    "",
    "When the recruiter submits data, the backend receives the job description and CV files, extracts text from the uploaded PDFs, builds candidate profiles, compares them against the job requirements, and returns ranked results to the frontend.",
    "",
    "Key backend points:",
    "ASP.NET Core controllers expose the API endpoints.",
    "Services handle business logic such as screening and authentication.",
    "PDF parsing is handled with PdfPig.",
    "OpenAI integration is used for AI-assisted ranking when configured.",
    "Authentication middleware protects the application endpoints.",
    "The backend currently uses in-memory storage for prototype simplicity.",
    "",
    "4. End-to-End Flow",
    "From an end-to-end perspective, the recruiter logs in, provides the job description, uploads CVs, and submits the batch. The React frontend sends the data to the ASP.NET backend using multipart form data. The backend parses the files, extracts candidate information, calls the ranking logic or AI ranking service, and generates a ranking report. The frontend then retrieves the report and displays the ranked candidates with explanations and percentage matches.",
    "",
    "5. Key Technical Highlights",
    "A few technical areas I focused on were authentication, multi-file upload handling, PDF text extraction, candidate ranking logic, and frontend usability. I also added progress feedback in the UI so users can see the ranking process while the backend is working. On the backend side, I used service-based design so that authentication, screening, and AI ranking remain separate concerns. That makes the project easier to maintain and extend.",
    "",
    "6. Why This Project Is Valuable",
    "This project demonstrates full-stack engineering skills because it includes frontend UI design, backend API development, authentication, document processing, AI integration, ranking logic, and recruiter-focused workflow design. It is also a practical product idea because it addresses a real business problem in hiring efficiency and candidate evaluation.",
    "",
    "7. Limitations And Future Improvements",
    "Right now, the project is a strong prototype. The backend still uses in-memory storage, so one improvement would be moving to a persistent database such as SQL Server. Another improvement would be full production-grade Google OAuth instead of a simplified Gmail flow. I would also improve PDF extraction further with OCR support for scanned resumes and expand the system with role management, analytics, and database-backed recruiter accounts.",
    "",
    "Short Version For Recruiters",
    "This is a full-stack AI-powered recruiting platform I built using ASP.NET Core and React. Recruiters can log in, provide a job description, upload multiple CVs, and get ranked candidates with explanations of strengths and gaps. The frontend manages the recruiter workflow and user experience, while the backend handles authentication, file processing, PDF parsing, ranking logic, and AI-assisted evaluation.",
    "",
    "Best Delivery Tip",
    "During an interview, explain the project in this order: business problem, user workflow, frontend role, backend role, technical highlights, and future improvements."
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
