import AppLayout from './components/layout/AppLayout'
import './App.css'

const navigationItems = [
  { label: 'Dashboard', description: 'Overview of hiring activity', isActive: true },
  { label: 'Job Posts', description: 'Manage role requirements' },
  { label: 'CV Screening', description: 'Upload and parse candidate files' },
  { label: 'Rankings', description: 'Compare candidate evaluation results' },
  { label: 'Reports', description: 'Review summaries and insights' },
]

const quickStats = [
  { value: '24', label: 'CVs screened this week' },
  { value: '08', label: 'Active job postings' },
  { value: '91%', label: 'Processing success rate' },
]

function App() {
  return (
    <AppLayout
      brand="CV Match Platform"
      heading="AI-Powered Recruitment Workspace"
      subheading="Screen candidates faster, compare rankings clearly, and keep recruiters aligned from upload to shortlist."
      navigationItems={navigationItems}
      stats={quickStats}
    >
      <section className="hero-panel">
        <div className="hero-copy">
          <p className="eyebrow">Recruiter Dashboard</p>
          <h2>Backend and frontend are now separated cleanly for a scalable hiring workflow.</h2>
          <p>
            This layout is ready for the next components: job posting forms, CV upload flows,
            candidate ranking tables, and detailed match explanations from the ASP.NET API.
          </p>
          <div className="hero-actions">
            <button type="button" className="primary-action">Create Job Posting</button>
            <button type="button" className="secondary-action">Open Screening Queue</button>
          </div>
        </div>

        <div className="hero-metrics" aria-label="Platform summary">
          <article className="metric-card emphasis">
            <span className="metric-label">Top insight</span>
            <strong className="metric-value">2 strong candidates</strong>
            <p className="metric-note">Latest ranking run found two applicants above the shortlist threshold.</p>
          </article>
          <article className="metric-card">
            <span className="metric-label">Next frontend step</span>
            <strong className="metric-value">Upload flow</strong>
            <p className="metric-note">Wire the CV upload form to `/api/screening/batches`.</p>
          </article>
        </div>
      </section>

      <section className="workspace-grid">
        <article className="workspace-card">
          <p className="card-kicker">Ready Section</p>
          <h3>Top Navigation</h3>
          <p>
            Includes page identity, status information, and recruiter-facing actions for the dashboard shell.
          </p>
        </article>

        <article className="workspace-card">
          <p className="card-kicker">Ready Section</p>
          <h3>Sidebar Navigation</h3>
          <p>
            Gives the app a stable dashboard structure so the rest of the pages can slot in without redesigning the frame.
          </p>
        </article>

        <article className="workspace-card">
          <p className="card-kicker">Next Section</p>
          <h3>Job And Upload Modules</h3>
          <p>
            The next implementation step is adding the upload and job requirement components inside this main content area.
          </p>
        </article>
      </section>
    </AppLayout>
  )
}

export default App
