Api.requireAuth();
renderNav('assign');

if (Api.getUser().role !== 'Admin') {
  window.location.href = '/dashboard.html';
}

const tbody = document.getElementById('rowsBody');
const alertBox = document.getElementById('alertBox');
const emptyState = document.getElementById('emptyState');
const viewFilter = document.getElementById('viewFilter');
const esc = Api.esc;

let members = [];

function fmtDate(iso) {
  if (!iso) return '—';
  return new Date(iso).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
}

async function loadMembers() {
  const all = await Api.get('/api/users');
  members = all.filter(m => m.isActive && m.emailConfirmed);
}

function memberOptions(selectedId) {
  return members.map(m =>
    `<option value="${esc(m.id)}" ${m.id === selectedId ? 'selected' : ''}>${esc(m.fullName)}</option>`
  ).join('');
}

async function loadFollowUps() {
  alertBox.innerHTML = '';
  try {
    const scope = viewFilter.value === 'unassigned' ? 'unassigned' : 'all';
    const items = await Api.get(`/api/followups?scope=${scope}`);

    tbody.innerHTML = '';
    emptyState.classList.toggle('hidden', items.length > 0);

    items.forEach(f => {
      const callLog = f.callDate
        ? `${fmtDate(f.callDate)} · ${esc(f.outcome || '')}${f.notes ? '<br><span class="text-muted">' + esc(f.notes) + '</span>' : ''}`
        : '<span class="text-muted">—</span>';

      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td>${esc(f.guestFullName)}</td>
        <td>${esc(f.guestPhoneNumber)}</td>
        <td>Week ${f.weekNumber}</td>
        <td>${fmtDate(f.scheduledFor)}</td>
        <td><span class="badge ${f.status.toLowerCase()}">${f.status}</span></td>
        <td>${callLog}</td>
        <td>${f.assignedToUserName ? esc(f.assignedToUserName) : '<span class="text-muted">Unassigned</span>'}</td>
        <td>
          <div style="display:flex; gap:6px;">
            <select data-followup-id="${f.id}" style="width:auto;">
              <option value="">Select member…</option>
              ${memberOptions(f.assignedToUserId)}
            </select>
            <button class="btn small" data-assign-id="${f.id}">Assign</button>
          </div>
        </td>
      `;
      tbody.appendChild(tr);
    });

    tbody.querySelectorAll('button[data-assign-id]').forEach(btn => {
      btn.addEventListener('click', () => assign(btn.dataset.assignId));
    });
  } catch (err) {
    alertBox.innerHTML = `<div class="alert error">${esc(err.message)}</div>`;
  }
}

async function assign(followUpId) {
  const select = tbody.querySelector(`select[data-followup-id="${followUpId}"]`);
  const assignedToUserId = select.value;

  if (!assignedToUserId) {
    alertBox.innerHTML = '<div class="alert error">Please choose a member to assign.</div>';
    return;
  }

  try {
    await Api.put(`/api/followups/${followUpId}/assign`, { assignedToUserId });
    loadFollowUps();
  } catch (err) {
    alertBox.innerHTML = `<div class="alert error">${esc(err.message)}</div>`;
  }
}

viewFilter.addEventListener('change', loadFollowUps);

(async () => {
  try {
    await loadMembers();
  } catch (err) {
    alertBox.innerHTML = `<div class="alert error">${esc(err.message)}</div>`;
  }
  await loadFollowUps();
})();
