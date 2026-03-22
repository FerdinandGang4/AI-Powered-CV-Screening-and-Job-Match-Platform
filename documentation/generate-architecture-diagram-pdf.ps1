$outputPath = Join-Path $PSScriptRoot "architecture-diagram-labeled.pdf"

function Escape-PdfText {
    param([string]$Text)
    return $Text.Replace('\', '\\').Replace('(', '\(').Replace(')', '\)')
}

$content = New-Object System.Collections.Generic.List[string]

$content.Add("0.97 0.96 0.93 rg")
$content.Add("0 0 612 792 re f")

$content.Add("BT")
$content.Add("/F2 22 Tf")
$content.Add("72 748 Td")
$content.Add("(AI-Powered CV Screening Platform) Tj")
$content.Add("ET")

$content.Add("BT")
$content.Add("/F1 13 Tf")
$content.Add("72 724 Td")
$content.Add("(Architecture Diagram) Tj")
$content.Add("ET")

$content.Add("0.78 0.88 0.99 rg 0.54 0.69 0.84 RG 1.8 w")
$content.Add("40 620 125 68 re B")
$content.Add("BT /F1 9 Tf 78 672 Td (<<actor>>) Tj ET")
$content.Add("BT /F2 13 Tf 66 653 Td (Recruiter) Tj ET")
$content.Add("BT /F1 9 Tf 50 636 Td (Uploads JD and CV batch) Tj ET")

$content.Add("0.91 0.97 0.94 rg 0.52 0.72 0.66 RG 1.8 w")
$content.Add("205 606 200 98 re B")
$content.Add("BT /F1 9 Tf 270 687 Td (<<component>>) Tj ET")
$content.Add("BT /F2 14 Tf 242 666 Td (ASP.NET Web App / API) Tj ET")
$content.Add("BT /F1 9 Tf 225 648 Td (Auth, upload endpoints,) Tj ET")
$content.Add("BT /F1 9 Tf 229 634 Td (workflow orchestration,) Tj ET")
$content.Add("BT /F1 9 Tf 266 620 Td (dashboard) Tj ET")

$content.Add("0.97 0.91 0.94 rg 0.77 0.58 0.66 RG 1.8 w")
$content.Add("445 620 125 68 re B")
$content.Add("BT /F1 9 Tf 479 672 Td (<<database>>) Tj ET")
$content.Add("BT /F2 13 Tf 480 653 Td (Storage) Tj ET")
$content.Add("BT /F1 9 Tf 459 636 Td (Files, parsed data, scores) Tj ET")

$content.Add("0.91 0.97 0.94 rg 0.52 0.72 0.66 RG 1.8 w")
$content.Add("50 450 140 88 re B")
$content.Add("BT /F1 9 Tf 89 520 Td (<<component>>) Tj ET")
$content.Add("BT /F2 12 Tf 77 501 Td (CV Parsing Service) Tj ET")
$content.Add("BT /F1 9 Tf 66 483 Td (Extracts text from PDF / DOCX) Tj ET")

$content.Add("1.00 0.94 0.85 rg 0.82 0.67 0.40 RG 1.8 w")
$content.Add("235 450 140 88 re B")
$content.Add("BT /F1 9 Tf 278 520 Td (<<service>>) Tj ET")
$content.Add("BT /F2 12 Tf 266 501 Td (AI / NLP Layer) Tj ET")
$content.Add("BT /F1 9 Tf 248 483 Td (Skills, experience, projects,) Tj ET")
$content.Add("BT /F1 9 Tf 288 469 Td (gaps) Tj ET")

$content.Add("0.91 0.97 0.94 rg 0.52 0.72 0.66 RG 1.8 w")
$content.Add("420 450 140 88 re B")
$content.Add("BT /F1 9 Tf 459 520 Td (<<component>>) Tj ET")
$content.Add("BT /F2 12 Tf 450 501 Td (Matching Engine) Tj ET")
$content.Add("BT /F1 9 Tf 440 483 Td (Scoring and ranking logic) Tj ET")

$content.Add("1.00 0.94 0.85 rg 0.82 0.67 0.40 RG 1.8 w")
$content.Add("110 290 165 88 re B")
$content.Add("BT /F1 9 Tf 160 360 Td (<<service>>) Tj ET")
$content.Add("BT /F2 12 Tf 138 341 Td (Explanation Engine) Tj ET")
$content.Add("BT /F1 9 Tf 122 323 Td (Strengths, missing skills,) Tj ET")
$content.Add("BT /F1 9 Tf 164 309 Td (gap notes) Tj ET")

$content.Add("0.78 0.88 0.99 rg 0.54 0.69 0.84 RG 1.8 w")
$content.Add("335 290 165 88 re B")
$content.Add("BT /F1 9 Tf 378 360 Td (<<boundary>>) Tj ET")
$content.Add("BT /F2 12 Tf 367 341 Td (Recruiter Dashboard) Tj ET")
$content.Add("BT /F1 9 Tf 351 323 Td (Ranked list, insights, actions) Tj ET")

$content.Add("0.43 0.45 0.50 RG 2 w")
$content.Add("165 654 m 205 654 l S")
$content.Add("405 654 m 445 654 l S")
$content.Add("305 606 m 305 538 l S")
$content.Add("250 606 m 150 538 l S")
$content.Add("360 606 m 490 538 l S")
$content.Add("190 494 m 235 494 l S")
$content.Add("375 494 m 420 494 l S")
$content.Add("305 450 m 220 378 l S")
$content.Add("490 450 m 418 378 l S")
$content.Add("335 334 m 275 334 l S")

$content.Add("BT /F1 9 Tf 168 664 Td (submits files) Tj ET")
$content.Add("BT /F1 9 Tf 411 664 Td (stores artifacts) Tj ET")
$content.Add("BT /F1 9 Tf 312 562 Td (calls NLP pipeline) Tj ET")
$content.Add("BT /F1 9 Tf 177 560 Td (sends CV files) Tj ET")
$content.Add("BT /F1 9 Tf 382 560 Td (requests ranking) Tj ET")
$content.Add("BT /F1 9 Tf 196 502 Td (parsed text) Tj ET")
$content.Add("BT /F1 9 Tf 379 502 Td (candidate profile) Tj ET")
$content.Add("BT /F1 9 Tf 240 399 Td (fit analysis) Tj ET")
$content.Add("BT /F1 9 Tf 418 399 Td (ranking result) Tj ET")
$content.Add("BT /F1 9 Tf 284 342 Td (explanations) Tj ET")

$content.Add("0.43 0.45 0.50 RG 1.5 w [5 4] 0 d")
$content.Add("300 494 m 505 688 l S")
$content.Add("490 494 m 505 688 l S")
$content.Add("418 334 m 505 688 l S")
$content.Add("[] 0 d")

$content.Add("BT /F1 9 Tf 352 592 Td (persists extracted fields) Tj ET")
$content.Add("BT /F1 9 Tf 490 558 Td (writes scores) Tj ET")
$content.Add("BT /F1 9 Tf 462 430 Td (loads reports) Tj ET")

$content.Add("BT")
$content.Add("/F2 14 Tf")
$content.Add("72 252 Td")
$content.Add("(Component Summary) Tj")
$content.Add("ET")

$summaryLines = @(
    "Recruiter uploads a job description and multiple CVs into the platform.",
    "The ASP.NET application coordinates upload, validation, workflow orchestration, and the recruiter UI.",
    "A parsing service extracts text from CV files, then the AI/NLP layer identifies skills, experience, projects, and gaps.",
    "The matching engine compares each candidate profile with the job requirements and calculates ranking scores.",
    "An explanation engine produces recruiter-friendly reasons for each result, and the dashboard presents ranked outcomes.",
    "Database and file storage persist the original documents, extracted candidate data, scores, and generated reports."
)

$y = 232
foreach ($line in $summaryLines) {
    $escaped = Escape-PdfText $line
    $content.Add("BT /F1 10 Tf 82 $y Td ($escaped) Tj ET")
    $y -= 16
}

$streamText = ($content -join "`n") + "`n"
$streamLength = [System.Text.Encoding]::ASCII.GetByteCount($streamText)

$objects = @(
    "<< /Type /Catalog /Pages 2 0 R >>",
    "<< /Type /Pages /Kids [ 3 0 R ] /Count 1 >>",
    "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R /F2 5 0 R >> >> /Contents 6 0 R >>",
    "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
    "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>",
    "<< /Length $streamLength >>`nstream`n$streamText" + "endstream"
)

$builder = New-Object System.Text.StringBuilder
[void]$builder.Append("%PDF-1.4`n")

$offsets = New-Object System.Collections.Generic.List[int]
for ($i = 0; $i -lt $objects.Count; $i++) {
    $offsets.Add([System.Text.Encoding]::ASCII.GetByteCount($builder.ToString()))
    $objectNumber = $i + 1
    [void]$builder.Append("$objectNumber 0 obj`n")
    [void]$builder.Append($objects[$i])
    [void]$builder.Append("`nendobj`n")
}

$xrefOffset = [System.Text.Encoding]::ASCII.GetByteCount($builder.ToString())
[void]$builder.Append("xref`n")
[void]$builder.Append("0 7`n")
[void]$builder.Append("0000000000 65535 f `n")
foreach ($offset in $offsets) {
    [void]$builder.Append(("{0:0000000000} 00000 n `n" -f $offset))
}
[void]$builder.Append("trailer`n")
[void]$builder.Append("<< /Size 7 /Root 1 0 R >>`n")
[void]$builder.Append("startxref`n")
[void]$builder.Append("$xrefOffset`n")
[void]$builder.Append("%%EOF")

[System.IO.File]::WriteAllBytes($outputPath, [System.Text.Encoding]::ASCII.GetBytes($builder.ToString()))
Write-Output "Created $outputPath"
