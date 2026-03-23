function TopNavbar({ heading, subheading, stats }) {
  return (
    <header className="top-navbar">
      <div className="top-navbar__intro">
        <p className="top-navbar__label">Recruitment Intelligence</p>
        <h1>{heading}</h1>
        <p className="top-navbar__subheading">{subheading}</p>
      </div>

      <div className="top-navbar__stats" aria-label="Application highlights">
        {stats.map((stat) => (
          <article key={stat.label} className="top-navbar__stat">
            <strong>{stat.value}</strong>
            <span>{stat.label}</span>
          </article>
        ))}
      </div>
    </header>
  )
}

export default TopNavbar
