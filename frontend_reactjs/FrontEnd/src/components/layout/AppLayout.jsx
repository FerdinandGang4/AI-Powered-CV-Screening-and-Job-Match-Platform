import Sidebar from './Sidebar'
import TopNavbar from './TopNavbar'
import './layout.css'

function AppLayout({ brand, heading, subheading, navigationItems, stats, children }) {
  return (
    <div className="app-shell">
      <Sidebar brand={brand} items={navigationItems} />
      <div className="app-main">
        <TopNavbar heading={heading} subheading={subheading} stats={stats} />
        <main className="app-content">{children}</main>
      </div>
    </div>
  )
}

export default AppLayout
