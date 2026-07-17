function initAuth(onLogin, onLogout) {
  const $ = id => document.getElementById(id);
  const whoami = $('whoami'), btnLogin = $('btnLogin'), btnRegister = $('btnRegister'), btnLogout = $('btnLogout');
  let mode = 'login';

  document.body.insertAdjacentHTML('beforeend', `
    <dialog id="authDialog">
      <h3 id="authTitle">Login</h3>
      <div class="body">
        <label>username <input id="authUser" autocomplete="username"></label>
        <label>password <input id="authPass" type="password" autocomplete="current-password"></label>
        <div class="error" id="authError"></div>
      </div>
      <div class="actions">
        <button id="authCancel">Cancel</button>
        <button id="authSubmit" class="btn-primary">Login</button>
      </div>
    </dialog>`);

  async function me() {
    const r = await fetch('/api/me');
    const signedIn = r.ok;
    if (whoami) whoami.textContent = signedIn ? (await r.json()).username : '';
    if (btnLogin) btnLogin.style.display = signedIn ? 'none' : '';
    if (btnRegister) btnRegister.style.display = signedIn ? 'none' : '';
    if (btnLogout) btnLogout.style.display = signedIn ? '' : 'none';
    (signedIn ? onLogin : onLogout)?.();
  }

  function open(m) {
    mode = m;
    $('authTitle').textContent = m === 'login' ? 'Login' : 'Register';
    $('authSubmit').textContent = m === 'login' ? 'Login' : 'Create account';
    $('authError').textContent = '';
    $('authPass').value = '';
    $('authDialog').showModal();
    $('authUser').focus();
  }

  function post(url, username, password) {
    return fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password }),
    });
  }

  async function submit() {
    const username = $('authUser').value, password = $('authPass').value;
    if (!username || !password) { $('authError').textContent = 'username and password required'; return; }

    $('authSubmit').disabled = true;
    const r = await post(mode === 'login' ? '/api/login' : '/api/register', username, password);
    if (!r.ok) {
      $('authSubmit').disabled = false;
      $('authError').textContent = (await r.text()) || (r.status === 401 ? 'wrong username or password' : 'failed');
      return;
    }

    if (mode === 'register') {
      const login = await post('/api/login', username, password);
      if (!login.ok) { $('authSubmit').disabled = false; open('login'); return; }
    }

    $('authSubmit').disabled = false;
    $('authDialog').close();
    me();
  }

  if (btnLogin) btnLogin.onclick = () => open('login');
  if (btnRegister) btnRegister.onclick = () => open('register');
  if (btnLogout) btnLogout.onclick = async () => { await fetch('/api/logout', { method: 'POST' }); me(); };
  $('authCancel').onclick = () => $('authDialog').close();
  $('authSubmit').onclick = submit;
  $('authDialog').addEventListener('keydown', e => {
    if (e.key === 'Enter') { e.preventDefault(); submit(); }
  });
  closeOnBackdrop($('authDialog'));

  me();

  return { openLogin: () => open('login'), openRegister: () => open('register') };
}

function closeOnBackdrop(dlg) {
  dlg.addEventListener('click', e => { if (e.target === dlg) dlg.close(); });
}

function esc(s) {
  return s.replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}
