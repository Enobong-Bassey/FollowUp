Api.requireAuth();
renderNav('profile');

const alertBox = document.getElementById('alertBox');
const form = document.getElementById('profileForm');
const avatarPreview = document.getElementById('avatarPreview');
const photoInput = document.getElementById('photoInput');
const esc = Api.esc;

async function loadProfile() {
  try {
    const p = await Api.get('/api/profile');
    document.getElementById('email').value = p.email;
    document.getElementById('fullName').value = p.fullName;
    document.getElementById('phone').value = p.phoneNumber;
    avatarPreview.src = p.profilePhotoUrl || '/img/default-avatar.svg';
  } catch (err) {
    alertBox.innerHTML = `<div class="alert error">${esc(err.message)}</div>`;
  }
}

form.addEventListener('submit', async (e) => {
  e.preventDefault();
  alertBox.innerHTML = '';
  const saveBtn = document.getElementById('saveBtn');
  saveBtn.disabled = true;

  try {
    const fullName = document.getElementById('fullName').value.trim();
    await Api.put('/api/profile', {
      fullName,
      phoneNumber: document.getElementById('phone').value.trim()
    });

    // Keep the cached session in sync (nav bar name)
    const user = Api.getUser();
    user.fullName = fullName;
    Api.setSession(Api.getToken(), user);

    alertBox.innerHTML = '<div class="alert success">Profile updated.</div>';
    renderNav('profile');
  } catch (err) {
    alertBox.innerHTML = `<div class="alert error">${esc(err.message)}</div>`;
  } finally {
    saveBtn.disabled = false;
  }
});

photoInput.addEventListener('change', async () => {
  const file = photoInput.files[0];
  if (!file) return;

  const formData = new FormData();
  formData.append('file', file);

  try {
    const result = await Api.postForm('/api/profile/photo', formData);
    avatarPreview.src = result.profilePhotoUrl;

    const user = Api.getUser();
    user.profilePhotoUrl = result.profilePhotoUrl;
    Api.setSession(Api.getToken(), user);
    renderNav('profile');

    alertBox.innerHTML = '<div class="alert success">Photo updated.</div>';
  } catch (err) {
    alertBox.innerHTML = `<div class="alert error">${esc(err.message)}</div>`;
  } finally {
    photoInput.value = '';
  }
});

loadProfile();
