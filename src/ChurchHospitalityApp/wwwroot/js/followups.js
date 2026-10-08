Api.requireAuth();
renderNav('followups');

const tbody = document.getElementById('followupsBody');
const alertBox = document.getElementById('alertBox');
const emptyState = document.getElementById('emptyState');
const statusFilter = document.getElementById('statusFilter');
const backdrop = document.getElementById('logModalBackdrop');
const logForm = document.getElementById('logForm');
const logAlertBox = document.getElementById('logAlertBox');
const esc = Api.esc;

statusFilter.addEventListener('change', loadFollowUps);

function fmtDate(iso) {
  if (!iso) return '—';
  return new Date(iso).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
}

// Local date as yyyy-mm-dd for <input type="date">
function todayLocal() {
  const d = new Date();
  const pad = (n) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

async function loadFollowUps() {
  alertBox.innerHTML = '';
  try {
    const status = statusFilter.value;
    const items = await Api.get(`/api/followups?scope=mine${status ? '&status=' + status : ''}`);

    tbody.innerHTML = '';
    emptyState.classList.toggle('hidden', items.length > 0);

    items.forEach(f => {
      const tr = document.createElement('tr');
      const logCell = f.status === 'Pending'
        ? `<button class="btn small" data-id="${f.id}">Log Call</button>`
        : `<span class="text-muted" style="font-size:0.85rem;">${fmtDate(f.callDate)} · ${esc(f.outcome || '')}${f.notes ? '<br>' + esc(f.notes) : ''}</span>`;

      tr.innerHTML = `
        <td>${esc(f.guestFullName)}</td>
        <td><a href="tel:${esc(f.guestPhoneNumber)}">${esc(f.guestPhoneNumber)}</a></td>
        <td>Week ${f.weekNumber}</td>
        <td>${fmtDate(f.scheduledFor)}</td>
        <td><span class="badge ${f.status.toLowerCase()}">${f.status}</span></td>
        <td>${logCell}</td>
      `;
      tbody.appendChild(tr);
    });

    tbody.querySelectorAll('button[data-id]').forEach(btn => {
      btn.addEventListener('click', () => openLogModal(btn.dataset.id));
    });
  } catch (err) {
    alertBox.innerHTML = `<div class="alert error">${esc(err.message)}</div>`;
  }
}

function openLogModal(id) {
  logForm.reset();
  logAlertBox.innerHTML = '';
  document.getElementById('logFollowUpId').value = id;
  document.getElementById('callDate').value = todayLocal();
  backdrop.classList.remove('hidden');
}

document.getElementById('cancelLogBtn').addEventListener('click', () => backdrop.classList.add('hidden'));
backdrop.addEventListener('click', (e) => { if (e.target === backdrop) backdrop.classList.add('hidden'); });

logForm.addEventListener('submit', async (e) => {
  e.preventDefault();
  logAlertBox.innerHTML = '';
  const saveBtn = document.getElementById('saveLogBtn');
  saveBtn.disabled = true;

  try {
    const id = document.getElementById('logFollowUpId').value;
    await Api.put(`/api/followups/${id}/log`, {
      callDate: document.getElementById('callDate').value,
      outcome: document.getElementById('outcome').value,
      status: document.getElementById('callStatus').value,
      notes: document.getElementById('callNotes').value.trim() || null
    });

    backdrop.classList.add('hidden');
    loadFollowUps();
  } catch (err) {
    logAlertBox.innerHTML = `<div class="alert error">${esc(err.message)}</div>`;
  } finally {
    saveBtn.disabled = false;
  }
});

loadFollowUps();
