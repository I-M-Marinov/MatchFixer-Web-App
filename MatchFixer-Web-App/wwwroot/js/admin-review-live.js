// Live Review Center updates for admins (SignalR)
(() => {
	if (window.__adminReviewLive || typeof signalR === 'undefined') return;
	window.__adminReviewLive = true;

	const root = document.querySelector('[data-review-live]');
	if (!root) return;

	let pendingEmails = parseInt(root.dataset.pendingEmails || '0', 10) || 0;
	let pendingChanges = parseInt(root.dataset.pendingChanges || '0', 10) || 0;

	// Badges
	function renderBadges() {
		const total = pendingEmails + pendingChanges;

		document.querySelectorAll('[data-review-total]').forEach(el => {
			el.textContent = total;
			el.style.display = total > 0 ? '' : 'none';
			el.title = `${total} item(s) awaiting review`;
		});

		const tabBadge = document.getElementById('approvalsTabBadge');
		if (tabBadge) tabBadge.textContent = pendingChanges;
	}

	function pulse(el) {
		if (!el) return;
		el.classList.remove('review-pulse');
		void el.offsetWidth; // restart animation
		el.classList.add('review-pulse');
	}

	// Toast
	function toast(title, text) {
		let host = document.getElementById('reviewToastHost');
		if (!host) {
			host = document.createElement('div');
			host.id = 'reviewToastHost';
			host.className = 'review-toast-host';
			document.body.appendChild(host);
		}

		const t = document.createElement('a');
		t.className = 'review-toast';
		t.href = '/Admin/EmailQueue/Index?tab=approvals';

		const icon = document.createElement('i');
		icon.className = 'bi bi-person-check-fill review-toast-icon';

		const body = document.createElement('div');
		const strong = document.createElement('div');
		strong.className = 'review-toast-title';
		strong.textContent = title;
		const small = document.createElement('div');
		small.className = 'review-toast-text';
		small.textContent = text;
		body.append(strong, small);

		t.append(icon, body);
		host.appendChild(t);

		requestAnimationFrame(() => t.classList.add('show'));
		setTimeout(() => {
			t.classList.remove('show');
			setTimeout(() => t.remove(), 300);
		}, 6000);
	}

	// Review Center cards
	function antiForgeryToken() {
		return document.querySelector('input[name="__RequestVerificationToken"]')?.value
			|| document.querySelector('meta[name="csrf-token"]')?.content
			|| '';
	}

	function el(tag, cls, text) {
		const e = document.createElement(tag);
		if (cls) e.className = cls;
		if (text != null) e.textContent = text;
		return e;
	}

	function buildCard(r) {
		const card = el('div', 'qa-card cr-card cr-card-new');
		card.dataset.requestId = r.id;

		const main = el('div', 'qa-card-main');
		const who = el('div', 'qa-card-recipient');
		who.append(el('i', 'bi bi-person-fill me-1'), document.createTextNode(r.userName), el('span', 'cr-email', r.userEmail));
		const subj = el('div', 'qa-card-subject');
		subj.append(el('span', 'badge bg-info text-dark me-1', r.typeLabel), document.createTextNode(' ' + r.summary));
		main.append(who, subj);

		const foot = el('div', 'qa-card-foot');
		const date = el('span', 'qa-card-date');
		date.append(el('i', 'bi bi-clock me-1'), document.createTextNode(r.requestedAt));
		foot.appendChild(date);

		const form = el('form', 'cr-form');
		form.method = 'post';
		form.action = '/Admin/ChangeRequest/Approve';

		const token = el('input'); token.type = 'hidden'; token.name = '__RequestVerificationToken'; token.value = antiForgeryToken();
		const id = el('input'); id.type = 'hidden'; id.name = 'id'; id.value = r.id;
		const note = el('input', 'form-control form-control-sm cr-note');
		note.type = 'text'; note.name = 'note'; note.maxLength = 1000; note.placeholder = 'Optional note (kept in the log)';

		const actions = el('div', 'cr-actions');
		const approve = el('button', 'btn btn-mint btn-sm');
		approve.type = 'submit'; approve.formAction = '/Admin/ChangeRequest/Approve';
		approve.append(el('i', 'bi bi-check2-circle'), document.createTextNode(' Approve'));
		const reject = el('button', 'btn btn-outline-danger btn-sm');
		reject.type = 'submit'; reject.formAction = '/Admin/ChangeRequest/Reject';
		reject.append(el('i', 'bi bi-x-octagon'), document.createTextNode(' Reject'));
		reject.addEventListener('click', e => { if (!confirm('Reject this request?')) e.preventDefault(); });
		actions.append(approve, reject);

		form.append(token, id, note, actions);
		card.append(main, foot, form);
		return card;
	}

	function syncEmptyState() {
		const list = document.getElementById('approvalsList');
		const empty = document.getElementById('approvalsEmpty');
		if (!list || !empty) return;
		const hasCards = list.querySelector('.cr-card') != null;
		list.style.display = hasCards ? '' : 'none';
		empty.style.display = hasCards ? 'none' : '';
	}

	// Hub
	const connection = new signalR.HubConnectionBuilder()
		.withUrl('/hubs/admin-review')
		.withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
		.build();

	connection.on('ChangeRequestCreated', r => {
		pendingChanges = r.pendingCount;
		renderBadges();
		document.querySelectorAll('[data-review-total]').forEach(pulse);

		const list = document.getElementById('approvalsList');
		if (list && !list.querySelector(`[data-request-id="${r.id}"]`)) {
			list.appendChild(buildCard(r));
			syncEmptyState();
		}

		toast(`New ${r.typeLabel.toLowerCase()} request`, `${r.userName}: ${r.summary}`);
	});

	connection.on('ChangeRequestResolved', r => {
		pendingChanges = r.pendingCount;
		renderBadges();

		const card = document.querySelector(`#approvalsList [data-request-id="${r.id}"]`);
		if (card) {
			card.classList.add('cr-card-leaving');
			setTimeout(() => { card.remove(); syncEmptyState(); }, 250);
		}
	});

	async function start() {
		try {
			await connection.start();
		} catch (err) {
			console.warn('[AdminReview] SignalR connection failed, retrying in 10s', err);
			setTimeout(start, 10000);
		}
	}

	renderBadges();
	syncEmptyState();
	start();
})();
