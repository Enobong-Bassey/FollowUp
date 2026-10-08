// Renders the top navigation bar into #navbar-root, adapting links to the user's role.
// Call renderNav('dashboard') on each page, passing the current page's key.

function renderNav(activeKey) {
  const user = Api.getUser();
  if (!user) return;

  const isAdmin = user.role === 'Admin';

  const links = [
    { key: 'dashboard', href: '/dashboard.html', label: 'Dashboard', show: true },
    { key: 'guests', href: '/guests.html', label: 'Guests', show: true },
    { key: 'followups', href: '/followups.html', label: 'My Follow-Ups', show: true },
    { key: 'assign', href: '/assign-followups.html', label: 'Assign Follow-Ups', show: isAdmin },
    { key: 'members', href: '/members.html', label: 'Members', show: isAdmin },
    { key: 'qrcode', href: '/qrcode.html', label: 'QR Code', show: isAdmin },
    { key: 'profile', href: '/profile.html', label: 'Profile', show: true }
  ];

  const linksHtml = links
    .filter(l => l.show)
    .map(l => `<a href="${l.href}" class="${l.key === activeKey ? 'active' : ''}">${l.label}</a>`)
    .join('');

  const photo = user.profilePhotoUrl || '/img/default-avatar.svg';

  document.getElementById('navbar-root').innerHTML = `
    <nav class="navbar">
      <div class="navbar-inner">
        <a class="navbar-brand" href="/dashboard.html">⛪ Hospitality Unit</a>
        <button class="nav-toggle" id="navToggle" aria-label="Toggle menu">&#9776;</button>
        <div class="nav-links" id="navLinks">
          ${linksHtml}
          <div class="nav-user">
            <img src="${Api.esc(photo)}" alt="">
            <span>${Api.esc(user.fullName)}</span>
            <button class="btn-logout" id="logoutBtn">Log out</button>
          </div>
        </div>
      </div>
    </nav>
  `;

  document.getElementById('navToggle').addEventListener('click', () => {
    document.getElementById('navLinks').classList.toggle('open');
  });

  document.getElementById('logoutBtn').addEventListener('click', () => {
    Api.clearSession();
    window.location.href = '/index.html';
  });
}
