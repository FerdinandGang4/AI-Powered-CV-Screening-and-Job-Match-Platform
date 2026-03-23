function TopNavbar({
  brand,
  heading,
  subheading,
  navigationItems,
  stats,
  onLoginClick,
  onSignUpClick,
  onLogoutClick,
  currentUser,
}) {
  function scrollToSection(targetId) {
    if (!currentUser) {
      onLoginClick?.()
      return
    }

    if (!targetId) {
      return
    }

    const section = document.getElementById(targetId)
    if (!section) {
      return
    }

    section.scrollIntoView({
      behavior: 'smooth',
      block: 'start',
    })
  }

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
            disabled={!currentUser}
            aria-disabled={!currentUser}
            onClick={() => scrollToSection(item.targetId)}
          >
            {item.label}
          </button>
        ))}
        <div className="top-navbar__contact-links">
          <a href="mailto:ferdinandgang4@gmail.com" aria-label="Send email to Gang Ferdinand Dinga">Email</a>
          <a href="https://www.linkedin.com/in/ferdinand-dinga-gang-91a912185/" target="_blank" rel="noreferrer" aria-label="Open LinkedIn profile">LinkedIn</a>
          <a href="https://github.com/FerdinandGang4/" target="_blank" rel="noreferrer" aria-label="Open GitHub profile">GitHub</a>
        </div>
        <div className="top-navbar__auth">
          {currentUser ? (
            <>
              <div className="top-navbar__user-chip">
                <strong>{currentUser.fullName}</strong>
                <span>{currentUser.companyName}</span>
              </div>
              <button type="button" className="top-navbar__auth-button top-navbar__auth-button--ghost" onClick={onLogoutClick}>
                Logout
              </button>
            </>
          ) : (
            <>
              <button type="button" className="top-navbar__auth-button top-navbar__auth-button--ghost" onClick={onLoginClick}>
                Login
              </button>
              <button type="button" className="top-navbar__auth-button top-navbar__auth-button--solid" onClick={onSignUpClick}>
                Sign Up
              </button>
            </>
          )}
        </div>
      </nav>
    </header>
  )
}

export default TopNavbar
