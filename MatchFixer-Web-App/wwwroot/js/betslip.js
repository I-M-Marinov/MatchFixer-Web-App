document.addEventListener('DOMContentLoaded', function () {
    if (document.getElementById('betSlipContent')) {
        initializeBetSlip();
    }
});

document.addEventListener('DOMContentLoaded', function () {
     
        const toggleBtn = document.getElementById('betSlipToggle');
        const closeBtn = document.getElementById('closeBetSlip');
        const panel = document.getElementById('betSlipPanel');

        toggleBtn?.addEventListener('click', function () {
            panel.classList.toggle('open');
            this.blur();
        });

        closeBtn?.addEventListener('click', function () {
            panel.classList.remove('open');
        });

        document.querySelectorAll(".bet-trigger").forEach(el => {
            el.addEventListener("click", () => {
                const matchId = el.dataset.matchId;
                const home = el.dataset.home;
                const away = el.dataset.away;
                const homeLogoUrl = el.dataset.homeLogoUrl;
                const awayLogoUrl = el.dataset.awayLogoUrl;
                const option = el.dataset.option;
                const startTimeUtc = el.dataset.startTime;
                console.log(startTimeUtc);


                const odds = el.dataset.odds ? parseFloat(el.dataset.odds) : null;

                if (matchId && home && away && homeLogoUrl && awayLogoUrl && option && startTimeUtc &&
                    !isNaN(Date.parse(startTimeUtc)) && odds !== null && !isNaN(odds)) {
                    addToBetSlip(matchId, home, away, homeLogoUrl, awayLogoUrl, option, odds, startTimeUtc);
                    updateBadge();
                } 
            });
        });
    });

    let betSlip = {
        bets: [],
        amount: 0
    };

async function addToBetSlip(matchId, homeTeam, awayTeam, homeLogoUrl, awayLogoUrl, option, odds, startTimeUtc) {
    try {
        // 🎯 Fetch effective odds from server
        const response = await fetch(`/BetSlip/GetEffectiveOdds?matchId=${matchId}&option=${option}`);
        if (!response.ok) {
            console.error("Failed to fetch odds:", response.statusText);
            return;
        }

        const data = await response.json(); // { odds: 2.35, boostId: "..." }
        const odds = data.odds;

        // 🔍 See if bet already exists
        const existingBet = betSlip.bets.find(b => b.matchId === matchId);

        if (existingBet) {
            const oldOption = existingBet.selectedOption;

            existingBet.selectedOption = option;
            existingBet.odds = odds;
            existingBet.homeLogoUrl = homeLogoUrl;
            existingBet.awayLogoUrl = awayLogoUrl;
            existingBet.startTimeUtc = startTimeUtc;

            // sync server session
            removeBetFromSession(matchId, oldOption);
            addBetToSession(existingBet);
        } else {
            const betItem = {
                matchId,
                homeTeam,
                awayTeam,
                homeLogoUrl,
                awayLogoUrl,
                selectedOption: option, // ← must match renderBetSlip()
                odds,
                startTimeUtc
            };
            console.log(betItem);


            betSlip.bets.push(betItem);
            addBetToSession(betItem);
        }

        renderBetSlip();
        updateBetStatuses();
        openBetSlip();
        animateLightning("lightning-flash");
        animateBetSlipBadge();

    } catch (err) {
        console.error("Error adding to bet slip:", err);
    }
}

function animateBetSlipBadge() {
    const btn = document.getElementById('betSlipToggle');
    if (!btn) return;

    btn.classList.remove("animate__animated", "animate__rubberBand");

    void btn.offsetWidth;

    btn.classList.add("animate__animated", "animate__rubberBand");

    btn.addEventListener("animationend", () => {
        btn.classList.remove("animate__animated", "animate__rubberBand");
    }, { once: true });
}
function animateBetSlipBadgeOnRemoval() {
    const btn = document.getElementById('betSlipToggle');
    if (!btn) return;

    btn.classList.remove("animate__animated", "animate__bounceIn");

    void btn.offsetWidth;

    btn.classList.add("animate__animated", "animate__bounceIn");

    btn.addEventListener("animationend", () => {
        btn.classList.remove("animate__animated", "animate__bounceIn");
    }, { once: true });
}
function addBetToSession(betItem) {
    if (!betItem.selectedOption) {
        console.error("SelectedOption is missing!", betItem);
        return; // stop sending invalid payload
    }

    const payload = {
        MatchId: betItem.matchId,
        HomeTeam: betItem.homeTeam,
        AwayTeam: betItem.awayTeam,
        HomeLogoUrl: betItem.homeLogoUrl,
        AwayLogoUrl: betItem.awayLogoUrl,
        SelectedOption: betItem.selectedOption,  // PascalCase
        Odds: betItem.odds,
        StartTimeUtc: betItem.startTimeUtc
    };

    console.log("Sending payload to server:", payload);

    fetch('/BetSlip/Add', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': getAntiForgeryToken()
        },
        body: JSON.stringify(payload)
    }).then(res => {
        console.log("Server response status:", res.status);
        return res.json().catch(() => null);
    }).then(data => console.log("Server response data:", data))
        .catch(err => console.error("Fetch error adding bet:", err));
}

function renderBetSlip() {
    const container = document.getElementById("betSlipContent");
    const amountContainer = document.getElementById("amountContainer");

    const existingClearBtn = amountContainer.querySelector(".clear-all-bets");
    if (existingClearBtn) existingClearBtn.remove();

    container.innerHTML = "";

    const clearBtn = document.createElement("p");
    clearBtn.className = "clear-all-bets";
    amountContainer.appendChild(clearBtn); 

    if (betSlip.bets.length === 0) {

        const p1 = document.createElement("p");
        p1.classList.add("mt-5");
        p1.textContent = "Your bet slip is empty.";

        const pInline = document.createElement("p");

        const textBefore = document.createTextNode("Go to the  ");

        const link = document.createElement("a");
        link.href = "/Event/LiveEvents";
        link.className = "btn btn-link events-board-btn";
        link.textContent = "Events Board";

        const textAfter = document.createTextNode("   to bet on an event.");

        pInline.appendChild(textBefore);
        pInline.appendChild(link);
        pInline.appendChild(textAfter);

        container.appendChild(p1);
        container.appendChild(pInline);


        document.getElementById("totalOdds").innerText = "0.00";
        document.getElementById("potentialReturn").innerText = "0.00";

        clearBtn.textContent = "Add a bet to submit";
        clearBtn.style.cursor = "default";
        clearBtn.style.color = "white"; 
        clearBtn.classList.add('add-bet-to-submit');
        clearBtn.onclick = null; 

        return;
    }

    if (betSlip.bets.length > 0) {
        clearBtn.textContent = "Remove all bets";
        clearBtn.style.cursor = "pointer";
        clearBtn.style.color = "#dc3545"; 
        clearBtn.onclick = async () => {
            await clearAllBetsFromSession();
            betSlip.bets = [];
            updateBadge();
            animateBetSlipBadgeOnRemoval();
            animateLightning("lightning-fade");
            renderBetSlip();
        };

    } else {
        clearBtn.textContent = "Add a bet to submit";
        clearBtn.style.cursor = "default";
        clearBtn.style.color = "white"; 
        clearBtn.classList.add('add-bet-to-submit');
        clearBtn.onclick = null; 
    }

    let totalOdds = 1;

    betSlip.bets.forEach((bet, index) => {
        totalOdds *= bet.odds;

        const item = document.createElement("div");
        item.classList.add("border", "rounded", "p-2", "mb-2");

        const wrapper = document.createElement("div");
        wrapper.classList.add("d-flex", "justify-content-around", "align-items-center");
        wrapper.style.marginTop = "0.2em";

        const left = document.createElement("div");
        left.classList.add("flex-grow", "align-items-center");

        // Team Logos
        const teamsDiv = document.createElement("div");
        teamsDiv.classList.add("flex", "items-center", "mb-2", "text-white");

        const homeLogo = document.createElement("img");
        homeLogo.src = bet.homeLogoUrl;
        homeLogo.alt = `${bet.homeTeam} Logo`;
        homeLogo.title = `${bet.homeTeam}`;
        homeLogo.classList.add("rounded-full", "mr-4", "object-cover");
        homeLogo.style.width = "auto";
        homeLogo.style.height = "60px";
        homeLogo.style.marginLeft = "0.2em";

        const versus = document.createElement("img");
        versus.src = "/images/live-events/versus.png";
        versus.alt = `VS`;
        versus.style.width = "auto";
        versus.style.height = "25px";
        versus.style.margin = "1em";
        versus.classList.add("img-fluid", "mb-2");

        const awayLogo = document.createElement("img");
        awayLogo.src = bet.awayLogoUrl;
        awayLogo.alt = `${bet.awayTeam} Logo`;
        awayLogo.title = `${bet.awayTeam}`;
        awayLogo.classList.add("rounded-full", "ml-2", "object-cover");
        awayLogo.style.width = "auto";
        awayLogo.style.height = "60px";

        teamsDiv.appendChild(homeLogo);
        teamsDiv.appendChild(versus);
        teamsDiv.appendChild(awayLogo);

        const br = document.createElement("br");
        const pick = document.createElement("span");
        pick.textContent = bet.selectedOption;

        if (bet.selectedOption === "Home"){
            pick.classList.add("pill-brand-blue");
        } else if (bet.selectedOption === "Draw"){
            pick.classList.add("pill-brand-yellow");
        } else if (bet.selectedOption === "Away") {
            pick.classList.add("pill-brand-green");
        }

        const pickText = document.createElement('p');
        pickText.textContent = "You bet on:";
        pickText.classList.add("text-center");
        pickText.style.marginBottom = "0em";
        pickText.style.fontSize = "0.85em";

        left.appendChild(teamsDiv);
        left.appendChild(br);

        const pickContainer = document.createElement("div");
        pickContainer.classList.add("flex", "items-center");
        pickContainer.appendChild(pickText);
        pickContainer.appendChild(pick);
        pickContainer.style.marginTop = "-1.2em";

        const right = document.createElement("div");
        right.classList.add("text-end");
        right.id = 'odds-multiplier';

        const oddsLabel = document.createElement("strong");
        oddsLabel.textContent = `${bet.odds.toFixed(2)}x`;

        const oddsLightning = document.createElement('i');
        oddsLightning.classList.add('bi', 'bi-lightning-charge-fill', 'lightning-icon');
        oddsLightning.style.color = '#ffd300';
        oddsLightning.style.textShadow = '0px 2px 2px black';

        const removeBtn = document.createElement("button");
        removeBtn.type = "button";
        removeBtn.classList.add("btn", "btn-sm", "mt-1", "no-feedback");
        removeBtn.id = 'remove-bet-button';

        // Tooltip wrapper
        const removeBtnSpan = document.createElement('span');
        removeBtnSpan.classList.add('custom-event-tooltip-wrapper');

        // Trash icon
        const removeIconElement = document.createElement('i');
        removeIconElement.classList.add('bi', 'bi-x-circle-fill');

        // Tooltip text
        const customTooltipSpan = document.createElement('span');
        customTooltipSpan.classList.add('custom-event-tooltip');
        customTooltipSpan.textContent = "Remove this pick";

        item.classList.add("bet-item", "border", "rounded", "p-2", "mb-2");
        item.dataset.startTime = bet.startTimeUtc;
        item.dataset.matchId = bet.matchId;
        item.dataset.option = bet.selectedOption;

        removeBtnSpan.appendChild(removeIconElement);
        removeBtnSpan.appendChild(customTooltipSpan);
        removeBtn.appendChild(removeBtnSpan);

        removeBtn.addEventListener("click", () => removeBet(bet.matchId, bet.selectedOption));

        right.appendChild(oddsLightning);
        right.appendChild(oddsLabel);
        right.appendChild(document.createElement("br"));
        right.appendChild(removeBtn);

        const statusBadge = document.createElement("span");
        statusBadge.classList.add("badge", "status-badge", "bg-secondary");
        statusBadge.textContent = "";

        const oddsChangeBadge = document.createElement("span");
        oddsChangeBadge.classList.add("badge", "odds-change-badge", "bg-secondary");
        oddsChangeBadge.textContent = "";

        const matchId = bet.matchId;
        const option = bet.selectedOption;

        const state = localStorage.getItem(`oddsDir_${matchId}_${option}`);
        if (state) {
            renderArrow(oddsChangeBadge, state === 'up');
        }

        right.appendChild(statusBadge);
        right.appendChild(oddsChangeBadge);

        wrapper.appendChild(left);
        wrapper.appendChild(right);
        item.appendChild(wrapper);
        item.appendChild(pickContainer);

        const hiddenInputs = document.createElement("div");

        const input1 = document.createElement("input");
        input1.type = "hidden";
        input1.name = `Bets[${index}].MatchId`;
        input1.value = bet.matchId;

        const input2 = document.createElement("input");
        input2.type = "hidden";
        input2.name = `Bets[${index}].SelectedOption`;
        input2.value = bet.selectedOption;

        const input3 = document.createElement("input");
        input3.type = "hidden";
        input3.name = `Bets[${index}].Odds`;
        input3.value = bet.odds;

        hiddenInputs.appendChild(input1);
        hiddenInputs.appendChild(input2);
        hiddenInputs.appendChild(input3);

        item.appendChild(hiddenInputs);

        container.appendChild(item);
    });

    document.getElementById("totalOdds").innerText = totalOdds.toFixed(2);
    updatePotentialReturn();
    updateBetStatuses();
    updateBadge();
}
function removeBet(matchId, selectedOption) {
    // Remove locally
    betSlip.bets = betSlip.bets.filter(b => !(b.matchId === matchId && b.option === selectedOption));

    updateBadge();
    animateBetSlipBadgeOnRemoval();

    // Remove from session server-side
    removeBetFromSession(matchId, selectedOption);

    animateLightning("lightning-fade");
    renderBetSlip();
}

async function clearAllBetsFromSession() {

    const token = getAntiForgeryToken();

    const response = await fetch('/BetSlip/RemoveAll', {
        method: 'POST',
        credentials: 'include',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        }
    });
}
function removeBetFromSession(matchId, selectedOption) {

    const token = getAntiForgeryToken();

    fetch('/BetSlip/Remove', {
        method: 'POST',
        credentials: 'include',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({ MatchId: matchId, SelectedOption: selectedOption })
    });
}
function updatePotentialReturn() {
    const amount = parseFloat(document.getElementById("betAmount").value || "0");
    const totalOdds = parseFloat(document.getElementById("totalOdds").innerText || "0");
    const potential = (amount * totalOdds).toFixed(2);
    document.getElementById("potentialReturn").innerText = potential;
}
function updateBadge() {
    const betSlipBadge = document.getElementById('betCountBadge');
    const betSlipBadgeContainer = document.getElementById('betSlipToggle');

    if (!betSlipBadge) return;

    const hasBets = betSlip.bets.length > 0;
    betSlipBadge.textContent = betSlip.bets.length.toString();

    if (hasBets) {
        betSlipBadge.style.display = "inline-block";
        betSlipBadgeContainer.classList.add("animate__animated", "animate__rubberBand");

    } else {
        betSlipBadge.style.display = "none";
        betSlipBadgeContainer.classList.remove("animate__animated", "animate__rubberBand");
    }
}
function openBetSlip() {
    const panel = document.getElementById("betSlipPanel");
    panel?.classList.add("open");
    panel?.classList.remove("d-none");
}
function closeBetSlip() {
    const panel = document.getElementById("betSlipPanel");
    panel?.classList.remove("open");
    panel?.classList.add("d-none");
}

document.getElementById("closeBetSlip")?.addEventListener("click", closeBetSlip);
function changeBetAmount(button, delta) {
    const input = button.parentElement.querySelector('input');
    const value = parseInt(input.value || '0', 10);
    const newValue = Math.max(0, value + delta);
    input.value = newValue;
    updatePotentialReturn();
}
function animateLightning(effectClass) {
    const lightningIcon = document.querySelector(".lightning-icon-all-odds");
    if (lightningIcon) {
        lightningIcon.classList.add(effectClass);
        lightningIcon.addEventListener("animationend", function handler() {
            lightningIcon.classList.remove(effectClass);
            lightningIcon.removeEventListener("animationend", handler);
        });
    }
}
function initializeBetSlip() {
    fetch('/BetSlip/Get')
        .then(response => response.json())
        .then(data => {
            if (data && data.bets) {
                betSlip = data;
                updateBadge();
                renderBetSlip();
            }
        })
        .catch(err => console.error("Failed to load bet slip:", err));
}
function getAntiForgeryToken() {
    const input = document.querySelector('input[name="__RequestVerificationToken"]');
    return input ? input.value : null;
}
function updateBetStatuses() {
    const now = new Date();


    document.querySelectorAll('.bet-item').forEach(item => {
        let startTimeStr = item.dataset.startTime;

        if (!startTimeStr) return;

        //if (!startTimeStr.endsWith('Z')) {
        //    startTimeStr += 'Z';
        //}

        const startTime = new Date(startTimeStr);

        const statusBadge = item.querySelector('.status-badge');
        const removeBtn = item.querySelector('#remove-bet-button');


        if (!statusBadge || !removeBtn) return;

        if (now >= startTime) {

            statusBadge.textContent = 'Started';
            statusBadge.className = 'badge status-badge bg-danger animate__animated animate__headShake animate__slower animate__infinite';

            statusBadge.style.position = 'absolute';
            statusBadge.style.right = '';
            statusBadge.style.bottom = '0.4em';
            statusBadge.style.left = '0.4em';

            statusBadge.style.width = "auto";
            statusBadge.style.maxWidth = "fit-content";
            statusBadge.style.whiteSpace = "nowrap";

            removeBtn.classList.add('animate__animated', 'animate__pulse', 'animate__infinite');
            removeBtn.style.color = "rgba(220, 53, 69)";

            const tooltip = removeBtn.querySelector('.custom-event-tooltip');
            if (tooltip) {
                tooltip.style.visibility = "visible";
                tooltip.style.opacity = "1";
                tooltip.style.backgroundColor = "rgba(220, 53, 69)";
                tooltip.classList.add("animate__animated", "animate__fadeIn");
            }



        } else {
            const diffMs = startTime - now;
            const diffMinutes = Math.ceil(diffMs / 60000);

            if (diffMinutes < 60) {
                statusBadge.className = 'badge status-badge bg-danger event-countdown';
                statusBadge.textContent = `Starts in ${diffMinutes}m`;
            } else if (diffMinutes >= 1440) {
                const diffDays = Math.ceil(diffMinutes / 1440);
                statusBadge.className = 'badge status-badge bg-warning event-countdown';
                statusBadge.textContent = `Starts in ${diffDays}d`;
            } else {
                const diffHours = Math.ceil(diffMinutes / 60);
                statusBadge.className = 'badge status-badge bg-warning event-countdown';
                statusBadge.textContent = `Starts in ${diffHours}h`;
            }
        }

    });
}
// Run every 3 seconds
setInterval(updateBetStatuses, 3000);