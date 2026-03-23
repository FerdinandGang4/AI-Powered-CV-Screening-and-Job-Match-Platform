function Sidebar({ brand, items }) {
  return (
    <aside className="sidebar">
      <div className="sidebar__brand">
        <div className="sidebar__mark" aria-hidden="true">CM</div>
        <div>
          <p className="sidebar__eyebrow">AI Hiring Suite</p>
          <h2>{brand}</h2>
        </div>
      </div>

      <nav className="sidebar__nav" aria-label="Primary">
        {items.map((item) => (
          <button
            key={item.label}
            type="button"
            className={`sidebar__item${item.isActive ? ' is-active' : ''}`}
          >
            <span className="sidebar__item-label">{item.label}</span>
            <span className="sidebar__item-description">{item.description}</span>
          </button>
        ))}
      </nav>

      <div className="sidebar__footer">
        <p className="sidebar__footer-title">Frontend Shell Ready</p>
        <p className="sidebar__footer-copy">
          This layout is prepared for React pages that consume the ASP.NET API endpoints.
        </p>
      </div>
    </aside>
  )
}

export default Sidebar
