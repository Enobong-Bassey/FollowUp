Api.requireAuth();
renderNav('members');

if (Api.getUser().role !== 'Admin') {
  window.location.href = '/dashboard.html';
}

const tbody = document.getElementById('membersBody');
const alertBox = document.getElementById('alertBox');
const backdrop = document.getElementById('addModalBackdrop');
const addForm = document.getElementById('addForm');
const addAlertBox = document.getElementById('addAlertBox');
const esc = Api.esc;

async function loadMembers() {
  try {
    const members = await Api.get('/api/users');
    tbody.innerHTML = '';

    members.forEach(m => {
      const tr = document.createElement('tr');
      const accountLabel = m.emailConfirmed
        ? '<span class="badge completed">Registered</span>'
        : '<span class="badge pending">Invite pending</span>';

      tr.innerHTML = `
        <td>${esc(m.fullName)}</td>
        <td>${esc(m.email)}</td>
        <td>${esc(m.phoneNumber)}</td>
        <td><span class="badge ${m.role.toLowerCase()}">${m.role}</span></td>
        <td>${accountLabel}</td>
        <td>
          <label style="display:flex; align-items:center; gap:6px; font-weight:400; margin:0;">
            <input type="checkbox" data-user-id="${esc(m.id)}" ${m.isActive ? 'checked' : ''}>
            ${m.isActive ? 'Active' : 'Inactive'}
          </label>
        </td>
      `;
      tbody.appendChild(tr);
    });

    tbody.querySelectorAll('input[type=checkbox]').forEach(cb => {
      cb.addEventListener('change', () => toggleActive(cb.dataset.userId, cb.checked));
    });
  } catch (err) {
    alertBox.innerHTML = `<div class="alert error">${esc(err.message)}</div>`;
  }
}

async function toggleActive(userId, isActive) {
  alertBox.innerHTML = '';
  try {
    await Api.put(`/api/users/${encodeURIComponent(userId)}/active`, { isActive });
    loadMembers();
  } catch (err) {
    alertBox.innerHTML = `<div class="alert error">${esc(err.message)}</div>`;
    loadMembers();
  }
}

document.getElementById('addMemberBtn').addEventListener('click', () => {
  addForm.reset();
  addAlertBox.innerHTML = '';
  backdrop.classList.remove('hidden');
});
document.getElementById('cancelAddBtn').addEventListener('click', () => backdrop.classList.add('hidden'));
backdrop.addEventListener('click', (e) => { if (e.target === backdrop) backdrop.classList.add('hidden'); });

addForm.addEventListener('submit', async (e) => {
  e.preventDefault();
  addAlertBox.innerHTML = '';
  const saveBtn = document.getElementById('saveAddBtn');
  saveBtn.disabled = true;

  try {
    const result = await Api.post('/api/users', {
      fullName: document.getElementById('newFullName').value.trim(),
      email: document.getElementById('newEmail').value.trim(),
      phoneNumber: document.getElementById('newPhone').value.trim(),
      role: document.getElementById('newRole').value
    });

    backdrop.classList.add('hidden');
    alertBox.innerHTML = result && result.warning
      ? `<div class="alert error">${esc(result.warning)}</div>`
      : '<div class="alert success">Member added and invitation email sent.</div>';
    loadMembers();
  } catch (err) {
    addAlertBox.innerHTML = `<div class="alert error">${esc(err.message)}</div>`;
  } finally {
    saveBtn.disabled = false;
  }
});

loadMembers();
