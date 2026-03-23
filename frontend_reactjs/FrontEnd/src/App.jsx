import AppLayout from './components/layout/AppLayout'
import './App.css'

const navigationItems = [
  { label: 'Upload', description: 'Job description and CV files', isActive: true },
  { label: 'Analysis', description: 'Parsing and evaluation flow' },
  { label: 'Ranking', description: 'Results and insights' },
]

const quickStats = [
  { value: '1', label: 'Job description' },
  { value: '4', label: 'CV files uploaded' },
  { value: '3', label: 'Ranked candidates' },
]

const rankedCandidates = [
  {
    name: 'Amina Hassan',
    score: '91%',
    summary: 'Strong fit for ASP.NET Core, C#, SQL, and backend API development.',
    gaps: 'Missing only minor Azure deployment depth.',
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
]

function App() {
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

      <section className="simple-grid">
        <article className="workspace-card upload-card">
          <p className="card-kicker">Step 1</p>
          <h3>Upload Job Description</h3>
          <p>
            Recruiters provide the role requirements so the platform can evaluate each CV against the same criteria.
          </p>
          <div className="mock-dropzone">
            <span>Drop job description here or browse file</span>
          </div>
        </article>

        <article className="workspace-card upload-card">
          <p className="card-kicker">Step 2</p>
          <h3>Upload Multiple CVs</h3>
          <p>
            Add several candidate CVs at once so the backend can parse, compare, and rank them together.
          </p>
          <div className="mock-dropzone">
            <span>Upload CV batch: `amina.pdf`, `david.pdf`, `grace.pdf`, `mark.pdf`</span>
          </div>
        </article>

        <article className="workspace-card process-card">
          <p className="card-kicker">Step 3</p>
          <h3>Processing Pipeline</h3>
          <p>
            The backend parses each CV, extracts skills, experience, and projects, then matches each candidate to the role requirements.
          </p>
          <ul className="process-list">
            <li>Parse CV content</li>
            <li>Extract skills, projects, and experience</li>
            <li>Score candidate relevance</li>
            <li>Generate explanations and gaps</li>
          </ul>
        </article>
      </section>

      <section className="results-section">
        <div className="results-header">
          <div>
            <p className="card-kicker">Step 4</p>
            <h3>Ranked Candidate Results</h3>
          </div>
          <p className="results-copy">
            The recruiter sees a ranked list with match score, reason for ranking, and missing skills or experience.
          </p>
        </div>

        <div className="candidate-results">
          {rankedCandidates.map((candidate, index) => (
            <article key={candidate.name} className="candidate-card">
              <div className="candidate-card__top">
                <span className="candidate-rank">#{index + 1}</span>
                <span className="candidate-score">{candidate.score}</span>
              </div>
              <h4>{candidate.name}</h4>
              <p className="candidate-summary">{candidate.summary}</p>
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
    </AppLayout>
  )
}

export default App
