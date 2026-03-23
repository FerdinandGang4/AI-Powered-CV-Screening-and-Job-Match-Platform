function TopNavbar({ brand, heading, subheading, navigationItems, stats }) {
  return (
    <header className="top-navbar">
      <div className="top-navbar__main">
        <div className="top-navbar__intro">
          <div className="top-navbar__meta">
            <div className="top-navbar__brand">
              <span className="top-navbar__brand-mark" aria-hidden="true">CM</span>
              <div>
                <p className="top-navbar__label">{brand}</p>
                <p className="top-navbar__credit">Powered by Engineer Gang Ferdinand Dinga</p>
              </div>
            </div>
          </div>
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
      </div>

      <nav className="top-navbar__nav" aria-label="Primary">
        {navigationItems.map((item) => (
          <button
            key={item.label}
            type="button"
            className={`top-navbar__nav-item${item.isActive ? ' is-active' : ''}`}
          >
            {item.label}
          </button>
        ))}
        <div className="top-navbar__auth">
          <button type="button" className="top-navbar__auth-button top-navbar__auth-button--ghost">
            Login
          </button>
          <button type="button" className="top-navbar__auth-button top-navbar__auth-button--solid">
            Sign Up
          </button>
        </div>
      </nav>
    </header>
  )
}

export default TopNavbar
