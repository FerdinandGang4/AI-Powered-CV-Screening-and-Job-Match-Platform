import AppLayout from './components/layout/AppLayout'
import './App.css'

const navigationItems = [
  { label: 'Upload', description: 'Job description and CV files', isActive: true },
  { label: 'Analysis', description: 'Parsing and evaluation flow' },
  { label: 'Ranking', description: 'Results and insights' },
]

const quickStats = [
  { value: '1', label: 'Job description' },
  { value: '8', label: 'CV files uploaded' },
  { value: 'Top 5', label: 'Shortlisted results' },
]

const rankedCandidates = [
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

export default App
