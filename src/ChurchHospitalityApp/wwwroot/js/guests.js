Api.requireAuth();
renderNav('guests');

const tbody = document.getElementById('guestsBody');
const alertBox = document.getElementById('alertBox');
const emptyState = document.getElementById('emptyState');
const searchInput = document.getElementById('searchInput');
const backdrop = document.getElementById('detailModalBackdrop');
const modal = document.getElementById('detailModal');
const esc = Api.esc;

let searchTimeout;
searchInput.addEventListener('input', () => {
  clearTimeout(searchTimeout);
  searchTimeout = setTimeout(loadGuests, 300);
});

function fmtDate(iso) {
  if (!iso) return '—';
  return new Date(iso).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
}

async function loadGuests() {
  alertBox.innerHTML = '';
  try {
    const query = searchInput.value.trim();
    const guests = await Api.get(`/api/guests${query ? '?search=' + encodeURIComponent(query) : ''}`);

    tbody.innerHTML = '';
    emptyState.classList.toggle('hidden', guests.length > 0);

    guests.forEach(g => {
      const tr = document.createElement('tr');
      tr.style.cursor = 'pointer';
      tr.innerHTML = `
        <td>${esc(g.fullName)}</td>
        <td>${esc(g.phoneNumber)}</td>
        <td>${fmtDate(g.visitDate)}</td>
        <td><span class="badge pending">${g.pendingFollowUps}</span></td>
        <td><span class="badge completed">${g.completedFollowUps}</span></td>
      `;
      tr.addEventListener('click', () => openDetail(g.id));
      tbody.appendChild(tr);
    });
  } catch (err) {
    alertBox.innerHTML = `<div class="alert error">${esc(err.message)}</div>`;
  }
}

async function openDetail(id) {
  try {
    const g = await Api.get(`/api/guests/${id}`);

    const rows = g.followUps.map(f => `
      <tr>
        <td>Week ${f.weekNumber}</td>
        <td>${fmtDate(f.scheduledFor)}</td>
        <td><span class="badge ${f.status.toLowerCase()}">${f.status}</span></td>
        <td>${f.assignedToUserName ? esc(f.assignedToUserName) : '<span class="text-muted">Unassigned</span>'}</td>
        <td>${fmtDate(f.callDate)}</td>
        <td>${f.outcome ? esc(f.outcome) : '—'}</td>
        <td>${f.notes ? esc(f.notes) : '—'}</td>
      </tr>
    `).join('');

    modal.innerHTML = `
      <h2>${esc(g.fullName)}</h2>
      <p class="text-muted">
        📞 ${esc(g.phoneNumber)} ${g.email ? ' · ✉️ ' + esc(g.email) : ''}<br>
        Visited: ${fmtDate(g.visitDate)} ${g.address ? '· ' + esc(g.address) : ''}
      </p>
      ${g.howHeardAboutUs ? `<p><strong>How they heard about us:</strong> ${esc(g.howHeardAboutUs)}</p>` : ''}
      ${g.notes ? `<p><strong>Notes:</strong> ${esc(g.notes)}</p>` : ''}
      <h3 style="margin-top:20px;">Follow-Up History</h3>
      <div class="table-wrap">
        <table>
          <thead><tr><th>Week</th><th>Scheduled</th><th>Status</th><th>Assigned To</th><th>Call Date</th><th>Outcome</th><th>Call Notes</th></tr></thead>
          <tbody>${rows}</tbody>
        </table>
      </div>
      <div class="modal-actions">
        <button class="btn secondary" id="closeModalBtn">Close</button>
      </div>
    `;

    backdrop.classList.remove('hidden');
    document.getElementById('closeModalBtn').addEventListener('click', () => backdrop.classList.add('hidden'));
  } catch (err) {
    alertBox.innerHTML = `<div class="alert error">${esc(err.message)}</div>`;
  }
}

backdrop.addEventListener('click', (e) => {
  if (e.target === backdrop) backdrop.classList.add('hidden');
});

loadGuests();
