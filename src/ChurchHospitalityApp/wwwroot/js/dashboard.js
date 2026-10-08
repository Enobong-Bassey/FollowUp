Api.requireAuth();
renderNav('dashboard');

const user = Api.getUser();
const isAdmin = user.role === 'Admin';
let currentScope = 'mine';

if (isAdmin) {
  document.getElementById('scopeToggleWrap').innerHTML = `
    <select id="scopeSelect" style="width:auto;">
      <option value="mine">My Follow-Ups</option>
      <option value="all">Whole Team</option>
    </select>`;
  document.getElementById('scopeSelect').addEventListener('change', (e) => {
    currentScope = e.target.value;
    loadSummary();
  });
}

async function loadSummary() {
  const alertBox = document.getElementById('alertBox');
  alertBox.innerHTML = '';
  try {
    const data = await Api.get(`/api/dashboard/summary?scope=${currentScope}`);
    document.getElementById('pendingCount').textContent = data.pendingFollowUps;
    document.getElementById('completedCount').textContent = data.completedFollowUps;
    document.getElementById('missedCount').textContent = data.missedFollowUps;

    const adminStats = document.getElementById('adminStats');
    if (data.totalGuestsThisMonth !== null && data.totalGuestsThisMonth !== undefined) {
      adminStats.classList.remove('hidden');
      document.getElementById('guestsMonthCount').textContent = data.totalGuestsThisMonth;
      document.getElementById('activeMembersCount').textContent = data.totalActiveMembers;
      document.getElementById('unassignedCount').textContent = data.unassignedFollowUps;
    } else {
      adminStats.classList.add('hidden');
    }
  } catch (err) {
    alertBox.innerHTML = `<div class="alert error">${Api.esc(err.message)}</div>`;
  }
}

loadSummary();
