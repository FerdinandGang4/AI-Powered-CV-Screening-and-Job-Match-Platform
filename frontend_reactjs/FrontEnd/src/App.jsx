import { useEffect, useMemo, useRef, useState } from 'react'
import AppLayout from './components/layout/AppLayout'
import './App.css'

const navigationItems = [
  { label: 'Upload', description: 'Job description and CV files', isActive: true },
  { label: 'Analysis', description: 'Parsing and evaluation flow' },
  { label: 'Ranking', description: 'Results and insights' },
]

const API_BASE_URL = 'http://localhost:5282/api'

const jobTitleOptions = [
  'Senior ASP.NET Developer',
  'ASP.NET Backend Developer',
  'Full Stack .NET Developer',
  'Software Engineer',
  'Backend API Engineer',
  'AI/ML Engineer',
]

const departmentOptions = [
  'Engineering',
  'Software Development',
  'Information Technology',
  'Data Science',
  'Human Resources',
  'Product Development',
]

const locationOptions = [
  'Remote',
  'Onsite',
  'Hybrid',
  'Chicago, IL',
  'Austin, TX',
  'New York, NY',
]

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
  {
    name: 'Samuel Okoro',
    score: '52%',
    summary: 'Has a useful technical foundation and some system support experience.',
    gaps: 'Needs stronger ASP.NET Core, backend API, and SQL project alignment.',
  },
]

function App() {
  const jobDescriptionFileInputRef = useRef(null)
  const candidateFilesInputRef = useRef(null)
  const [jobPostings, setJobPostings] = useState([])
  const [jobPostingMode, setJobPostingMode] = useState('existing')
  const [jobDescriptionInputMode, setJobDescriptionInputMode] = useState('upload')
  const [selectedJobPostingId, setSelectedJobPostingId] = useState('')
  const [jobDescriptionFile, setJobDescriptionFile] = useState(null)
  const [jobDescriptionText, setJobDescriptionText] = useState('')
  const [customJobPosting, setCustomJobPosting] = useState({
    title: jobTitleOptions[0],
    department: departmentOptions[0],
    location: locationOptions[0],
    minimumYearsExperience: 0,
  })
  const [candidateFiles, setCandidateFiles] = useState([])
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [uploadMessage, setUploadMessage] = useState('Select a job posting, attach the role description, and upload candidate CVs to generate a ranking report.')
  const [uploadError, setUploadError] = useState('')
  const [jobPostingMessage, setJobPostingMessage] = useState('')
  const [lastBatch, setLastBatch] = useState(null)
  const [rankingReport, setRankingReport] = useState(null)
  const [candidateDisplayCount, setCandidateDisplayCount] = useState(6)
  const [currentUser, setCurrentUser] = useState(null)
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

  const selectedJobPosting = useMemo(
    () => jobPostings.find((jobPosting) => jobPosting.id === selectedJobPostingId) ?? null,
    [jobPostings, selectedJobPostingId],
  )

  useEffect(() => {
    let ignore = false

    async function loadJobPostings() {
      try {
        const data = await fetchJobPostings()
        if (!ignore) {
          syncJobPostings(data)
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

  function syncJobPostings(data) {
    setJobPostings(data)
    setSelectedJobPostingId((current) => {
      if (data.length === 0) {
        return ''
      }

      if (!current) {
        return ''
      }

      const stillExists = data.some((jobPosting) => jobPosting.id === current)
      if (stillExists) {
        return current
      }

      setJobPostingMessage('The previously selected job posting is no longer available. Leave the selector blank to use your uploaded or pasted job description, or choose another saved posting.')
      return ''
    })
  }

  async function fetchJobPostings() {
    const response = await fetch(`${API_BASE_URL}/jobpostings`)
    if (!response.ok) {
      throw new Error('Unable to load job postings from the backend.')
    }

    return response.json()
  }

  const quickStats = useMemo(() => [
    { value: selectedJobPostingId || jobDescriptionFile || jobDescriptionText.trim() ? '1' : '0', label: 'Job description' },
    { value: `${candidateFiles.length}`, label: 'CV files uploaded' },
    { value: `Top ${candidateDisplayCount}`, label: 'Shortlisted results' },
  ], [candidateDisplayCount, candidateFiles.length, jobDescriptionFile, jobDescriptionText, selectedJobPostingId])

  const rankedCandidates = useMemo(() => {
    if (!rankingReport?.rankedCandidates?.length) {
      return []
    }

    return rankingReport.rankedCandidates.slice(0, candidateDisplayCount).map((candidate) => {
      const percentage = `${Math.round(candidate.overallScore)}%`
      const explanation = candidate.explanation?.summary
        ?? `${candidate.candidate.fullName} was evaluated against the selected role.`
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

  async function createCustomJobPosting() {
    setUploadError('')
    setJobPostingMessage('')

    if (!customJobPosting.title.trim()) {
      setUploadError('Please provide a custom job posting title.')
      return null
    }

    if (!jobDescriptionText.trim() && !jobDescriptionFile) {
      setUploadError('Please add a pasted job description or upload a job description file before saving the custom job posting.')
      return null
    }

    const createResponse = await fetch(`${API_BASE_URL}/jobpostings`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        title: customJobPosting.title.trim(),
        department: customJobPosting.department.trim(),
        descriptionText: jobDescriptionText.trim() || 'Uploaded job description document',
        minimumYearsExperience: Number(customJobPosting.minimumYearsExperience) || 0,
        location: customJobPosting.location.trim(),
        requirements: [],
      }),
    })

    if (!createResponse.ok) {
      throw new Error('Unable to create the custom job posting.')
    }

    const createdJobPosting = await createResponse.json()

    setJobPostings((current) => {
      const withoutDuplicate = current.filter((item) => item.id !== createdJobPosting.id)
      return [createdJobPosting, ...withoutDuplicate]
    })
    setSelectedJobPostingId(createdJobPosting.id)
    setJobPostingMode('existing')
    setJobPostingMessage(`Custom job posting "${createdJobPosting.title}" was saved and is now available in Existing Job Posting.`)

    return createdJobPosting
  }

  async function handleSaveCustomJobPosting() {
    try {
      await createCustomJobPosting()
    } catch (error) {
      setUploadError(error.message)
    }
  }

  async function handleSubmit(event) {
    event.preventDefault()

    setUploadError('')
    setJobPostingMessage('')

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

    let jobPostingId = selectedJobPostingId

    if (jobPostingMode === 'custom') {
      if (jobDescriptionInputMode === 'upload' && !hasUploadedJobDescription) {
        setUploadError('Please upload the custom job description file.')
        return
      }

      if (jobDescriptionInputMode === 'paste' && !hasPastedJobDescription) {
        setUploadError('Please paste the custom job description.')
        return
      }
    } else if (!selectedJobPostingId && !hasUploadedJobDescription && !hasPastedJobDescription) {
      setUploadError('Please select an existing job posting or provide a job description to screen candidates.')
      return
    }

    setIsSubmitting(true)
    setUploadMessage('Uploading files and generating ranking report...')

    try {
      if (jobPostingMode === 'custom') {
        const createdJobPosting = await createCustomJobPosting()
        if (!createdJobPosting) {
          throw new Error('Unable to create the custom job posting.')
        }
        jobPostingId = createdJobPosting.id
      } else {
        const latestJobPostings = await fetchJobPostings()
        syncJobPostings(latestJobPostings)

        if (jobPostingId) {
          const validSelectedJobPosting = latestJobPostings.find((jobPosting) => jobPosting.id === jobPostingId)
          if (!validSelectedJobPosting) {
            jobPostingId = ''
            setSelectedJobPostingId('')
            setJobPostingMessage('Your previous job posting selection is no longer available. The upload will now use the job description you provided unless you select another saved posting.')
          }
        }
      }

      const formData = new FormData()
      if (jobPostingId) {
        formData.append('jobPostingId', jobPostingId)
      }

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

  function handleResetWorkflow() {
    if (jobDescriptionFileInputRef.current) {
      jobDescriptionFileInputRef.current.value = ''
    }

    if (candidateFilesInputRef.current) {
      candidateFilesInputRef.current.value = ''
    }

    setJobPostingMode('existing')
    setJobDescriptionInputMode('upload')
    setSelectedJobPostingId('')
    setJobDescriptionFile(null)
    setJobDescriptionText('')
    setCustomJobPosting({
      title: jobTitleOptions[0],
      department: departmentOptions[0],
      location: locationOptions[0],
      minimumYearsExperience: 0,
    })
    setCandidateFiles([])
    setIsSubmitting(false)
    setUploadError('')
    setJobPostingMessage('')
    setLastBatch(null)
    setRankingReport(null)
    setCandidateDisplayCount(6)
    setUploadMessage('Select a job posting, attach the role description, and upload candidate CVs to generate a ranking report.')
  }

  function handleOpenSignUpModal() {
    setShowLoginModal(false)
    setSignUpError('')
    setSignUpMessage('')
    setShowSignUpModal(true)
  }

  function handleOpenLoginModal() {
    setShowSignUpModal(false)
    setLoginError('')
    setLoginMessage('')
    setShowLoginModal(true)
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

  function handleLoginClick() {
    handleOpenLoginModal()
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

      const payload = await response.json()
      if (!response.ok) {
        throw new Error(payload?.message || 'Sign up could not be completed.')
      }

      setSignUpMessage(payload.message || 'Account created successfully.')
      setCurrentUser({
        fullName: payload.fullName,
        companyName: payload.companyName,
        email: payload.email,
      })
      setUploadMessage(`Welcome, ${payload.fullName}. Your recruiter account has been created.`)
      setTimeout(() => {
        handleCloseSignUpModal()
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

      const payload = await response.json()
      if (!response.ok) {
        throw new Error(payload?.message || 'Login could not be completed.')
      }

      setCurrentUser({
        fullName: payload.fullName,
        companyName: payload.companyName,
        email: payload.email,
      })
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

      <form className="upload-form" onSubmit={handleSubmit}>
        <section className="simple-grid">
          <article className="workspace-card upload-card">
            <p className="card-kicker">Step 1</p>
            <h3>Upload Job Description</h3>
            <p>
              Choose an existing job posting or create a custom one, then upload or paste the job description for screening.
            </p>
            <div className="mode-toggle">
              <button
                type="button"
                className={`mode-toggle__button${jobPostingMode === 'existing' ? ' is-active' : ''}`}
                onClick={() => setJobPostingMode('existing')}
              >
                Existing Job Posting
              </button>
              <button
                type="button"
                className={`mode-toggle__button${jobPostingMode === 'custom' ? ' is-active' : ''}`}
                onClick={() => setJobPostingMode('custom')}
              >
                Custom Job Posting
              </button>
            </div>
            {jobPostingMode === 'existing' ? (
              <>
                <label className="field-label" htmlFor="job-posting">Job Posting</label>
                <select
                  id="job-posting"
                  className="form-select"
                  value={selectedJobPostingId}
                  onChange={(event) => setSelectedJobPostingId(event.target.value)}
                >
                  <option value="">Use uploaded job description instead</option>
                  {jobPostings.map((jobPosting) => (
                    <option key={jobPosting.id} value={jobPosting.id}>
                      {jobPosting.title}
                    </option>
                  ))}
                </select>
                <p className="helper-text">
                  Selecting a saved job posting is optional. If you leave this blank, the system will analyze the uploaded or pasted job description as the source of truth.
                </p>
                {jobPostingMessage ? <p className="helper-text helper-text--success">{jobPostingMessage}</p> : null}
                {selectedJobPosting ? (
                  <div className="job-preview">
                    <div className="job-preview__top">
                      <strong>{selectedJobPosting.title}</strong>
                      <span>{selectedJobPosting.minimumYearsExperience}+ years</span>
                    </div>
                    <p>{selectedJobPosting.descriptionText}</p>
                    <div className="job-preview__meta">
                      <span>{selectedJobPosting.department || 'Engineering'}</span>
                      <span>{selectedJobPosting.location || 'Remote'}</span>
                    </div>
                  </div>
                ) : null}
              </>
            ) : (
              <div className="custom-job-grid">
                <div>
                  <label className="field-label" htmlFor="custom-title">Job Title</label>
                  <select
                    id="custom-title"
                    className="form-select"
                    value={customJobPosting.title}
                    onChange={(event) =>
                      setCustomJobPosting((current) => ({ ...current, title: event.target.value }))
                    }
                  >
                    <option value="">Select job title</option>
                    {jobTitleOptions.map((title) => (
                      <option key={title} value={title}>
                        {title}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="field-label" htmlFor="custom-department">Department</label>
                  <select
                    id="custom-department"
                    className="form-select"
                    value={customJobPosting.department}
                    onChange={(event) =>
                      setCustomJobPosting((current) => ({ ...current, department: event.target.value }))
                    }
                  >
                    <option value="">Select department</option>
                    {departmentOptions.map((department) => (
                      <option key={department} value={department}>
                        {department}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="field-label" htmlFor="custom-location">Location</label>
                  <select
                    id="custom-location"
                    className="form-select"
                    value={customJobPosting.location}
                    onChange={(event) =>
                      setCustomJobPosting((current) => ({ ...current, location: event.target.value }))
                    }
                  >
                    <option value="">Select location</option>
                    {locationOptions.map((location) => (
                      <option key={location} value={location}>
                        {location}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="field-label" htmlFor="custom-years">Minimum Years Experience</label>
                  <input
                    id="custom-years"
                    className="form-input"
                    type="number"
                    min="0"
                    value={customJobPosting.minimumYearsExperience}
                    onChange={(event) =>
                      setCustomJobPosting((current) => ({
                        ...current,
                        minimumYearsExperience: event.target.value,
                      }))
                    }
                  />
                </div>
              </div>
            )}
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
            {jobPostingMode === 'custom' ? (
              <button
                type="button"
                className="secondary-submit-button"
                onClick={handleSaveCustomJobPosting}
              >
                Save Custom Job Posting
              </button>
            ) : null}
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
            <button
              className="secondary-submit-button"
              type="button"
              onClick={handleResetWorkflow}
              disabled={isSubmitting}
            >
              Reset App
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
          {rankedCandidates.length > 0 ? rankedCandidates.map((candidate, index) => (
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
                Upload a job description and at least one CV, then run screening to see ranked candidates here.
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

export default App
