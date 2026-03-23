import { useEffect, useMemo, useRef, useState } from 'react'
import AppLayout from './components/layout/AppLayout'
import './App.css'

const navigationItems = [
  { label: 'Upload', description: 'Job description and CV files', isActive: true, targetId: 'upload-workflow' },
  { label: 'Analysis', description: 'Parsing and evaluation flow', targetId: 'analysis-workflow' },
  { label: 'Ranking', description: 'Results and insights', targetId: 'ranking-results' },
]

const API_BASE_URL = 'http://localhost:5282/api'

const AUTH_STORAGE_KEY = 'cvscreening_recruiter_auth'

function App() {
  const jobDescriptionFileInputRef = useRef(null)
  const candidateFilesInputRef = useRef(null)
  const [jobDescriptionInputMode, setJobDescriptionInputMode] = useState('upload')
  const [jobDescriptionFile, setJobDescriptionFile] = useState(null)
  const [jobDescriptionText, setJobDescriptionText] = useState('')
  const [candidateFiles, setCandidateFiles] = useState([])
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [screeningProgress, setScreeningProgress] = useState(0)
  const [screeningStage, setScreeningStage] = useState('Ready to screen')
  const [uploadMessage, setUploadMessage] = useState('Add a job description, upload candidate CVs, and generate a ranking report.')
  const [uploadError, setUploadError] = useState('')
  const [lastBatch, setLastBatch] = useState(null)
  const [rankingReport, setRankingReport] = useState(null)
  const [candidateDisplayCount, setCandidateDisplayCount] = useState(6)
  const [currentUser, setCurrentUser] = useState(null)
  const [authToken, setAuthToken] = useState('')
  const [showLoginModal, setShowLoginModal] = useState(false)
  const [loginForm, setLoginForm] = useState({
    email: '',
    password: '',
  })
  const [loginMessage, setLoginMessage] = useState('')
  const [loginError, setLoginError] = useState('')
  const [isLoggingIn, setIsLoggingIn] = useState(false)
  const [showSignUpModal, setShowSignUpModal] = useState(false)
  const [signUpForm, setSignUpForm] = useState({
    fullName: '',
    companyName: '',
    email: '',
    password: '',
    confirmPassword: '',
  })
  const [signUpMessage, setSignUpMessage] = useState('')
  const [signUpError, setSignUpError] = useState('')
  const [isSigningUp, setIsSigningUp] = useState(false)
  const [showGoogleModal, setShowGoogleModal] = useState(false)
  const [googleForm, setGoogleForm] = useState({
    fullName: '',
    companyName: '',
    email: '',
  })
  const [googleMessage, setGoogleMessage] = useState('')
  const [googleError, setGoogleError] = useState('')
  const [isSigningInWithGoogle, setIsSigningInWithGoogle] = useState(false)

  useEffect(() => {
    const savedAuth = window.localStorage.getItem(AUTH_STORAGE_KEY)
    if (!savedAuth) {
      return
    }

    try {
      const parsedAuth = JSON.parse(savedAuth)
      if (parsedAuth?.token && parsedAuth?.fullName) {
        setAuthToken(parsedAuth.token)
        setCurrentUser({
          fullName: parsedAuth.fullName,
          companyName: parsedAuth.companyName,
          email: parsedAuth.email,
        })
      }
    } catch {
      window.localStorage.removeItem(AUTH_STORAGE_KEY)
    }
  }, [])

  useEffect(() => {
    if (!isSubmitting) {
      return undefined
    }

    const stages = [
      { progress: 12, label: 'Uploading files to the backend...' },
      { progress: 32, label: 'Parsing the job description...' },
      { progress: 56, label: 'Extracting candidate CV content...' },
      { progress: 78, label: 'Scoring candidates against the role...' },
      { progress: 92, label: 'Generating the final ranking...' },
    ]

    let stageIndex = 0
    setScreeningProgress(stages[0].progress)
    setScreeningStage(stages[0].label)

    const intervalId = window.setInterval(() => {
      stageIndex += 1

      if (stageIndex >= stages.length) {
        window.clearInterval(intervalId)
        return
      }

      setScreeningProgress(stages[stageIndex].progress)
      setScreeningStage(stages[stageIndex].label)
    }, 900)

    return () => window.clearInterval(intervalId)
  }, [isSubmitting])

  function getAuthHeaders() {
    return authToken
      ? { Authorization: `Bearer ${authToken}` }
      : {}
  }

  const quickStats = useMemo(() => [
    { value: jobDescriptionFile || jobDescriptionText.trim() ? '1' : '0', label: 'Job description' },
    { value: `${candidateFiles.length}`, label: 'CV files uploaded' },
    { value: `Top ${candidateDisplayCount}`, label: 'Shortlisted results' },
  ], [candidateDisplayCount, candidateFiles.length, jobDescriptionFile, jobDescriptionText])

  const rankedCandidates = useMemo(() => {
    if (!rankingReport?.rankedCandidates?.length) {
      return []
    }

    return rankingReport.rankedCandidates.slice(0, candidateDisplayCount).map((candidate) => {
      const percentage = `${Math.round(candidate.overallScore)}%`
      const explanation = candidate.explanation?.summary
        ?? `${candidate.candidate.fullName} was evaluated against the provided job description.`
      const strengths = candidate.explanation?.strengths ?? 'No major strengths were highlighted.'
      const notes = candidate.explanation?.notes ?? 'No additional reviewer notes were generated.'
      const gapSummary = candidate.skillGaps?.length
        ? candidate.skillGaps.map((gap) => gap.skillName).join(', ')
        : 'No major matching gaps detected.'

      return {
        name: candidate.candidate.fullName,
        score: percentage,
        summary: explanation,
        recommendation: candidate.recommendation,
        strengths,
        notes,
        gaps: gapSummary,
        gapCount: candidate.skillGaps?.length ?? 0,
        extractionWarning: candidate.extractionWarning ?? '',
        scoreBreakdown: [
          { label: 'Skills', value: Math.round(candidate.skillScore ?? 0) },
          { label: 'Experience', value: Math.round(candidate.experienceScore ?? 0) },
          { label: 'Projects', value: Math.round(candidate.projectScore ?? 0) },
          { label: 'Semantic', value: Math.round(candidate.semanticScore ?? 0) },
        ],
      }
    })
  }, [candidateDisplayCount, rankingReport])

  const maxDisplayCount = rankingReport?.rankedCandidates?.length
    ? Math.min(6, Math.max(1, rankingReport.rankedCandidates.length))
    : 1

  const topCandidateName = useMemo(() => {
    if (!rankingReport?.rankedCandidates?.length) {
      return 'No candidate yet'
    }

    return rankingReport.rankedCandidates[0]?.candidate?.fullName ?? 'No candidate yet'
  }, [rankingReport])

  const extractionWarningCount = useMemo(
    () => rankingReport?.rankedCandidates?.filter((candidate) => candidate.extractionWarning).length ?? 0,
    [rankingReport],
  )

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

    if (!authToken) {
      setUploadError('Please log in before using the screening workflow.')
      setShowLoginModal(true)
      return
    }

    const hasUploadedJobDescription = Boolean(jobDescriptionFile)
    const hasPastedJobDescription = Boolean(jobDescriptionText.trim())

    if (jobDescriptionInputMode === 'upload' && !hasUploadedJobDescription) {
      setUploadError('Please upload a job description file.')
      return
    }

    if (jobDescriptionInputMode === 'paste' && !hasPastedJobDescription) {
      setUploadError('Please paste the job description text.')
      return
    }

    if (candidateFiles.length === 0) {
      setUploadError('Please upload at least one candidate CV.')
      return
    }

    setIsSubmitting(true)
    setScreeningProgress(10)
    setScreeningStage('Uploading files to the backend...')
    setUploadMessage('Uploading files and generating ranking report...')

    try {
      const formData = new FormData()

      if (jobDescriptionInputMode === 'upload' && jobDescriptionFile) {
        formData.append('jobDescriptionFile', jobDescriptionFile)
      }

      if (jobDescriptionInputMode === 'paste' && jobDescriptionText.trim()) {
        formData.append('jobDescriptionText', jobDescriptionText.trim())
      }

      candidateFiles.forEach((file, index) => {
        const baseName = file.name.replace(/\.[^/.]+$/, '').replace(/[-_]+/g, ' ').trim()
        const candidateName = baseName.length > 0 ? toTitleCase(baseName) : `Candidate ${index + 1}`
        const candidateEmail = `candidate${index + 1}@example.com`

        formData.append(`candidateCvs[${index}].candidateName`, candidateName)
        formData.append(`candidateCvs[${index}].candidateEmail`, candidateEmail)
        formData.append(`candidateCvs[${index}].cvFile`, file)
      })

      const uploadResponse = await fetch(`${API_BASE_URL}/screening/batches`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: formData,
      })

      if (!uploadResponse.ok) {
        let errorMessage = 'The upload request failed. Please check the selected files and try again.'

        try {
          const errorBody = await uploadResponse.json()
          if (errorBody?.message) {
            errorMessage = errorBody.message
          }
        } catch {
          // Ignore JSON parsing errors and keep the default message.
        }

        throw new Error(errorMessage)
      }

      const batchResult = await uploadResponse.json()
      setLastBatch(batchResult)

      const reportResponse = await fetch(`${API_BASE_URL}/screening/batches/${batchResult.batchId}/report`, {
        headers: getAuthHeaders(),
      })
      if (!reportResponse.ok) {
        throw new Error('Files uploaded, but the ranking report could not be retrieved.')
      }

      const report = await reportResponse.json()
      setRankingReport(report)
      setScreeningProgress(100)
      setScreeningStage('Ranking complete')
      setUploadMessage(`Upload successful. Ranking report generated for ${report.totalCandidates} evaluated candidate(s).`)
    } catch (error) {
      setScreeningProgress(0)
      setScreeningStage('Screening could not be completed')
      setUploadError(error.message)
      setUploadMessage('Upload could not be completed.')
    } finally {
      setIsSubmitting(false)
    }
  }

  function handleResetWorkflow() {
    if (jobDescriptionFileInputRef.current) {
      jobDescriptionFileInputRef.current.value = ''
    }

    if (candidateFilesInputRef.current) {
      candidateFilesInputRef.current.value = ''
    }

    setJobDescriptionInputMode('upload')
    setJobDescriptionFile(null)
    setJobDescriptionText('')
    setCandidateFiles([])
    setIsSubmitting(false)
    setScreeningProgress(0)
    setScreeningStage('Ready to screen')
    setUploadError('')
    setLastBatch(null)
    setRankingReport(null)
    setCandidateDisplayCount(6)
    setUploadMessage('Add a job description, upload candidate CVs, and generate a ranking report.')
  }

  async function handleLogout() {
    const tokenToLogout = authToken

    try {
      if (tokenToLogout) {
        await fetch(`${API_BASE_URL}/auth/logout`, {
          method: 'POST',
          headers: {
            Authorization: `Bearer ${tokenToLogout}`,
          },
        })
      }
    } catch {
      // Ignore logout API failures and still clear the local session.
    }

    window.localStorage.removeItem(AUTH_STORAGE_KEY)
    setAuthToken('')
    setCurrentUser(null)
    setShowLoginModal(false)
    setShowSignUpModal(false)
    setShowGoogleModal(false)
    setLoginError('')
    setLoginMessage('')
    setSignUpError('')
    setSignUpMessage('')
    setGoogleError('')
    setGoogleMessage('')
    handleResetWorkflow()
    setUploadMessage('You have been logged out. Please log in again to use the application.')
  }

  function handleOpenSignUpModal() {
    setShowLoginModal(false)
    setShowGoogleModal(false)
    setSignUpError('')
    setSignUpMessage('')
    setShowSignUpModal(true)
  }

  function handleOpenLoginModal() {
    setShowSignUpModal(false)
    setShowGoogleModal(false)
    setLoginError('')
    setLoginMessage('')
    setShowLoginModal(true)
  }

  function handleOpenGoogleModal() {
    setShowLoginModal(false)
    setShowSignUpModal(false)
    setGoogleError('')
    setGoogleMessage('')
    setShowGoogleModal(true)
  }

  function persistAuth(payload) {
    const authState = {
      token: payload.token,
      fullName: payload.fullName,
      companyName: payload.companyName,
      email: payload.email,
    }

    setAuthToken(payload.token)
    setCurrentUser({
      fullName: payload.fullName,
      companyName: payload.companyName,
      email: payload.email,
    })
    window.localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(authState))
  }

  function handleCloseLoginModal() {
    setShowLoginModal(false)
    setLoginError('')
    setLoginMessage('')
    setIsLoggingIn(false)
    setLoginForm({
      email: '',
      password: '',
    })
  }

  function handleCloseSignUpModal() {
    setShowSignUpModal(false)
    setSignUpError('')
    setSignUpMessage('')
    setIsSigningUp(false)
    setSignUpForm({
      fullName: '',
      companyName: '',
      email: '',
      password: '',
      confirmPassword: '',
    })
  }

  function handleCloseGoogleModal() {
    setShowGoogleModal(false)
    setGoogleError('')
    setGoogleMessage('')
    setIsSigningInWithGoogle(false)
    setGoogleForm({
      fullName: '',
      companyName: '',
      email: '',
    })
  }

  function handleLoginClick() {
    handleOpenLoginModal()
  }

  async function handleGoogleSignInSubmit(event) {
    event.preventDefault()
    setGoogleError('')
    setGoogleMessage('')

    if (!googleForm.email.trim()) {
      setGoogleError('Please enter your Gmail address.')
      return
    }

    if (!googleForm.email.trim().toLowerCase().endsWith('@gmail.com')) {
      setGoogleError('Please use a Gmail address ending with @gmail.com.')
      return
    }

    setIsSigningInWithGoogle(true)

    try {
      const response = await fetch(`${API_BASE_URL}/auth/google`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          fullName: googleForm.fullName.trim(),
          companyName: googleForm.companyName.trim(),
          email: googleForm.email.trim(),
        }),
      })

      const payload = await parseJsonResponse(response)
      if (!response.ok) {
        throw new Error(payload?.message || 'Google sign-in could not be completed.')
      }

      persistAuth(payload)
      setGoogleMessage(payload.message || 'Signed in with Gmail.')
      setUploadMessage(`Welcome, ${payload.fullName}. You are now signed in with Gmail.`)
      setTimeout(() => {
        handleCloseGoogleModal()
      }, 1200)
    } catch (error) {
      setGoogleError(error.message)
    } finally {
      setIsSigningInWithGoogle(false)
    }
  }

async function handleSignUpSubmit(event) {
    event.preventDefault()
    setSignUpError('')
    setSignUpMessage('')

    if (!signUpForm.fullName.trim() || !signUpForm.companyName.trim() || !signUpForm.email.trim() || !signUpForm.password) {
      setSignUpError('Please fill in all sign up fields.')
      return
    }

    if (signUpForm.password.length < 8) {
      setSignUpError('Password must be at least 8 characters long.')
      return
    }

    if (signUpForm.password !== signUpForm.confirmPassword) {
      setSignUpError('Password confirmation does not match.')
      return
    }

    setIsSigningUp(true)

    try {
      const response = await fetch(`${API_BASE_URL}/auth/signup`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          fullName: signUpForm.fullName.trim(),
          companyName: signUpForm.companyName.trim(),
          email: signUpForm.email.trim(),
          password: signUpForm.password,
        }),
      })

      const payload = await parseJsonResponse(response)
      if (!response.ok) {
        throw new Error(payload?.message || 'Sign up could not be completed.')
      }

      setSignUpMessage(payload.message || 'Account created successfully.')
      setUploadMessage(`Account created for ${payload.fullName}. Please log in before using the application.`)
      setTimeout(() => {
        handleCloseSignUpModal()
        handleOpenLoginModal()
        setLoginForm((current) => ({
          ...current,
          email: signUpForm.email.trim(),
        }))
      }, 1200)
    } catch (error) {
      setSignUpError(error.message)
    } finally {
      setIsSigningUp(false)
    }
  }

  async function handleLoginSubmit(event) {
    event.preventDefault()
    setLoginError('')
    setLoginMessage('')

    if (!loginForm.email.trim() || !loginForm.password) {
      setLoginError('Please enter your email and password.')
      return
    }

    setIsLoggingIn(true)

    try {
      const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          email: loginForm.email.trim(),
          password: loginForm.password,
        }),
      })

      const payload = await parseJsonResponse(response)
      if (!response.ok) {
        throw new Error(payload?.message || 'Login could not be completed.')
      }

      persistAuth(payload)
      setLoginMessage(payload.message || 'Login successful.')
      setUploadMessage(`Welcome back, ${payload.fullName}. You are now logged in.`)
      setTimeout(() => {
        handleCloseLoginModal()
      }, 1200)
    } catch (error) {
      setLoginError(error.message)
    } finally {
      setIsLoggingIn(false)
    }
  }

  return (
    <AppLayout
      brand="CV Match Platform"
      heading="AI-Powered CV Screening Platform"
      subheading="A simple recruiter workflow for uploading job requirements, screening multiple CVs, and reviewing ranked candidates with clear explanations."
      navigationItems={navigationItems}
      stats={quickStats}
      onLoginClick={handleLoginClick}
      onSignUpClick={handleOpenSignUpModal}
      onLogoutClick={handleLogout}
      currentUser={currentUser}
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

      <form className="upload-form" id="upload-workflow" onSubmit={handleSubmit}>
        {!currentUser ? (
          <section className="auth-gate-card">
            <p className="card-kicker">Access Required</p>
            <h3>Sign up or log in before using the recruiter workflow</h3>
            <p>
              The backend now protects screening and candidate endpoints. Create a recruiter account or log in to continue.
            </p>
            <div className="hero-actions">
              <button type="button" className="primary-action" onClick={handleOpenSignUpModal}>Create Account</button>
              <button type="button" className="secondary-action" onClick={handleOpenLoginModal}>Log In</button>
              <button type="button" className="secondary-action" onClick={handleOpenGoogleModal}>Continue with Gmail</button>
            </div>
          </section>
        ) : null}

        <section className="simple-grid">
          <article className="workspace-card upload-card">
            <p className="card-kicker">Step 1</p>
            <h3>Upload Job Description</h3>
            <p>
              Add the role description in the simplest possible way. Choose one input method and provide the job description for screening.
            </p>
            <label className="field-label">Job Description Input</label>
            <div className="mode-toggle mode-toggle--compact">
              <button
                type="button"
                className={`mode-toggle__button${jobDescriptionInputMode === 'upload' ? ' is-active' : ''}`}
                onClick={() => {
                  setJobDescriptionInputMode('upload')
                  setJobDescriptionText('')
                }}
              >
                Upload File
              </button>
              <button
                type="button"
                className={`mode-toggle__button${jobDescriptionInputMode === 'paste' ? ' is-active' : ''}`}
                onClick={() => {
                  setJobDescriptionInputMode('paste')
                  setJobDescriptionFile(null)
                  if (jobDescriptionFileInputRef.current) {
                    jobDescriptionFileInputRef.current.value = ''
                  }
                }}
              >
                Paste Text
              </button>
            </div>
            <label className="field-label" htmlFor="job-description-file">Job Description File</label>
            <input
              id="job-description-file"
              ref={jobDescriptionFileInputRef}
              className="form-input"
              type="file"
              accept=".pdf,.doc,.docx,.txt"
              disabled={jobDescriptionInputMode !== 'upload'}
              onChange={(event) => {
                setJobDescriptionFile(event.target.files?.[0] ?? null)
                if (event.target.files?.[0]) {
                  setJobDescriptionText('')
                }
              }}
            />
            <label className="field-label" htmlFor="job-description-text">Paste Job Description</label>
            <textarea
              id="job-description-text"
              className="form-textarea"
              rows="6"
              placeholder="Paste the job description here."
              value={jobDescriptionText}
              disabled={jobDescriptionInputMode !== 'paste'}
              onChange={(event) => {
                setJobDescriptionText(event.target.value)
                if (event.target.value) {
                  setJobDescriptionFile(null)
                  if (jobDescriptionFileInputRef.current) {
                    jobDescriptionFileInputRef.current.value = ''
                  }
                }
              }}
            />
            <div className="mock-dropzone">
              <span>
                {jobDescriptionInputMode === 'upload' && jobDescriptionFile
                  ? `Selected file: ${jobDescriptionFile.name}`
                  : jobDescriptionInputMode === 'paste' && jobDescriptionText.trim()
                    ? 'Pasted job description will be used for screening.'
                    : jobDescriptionInputMode === 'upload'
                      ? 'Upload one job description file for screening.'
                      : 'Paste one job description for screening.'}
              </span>
            </div>
          </article>

          <article className="workspace-card upload-card">
            <p className="card-kicker">Step 2</p>
            <h3>Upload Multiple CVs</h3>
            <p>
              Add candidate CVs in one batch so the backend can parse and compare them against the uploaded or pasted job description.
            </p>
            <label className="field-label" htmlFor="candidate-cv-files">Candidate CV Files</label>
            <input
              id="candidate-cv-files"
              ref={candidateFilesInputRef}
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

          <article className="workspace-card process-card" id="analysis-workflow">
            <p className="card-kicker">Step 3</p>
            <h3>Run Screening</h3>
            <p>
              Submit the job description and CV files to the ASP.NET backend. The platform will parse, score, explain, and rank the available candidates.
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
            <button
              className="secondary-submit-button"
              type="button"
              onClick={handleResetWorkflow}
              disabled={isSubmitting}
            >
              Reset App
            </button>
            <div className="screening-progress" aria-live="polite">
              <div className="screening-progress__header">
                <span>Ranking Progress</span>
                <strong>{isSubmitting || screeningProgress > 0 ? `${screeningProgress}%` : '0%'}</strong>
              </div>
              <div className="screening-progress__track" aria-hidden="true">
                <div
                  className={`screening-progress__fill${isSubmitting ? ' is-active' : ''}`}
                  style={{ width: `${screeningProgress}%` }}
                />
              </div>
              <p className="screening-progress__label">
                {isSubmitting ? screeningStage : rankingReport ? 'Ranking complete' : 'Ready to start screening'}
              </p>
            </div>
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

      <section className="results-section" id="ranking-results">
        <div className="results-header">
          <div>
            <p className="card-kicker">Step 4</p>
            <h3>Top {candidateDisplayCount} Ranked Candidates</h3>
            <div className="results-badges">
              <span className={`status-badge ${rankingReport ? (rankingReport.aiUsed ? 'status-badge--ai' : 'status-badge--fallback') : ''}`}>
                {rankingReport ? (rankingReport.aiUsed ? 'AI Ranked' : 'Fallback Ranked') : 'No Ranking Yet'}
              </span>
              <span className="status-badge">{topCandidateName}</span>
              <span className="status-badge">{rankingReport?.totalCandidates ?? 0} Evaluated</span>
              {extractionWarningCount > 0 ? (
                <span className="status-badge status-badge--warning">{extractionWarningCount} Extraction Warning{extractionWarningCount > 1 ? 's' : ''}</span>
              ) : null}
            </div>
          </div>
          <div className="results-controls">
            <p className="results-copy">
              The recruiter can choose how many top candidates to review by match percentage, ranking position, and key gaps.
            </p>
            <label className="results-picker" htmlFor="candidate-count">
              <span>Show top</span>
              <select
                id="candidate-count"
                className="form-select form-select--compact"
                value={candidateDisplayCount}
                onChange={(event) => setCandidateDisplayCount(Number(event.target.value))}
              >
                {Array.from({ length: maxDisplayCount }, (_, index) => index + 1).map((count) => (
                  <option key={count} value={count}>
                    {count} candidate{count > 1 ? 's' : ''}
                  </option>
                ))}
              </select>
            </label>
          </div>
        </div>

        {rankingReport?.jobPosting ? (
          <>
            {rankingReport.aiStatusMessage ? (
              <div className={`report-banner ${rankingReport.aiUsed ? 'report-banner--ai' : 'report-banner--fallback'}`}>
                {rankingReport.aiStatusMessage}
              </div>
            ) : null}
            <div className="report-overview">
              <div>
              <span className="report-overview__label">Role</span>
              <strong>{rankingReport.jobPosting.title}</strong>
              </div>
              <div>
              <span className="report-overview__label">Department</span>
              <strong>{rankingReport.jobPosting.department || 'Engineering'}</strong>
              </div>
              <div>
              <span className="report-overview__label">Location</span>
              <strong>{rankingReport.jobPosting.location || 'Remote'}</strong>
              </div>
              <div>
              <span className="report-overview__label">Generated</span>
              <strong>{new Date(rankingReport.generatedAtUtc).toLocaleString()}</strong>
              </div>
            </div>
          </>
        ) : null}

        <div className="candidate-results">
          {currentUser && rankedCandidates.length > 0 ? rankedCandidates.map((candidate, index) => (
            <article key={candidate.name} className="candidate-card">
              <div className="candidate-card__top">
                <span className="candidate-rank">#{index + 1}</span>
                <span className="candidate-score">{candidate.score} Match</span>
              </div>
              <h4>{candidate.name}</h4>
              <p className="candidate-recommendation">{candidate.recommendation ?? 'Preliminary Review'}</p>
              <p className="candidate-summary">{candidate.summary}</p>
              <div className="candidate-progress" aria-hidden="true">
                <div className="candidate-progress__fill" style={{ width: candidate.score }} />
              </div>
              {candidate.scoreBreakdown ? (
                <div className="score-breakdown">
                  {candidate.scoreBreakdown.map((item) => (
                    <div key={`${candidate.name}-${item.label}`} className="score-breakdown__item">
                      <span>{item.label}</span>
                      <strong>{item.value}</strong>
                    </div>
                  ))}
                </div>
              ) : null}
              <p className="candidate-strength">
                <strong>Strengths:</strong> {candidate.strengths ?? candidate.summary}
              </p>
              <p className="candidate-gap">
                <strong>Gaps:</strong> {candidate.gaps}
              </p>
              {candidate.extractionWarning ? (
                <p className="candidate-warning">
                  <strong>Extraction Warning:</strong> {candidate.extractionWarning}
                </p>
              ) : null}
              <p className="candidate-note">
                <strong>Review Note:</strong> {candidate.notes ?? `Gap count: ${candidate.gapCount ?? 0}`}
              </p>
            </article>
          )) : (
            <article className="candidate-card candidate-card--empty">
              <h4>No ranking generated yet</h4>
              <p className="candidate-summary">
                {currentUser
                  ? 'Upload a job description and at least one CV, then run screening to see ranked candidates here.'
                  : 'Log in first, then upload a job description and CVs to generate ranked candidates.'}
              </p>
            </article>
          )}
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

      {showSignUpModal ? (
        <div className="auth-modal-backdrop" role="presentation" onClick={handleCloseSignUpModal}>
          <section
            className="auth-modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="signup-title"
            onClick={(event) => event.stopPropagation()}
          >
            <div className="auth-modal__header">
              <div>
                <p className="card-kicker">Recruiter Access</p>
                <h3 id="signup-title">Create Your Recruiter Account</h3>
              </div>
              <button type="button" className="auth-modal__close" onClick={handleCloseSignUpModal}>
                Close
              </button>
            </div>
            <form className="auth-form" onSubmit={handleSignUpSubmit}>
              <label className="field-label" htmlFor="signup-full-name">Full Name</label>
              <input
                id="signup-full-name"
                className="form-input"
                value={signUpForm.fullName}
                onChange={(event) => setSignUpForm((current) => ({ ...current, fullName: event.target.value }))}
              />

              <label className="field-label" htmlFor="signup-company">Company</label>
              <input
                id="signup-company"
                className="form-input"
                value={signUpForm.companyName}
                onChange={(event) => setSignUpForm((current) => ({ ...current, companyName: event.target.value }))}
              />

              <label className="field-label" htmlFor="signup-email">Email</label>
              <input
                id="signup-email"
                className="form-input"
                type="email"
                value={signUpForm.email}
                onChange={(event) => setSignUpForm((current) => ({ ...current, email: event.target.value }))}
              />

              <label className="field-label" htmlFor="signup-password">Password</label>
              <input
                id="signup-password"
                className="form-input"
                type="password"
                value={signUpForm.password}
                onChange={(event) => setSignUpForm((current) => ({ ...current, password: event.target.value }))}
              />

              <label className="field-label" htmlFor="signup-confirm-password">Confirm Password</label>
              <input
                id="signup-confirm-password"
                className="form-input"
                type="password"
                value={signUpForm.confirmPassword}
                onChange={(event) => setSignUpForm((current) => ({ ...current, confirmPassword: event.target.value }))}
              />

              {signUpMessage ? <p className="auth-form__success">{signUpMessage}</p> : null}
              {signUpError ? <p className="auth-form__error">{signUpError}</p> : null}

              <button className="submit-button" type="submit" disabled={isSigningUp}>
                {isSigningUp ? 'Creating Account...' : 'Create Recruiter Account'}
              </button>
              <button className="secondary-submit-button auth-form__secondary" type="button" onClick={handleOpenGoogleModal}>
                Continue with Gmail
              </button>
            </form>
          </section>
        </div>
      ) : null}

      {showLoginModal ? (
        <div className="auth-modal-backdrop" role="presentation" onClick={handleCloseLoginModal}>
          <section
            className="auth-modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="login-title"
            onClick={(event) => event.stopPropagation()}
          >
            <div className="auth-modal__header">
              <div>
                <p className="card-kicker">Recruiter Access</p>
                <h3 id="login-title">Log In To Your Recruiter Account</h3>
              </div>
              <button type="button" className="auth-modal__close" onClick={handleCloseLoginModal}>
                Close
              </button>
            </div>
            <form className="auth-form" onSubmit={handleLoginSubmit}>
              <label className="field-label" htmlFor="login-email">Email</label>
              <input
                id="login-email"
                className="form-input"
                type="email"
                value={loginForm.email}
                onChange={(event) => setLoginForm((current) => ({ ...current, email: event.target.value }))}
              />

              <label className="field-label" htmlFor="login-password">Password</label>
              <input
                id="login-password"
                className="form-input"
                type="password"
                value={loginForm.password}
                onChange={(event) => setLoginForm((current) => ({ ...current, password: event.target.value }))}
              />

              {loginMessage ? <p className="auth-form__success">{loginMessage}</p> : null}
              {loginError ? <p className="auth-form__error">{loginError}</p> : null}

              <button className="submit-button" type="submit" disabled={isLoggingIn}>
                {isLoggingIn ? 'Logging In...' : 'Log In'}
              </button>
              <button className="secondary-submit-button auth-form__secondary" type="button" onClick={handleOpenGoogleModal}>
                Continue with Gmail
              </button>
            </form>
          </section>
        </div>
      ) : null}

      {showGoogleModal ? (
        <div className="auth-modal-backdrop" role="presentation" onClick={handleCloseGoogleModal}>
          <section
            className="auth-modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="google-title"
            onClick={(event) => event.stopPropagation()}
          >
            <div className="auth-modal__header">
              <div>
                <p className="card-kicker">Quick Access</p>
                <h3 id="google-title">Continue With Gmail</h3>
              </div>
              <button type="button" className="auth-modal__close" onClick={handleCloseGoogleModal}>
                Close
              </button>
            </div>
            <form className="auth-form" onSubmit={handleGoogleSignInSubmit}>
              <p className="auth-form__helper">
                Use your Gmail address to create or access a recruiter account without setting a password.
              </p>

              <label className="field-label" htmlFor="google-full-name">Full Name</label>
              <input
                id="google-full-name"
                className="form-input"
                value={googleForm.fullName}
                onChange={(event) => setGoogleForm((current) => ({ ...current, fullName: event.target.value }))}
              />

              <label className="field-label" htmlFor="google-company">Company</label>
              <input
                id="google-company"
                className="form-input"
                value={googleForm.companyName}
                onChange={(event) => setGoogleForm((current) => ({ ...current, companyName: event.target.value }))}
              />

              <label className="field-label" htmlFor="google-email">Gmail Address</label>
              <input
                id="google-email"
                className="form-input"
                type="email"
                placeholder="name@gmail.com"
                value={googleForm.email}
                onChange={(event) => setGoogleForm((current) => ({ ...current, email: event.target.value }))}
              />

              {googleMessage ? <p className="auth-form__success">{googleMessage}</p> : null}
              {googleError ? <p className="auth-form__error">{googleError}</p> : null}

              <button className="submit-button" type="submit" disabled={isSigningInWithGoogle}>
                {isSigningInWithGoogle ? 'Signing In...' : 'Continue With Gmail'}
              </button>
            </form>
          </section>
        </div>
      ) : null}
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

async function parseJsonResponse(response) {
  const responseText = await response.text()
  if (!responseText) {
    return {}
  }

  try {
    return JSON.parse(responseText)
  } catch {
    return { message: responseText }
  }
}

export default App
