import { useEffect, useMemo, useState } from 'react'
import AppLayout from './components/layout/AppLayout'
import './App.css'

const navigationItems = [
  { label: 'Upload', description: 'Job description and CV files', isActive: true },
  { label: 'Analysis', description: 'Parsing and evaluation flow' },
  { label: 'Ranking', description: 'Results and insights' },
]

const API_BASE_URL = 'http://localhost:5282/api'

const fallbackRankedCandidates = [
  {
    name: 'Amina Hassan',
    score: '91%',
    summary: 'Strong fit for ASP.NET Core, C#, SQL, and backend API development.',
    gaps: 'Minor Azure deployment depth gap.',
  },
  {
    name: 'David Mensah',
    score: '76%',
    summary: 'Good engineering background with partial ASP.NET experience.',
    gaps: 'Needs stronger backend specialization and more years of experience.',
  },
  {
    name: 'Grace Njeri',
    score: '68%',
    summary: 'Relevant project work and solid problem-solving profile.',
    gaps: 'Missing some required framework skills and less direct role alignment.',
  },
  {
    name: 'Mark Otieno',
    score: '64%',
    summary: 'Useful software engineering foundation and some API exposure.',
    gaps: 'Gap in ASP.NET Core depth and fewer relevant backend projects.',
  },
  {
    name: 'Lilian Boateng',
    score: '58%',
    summary: 'Shows transferable skills and a promising technical background.',
    gaps: 'Needs more direct C# backend experience and stronger role alignment.',
  },
]

function App() {
  const [jobPostings, setJobPostings] = useState([])
  const [selectedJobPostingId, setSelectedJobPostingId] = useState('')
  const [jobDescriptionFile, setJobDescriptionFile] = useState(null)
  const [candidateFiles, setCandidateFiles] = useState([])
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [uploadMessage, setUploadMessage] = useState('Select a job posting, attach the role description, and upload candidate CVs to generate a ranking report.')
  const [uploadError, setUploadError] = useState('')
  const [lastBatch, setLastBatch] = useState(null)
  const [rankingReport, setRankingReport] = useState(null)

  useEffect(() => {
    let ignore = false

    async function loadJobPostings() {
      try {
        const response = await fetch(`${API_BASE_URL}/jobpostings`)
        if (!response.ok) {
          throw new Error('Unable to load job postings from the backend.')
        }

        const data = await response.json()
        if (!ignore) {
          setJobPostings(data)
          if (data.length > 0) {
            setSelectedJobPostingId((current) => current || data[0].id)
          }
        }
      } catch (error) {
        if (!ignore) {
          setUploadError(error.message)
        }
      }
    }

    loadJobPostings()

    return () => {
      ignore = true
    }
  }, [])

  const quickStats = useMemo(() => [
    { value: selectedJobPostingId ? '1' : '0', label: 'Job description' },
    { value: `${candidateFiles.length}`, label: 'CV files uploaded' },
    { value: rankingReport ? `Top ${Math.min(rankingReport.rankedCandidates.length, 5)}` : 'Top 5', label: 'Shortlisted results' },
  ], [candidateFiles.length, rankingReport, selectedJobPostingId])

  const rankedCandidates = useMemo(() => {
    if (!rankingReport?.rankedCandidates?.length) {
      return fallbackRankedCandidates
    }

    return rankingReport.rankedCandidates.slice(0, 5).map((candidate) => {
      const percentage = `${Math.round(candidate.overallScore)}%`
      const explanation = candidate.explanation?.summary
        ?? `${candidate.candidate.fullName} was evaluated against the selected role.`
      const gapSummary = candidate.skillGaps?.length
        ? candidate.skillGaps.map((gap) => gap.skillName).join(', ')
        : 'No major matching gaps detected.'

      return {
        name: candidate.candidate.fullName,
        score: percentage,
        summary: explanation,
        gaps: gapSummary,
      }
    })
  }, [rankingReport])

  function handleCandidateFilesChange(event) {
    const newFiles = Array.from(event.target.files ?? [])
    if (newFiles.length === 0) {
      return
    }

    setCandidateFiles((currentFiles) => {
      const fileMap = new Map(
        currentFiles.map((file) => [`${file.name}-${file.size}-${file.lastModified}`, file]),
      )

      newFiles.forEach((file) => {
        fileMap.set(`${file.name}-${file.size}-${file.lastModified}`, file)
      })

      return Array.from(fileMap.values())
    })

    event.target.value = ''
  }

  function removeCandidateFile(fileToRemove) {
    setCandidateFiles((currentFiles) =>
      currentFiles.filter(
        (file) => `${file.name}-${file.size}-${file.lastModified}` !== fileToRemove,
      ),
    )
  }

  async function handleSubmit(event) {
    event.preventDefault()

    setUploadError('')

    if (!selectedJobPostingId) {
      setUploadError('Please select a job posting before uploading documents.')
      return
    }

    if (!jobDescriptionFile) {
      setUploadError('Please attach a job description file.')
      return
    }

    if (candidateFiles.length === 0) {
      setUploadError('Please upload at least one candidate CV.')
      return
    }

    const formData = new FormData()
    formData.append('jobPostingId', selectedJobPostingId)
    formData.append('jobDescriptionFile', jobDescriptionFile)

    candidateFiles.forEach((file, index) => {
      const baseName = file.name.replace(/\.[^/.]+$/, '').replace(/[-_]+/g, ' ').trim()
      const candidateName = baseName.length > 0 ? toTitleCase(baseName) : `Candidate ${index + 1}`
      const candidateEmail = `candidate${index + 1}@example.com`

      formData.append(`candidateCvs[${index}].candidateName`, candidateName)
      formData.append(`candidateCvs[${index}].candidateEmail`, candidateEmail)
      formData.append(`candidateCvs[${index}].cvFile`, file)
    })

    setIsSubmitting(true)
    setUploadMessage('Uploading files and generating ranking report...')

    try {
      const uploadResponse = await fetch(`${API_BASE_URL}/screening/batches`, {
        method: 'POST',
        body: formData,
      })

      if (!uploadResponse.ok) {
        throw new Error('The upload request failed. Please check the selected files and try again.')
      }

      const batchResult = await uploadResponse.json()
      setLastBatch(batchResult)

      const reportResponse = await fetch(`${API_BASE_URL}/screening/batches/${batchResult.batchId}/report`)
      if (!reportResponse.ok) {
        throw new Error('Files uploaded, but the ranking report could not be retrieved.')
      }

      const report = await reportResponse.json()
      setRankingReport(report)
      setUploadMessage(`Upload successful. Ranking report generated for ${report.totalCandidates} evaluated candidate(s).`)
    } catch (error) {
      setUploadError(error.message)
      setUploadMessage('Upload could not be completed.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <AppLayout
      brand="CV Match Platform"
      heading="AI-Powered CV Screening Platform"
      subheading="A simple recruiter workflow for uploading job requirements, screening multiple CVs, and reviewing ranked candidates with clear explanations."
      navigationItems={navigationItems}
      stats={quickStats}
    >
      <section className="hero-panel">
        <div className="hero-copy">
          <p className="eyebrow">Proposed Solution</p>
          <h2>Upload the role, screen the CVs, and review the ranking in one simple flow.</h2>
          <p>
            The platform helps recruiters upload a job description and multiple CVs, process
            candidate content, score each applicant against the role, and highlight strengths,
            missing skills, and experience gaps.
          </p>
        </div>

        <div className="hero-metrics" aria-label="Platform summary">
          <article className="metric-card emphasis">
            <span className="metric-label">Core functionality</span>
            <strong className="metric-value">Upload and rank</strong>
            <p className="metric-note">Focus on CV parsing, candidate scoring, ranking, and recruiter-friendly explanations.</p>
          </article>
          <article className="metric-card">
            <span className="metric-label">System behavior</span>
            <strong className="metric-value">Explain the results</strong>
            <p className="metric-note">Show why a candidate matches, what is missing, and how the ranking was decided.</p>
          </article>
        </div>
      </section>

      <form className="upload-form" onSubmit={handleSubmit}>
        <section className="simple-grid">
          <article className="workspace-card upload-card">
            <p className="card-kicker">Step 1</p>
            <h3>Upload Job Description</h3>
            <p>
              Select the role being screened, then attach the job description file so candidate evaluation uses the right requirements.
            </p>
            <label className="field-label" htmlFor="job-posting">Job Posting</label>
            <select
              id="job-posting"
              className="form-select"
              value={selectedJobPostingId}
              onChange={(event) => setSelectedJobPostingId(event.target.value)}
            >
              <option value="">Select a job posting</option>
              {jobPostings.map((jobPosting) => (
                <option key={jobPosting.id} value={jobPosting.id}>
                  {jobPosting.title}
                </option>
              ))}
            </select>
            <label className="field-label" htmlFor="job-description-file">Job Description File</label>
            <input
              id="job-description-file"
              className="form-input"
              type="file"
              accept=".pdf,.doc,.docx,.txt"
              onChange={(event) => setJobDescriptionFile(event.target.files?.[0] ?? null)}
            />
            <div className="mock-dropzone">
              <span>{jobDescriptionFile ? `Selected: ${jobDescriptionFile.name}` : 'Attach the job description document'}</span>
            </div>
          </article>

          <article className="workspace-card upload-card">
            <p className="card-kicker">Step 2</p>
            <h3>Upload Multiple CVs</h3>
            <p>
              Add candidate CVs in one batch so the backend can parse and compare them against the selected job posting.
            </p>
            <label className="field-label" htmlFor="candidate-cv-files">Candidate CV Files</label>
            <input
              id="candidate-cv-files"
              className="form-input"
              type="file"
              accept=".pdf,.doc,.docx,.txt"
              multiple
              onChange={handleCandidateFilesChange}
            />
            <div className="mock-dropzone mock-dropzone--list">
              {candidateFiles.length > 0 ? (
                <ul className="file-list">
                  {candidateFiles.map((file) => (
                    <li key={`${file.name}-${file.lastModified}`} className="file-list__item">
                      <span>{file.name}</span>
                      <button
                        type="button"
                        className="file-remove"
                        onClick={() => removeCandidateFile(`${file.name}-${file.size}-${file.lastModified}`)}
                      >
                        Remove
                      </button>
                    </li>
                  ))}
                </ul>
              ) : (
                <span>Upload multiple CVs for screening</span>
              )}
            </div>
            <p className="helper-text">You can select more files again later and they will be added to the list.</p>
          </article>

          <article className="workspace-card process-card">
            <p className="card-kicker">Step 3</p>
            <h3>Run Screening</h3>
            <p>
              Submit the selected files to the ASP.NET backend. The platform will parse, score, explain, and rank the available candidates.
            </p>
            <ul className="process-list">
              <li>Parse CV content</li>
              <li>Extract skills, projects, and experience</li>
              <li>Score candidate relevance</li>
              <li>Generate explanations and gaps</li>
            </ul>
            <button className="submit-button" type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Processing Upload...' : 'Upload And Generate Ranking'}
            </button>
            <p className="upload-status">{uploadMessage}</p>
            {uploadError ? <p className="upload-error">{uploadError}</p> : null}
            {lastBatch ? (
              <div className="upload-summary">
                <strong>Latest Batch:</strong> {lastBatch.batchId}
              </div>
            ) : null}
          </article>
        </section>
      </form>

      <section className="results-section">
        <div className="results-header">
          <div>
            <p className="card-kicker">Step 4</p>
            <h3>Top 5 Ranked Candidates</h3>
          </div>
          <p className="results-copy">
            The recruiter can quickly review the first five candidates by match percentage, ranking position, and key gaps.
          </p>
        </div>

        <div className="candidate-results">
          {rankedCandidates.map((candidate, index) => (
            <article key={candidate.name} className="candidate-card">
              <div className="candidate-card__top">
                <span className="candidate-rank">#{index + 1}</span>
                <span className="candidate-score">{candidate.score} Match</span>
              </div>
              <h4>{candidate.name}</h4>
              <p className="candidate-summary">{candidate.summary}</p>
              <div className="candidate-progress" aria-hidden="true">
                <div className="candidate-progress__fill" style={{ width: candidate.score }} />
              </div>
              <p className="candidate-gap">
                <strong>Gap:</strong> {candidate.gaps}
              </p>
            </article>
          ))}
        </div>
      </section>

      <section className="insight-strip">
        <article className="insight-card">
          <p className="card-kicker">Actionable Insight</p>
          <h3>Why this frontend is simpler</h3>
          <p>
            It focuses only on the essential journey from upload to ranked output, which matches your problem statement and keeps the MVP easy to explain and implement.
          </p>
        </article>
        <article className="insight-card">
          <p className="card-kicker">Technology Direction</p>
          <h3>Built for ASP.NET plus React</h3>
          <p>
            React handles the user interface while ASP.NET manages parsing, matching, scoring, and future AI/ML integration through modular backend services.
          </p>
        </article>
      </section>

      <section className="creator-section">
        <article className="creator-card">
          <div className="creator-card__header">
            <div className="creator-card__mark" aria-hidden="true">GD</div>
            <div>
              <p className="card-kicker">Developed By</p>
              <h3>Gang Ferdinand Dinga</h3>
              <p className="creator-role">Software Engineer | ASP.NET | React | Scalable Systems</p>
            </div>
          </div>
          <p className="creator-description">
            Focused on building practical software solutions with ASP.NET, React, and modular application architecture for real-world hiring workflows.
          </p>
          <div className="creator-links">
            <a href="mailto:ferdinandgang4@gmail.com">Email</a>
            <a href="tel:+16412332357">Phone</a>
            <a href="https://www.linkedin.com/in/ferdinand-dinga-gang-91a912185/" target="_blank" rel="noreferrer">LinkedIn Profile</a>
            <a href="https://github.com/FerdinandGang4/" target="_blank" rel="noreferrer">GitHub Portfolio</a>
          </div>
          <div className="creator-meta">
            <span>ferdinandgang4@gmail.com</span>
            <span>+1 641 233 2357</span>
          </div>
        </article>
      </section>

      <footer className="page-footer">
        <div>
          <p className="page-footer__brand">AI-Powered CV Screening Platform</p>
          <p className="page-footer__credit">Powered by Engineer Gang Ferdinand Dinga</p>
        </div>
        <div className="page-footer__details">
          <p className="page-footer__copy">
            Upload job descriptions, screen multiple CVs, rank candidates, and highlight matching gaps in one streamlined workflow.
          </p>
          <div className="page-footer__contact">
            <span>Gang Ferdinand Dinga</span>
            <a href="mailto:ferdinandgang4@gmail.com">ferdinandgang4@gmail.com</a>
            <a href="tel:+16412332357">+1 641 233 2357</a>
            <a href="https://www.linkedin.com/in/ferdinand-dinga-gang-91a912185/" target="_blank" rel="noreferrer">LinkedIn</a>
            <a href="https://github.com/FerdinandGang4/" target="_blank" rel="noreferrer">GitHub</a>
          </div>
        </div>
      </footer>
    </AppLayout>
  )
}

function toTitleCase(value) {
  return value
    .split(' ')
    .filter(Boolean)
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1).toLowerCase())
    .join(' ')
}

export default App
