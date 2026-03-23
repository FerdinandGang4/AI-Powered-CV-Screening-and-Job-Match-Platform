import TopNavbar from './TopNavbar'
import './layout.css'

function AppLayout({
  brand,
  heading,
  subheading,
  navigationItems,
  stats,
  onLoginClick,
  onSignUpClick,
  children,
}) {
  return (
    <div className="app-shell">
      <TopNavbar
        brand={brand}
        heading={heading}
        subheading={subheading}
        navigationItems={navigationItems}
        stats={stats}
        onLoginClick={onLoginClick}
        onSignUpClick={onSignUpClick}
      />
      <main className="app-content">{children}</main>
    </div>
  )
}

export default AppLayout
