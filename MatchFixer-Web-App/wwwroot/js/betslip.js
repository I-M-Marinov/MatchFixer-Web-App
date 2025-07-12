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
                const odds = parseFloat(el.dataset.odds);

                if (matchId && home && away && homeLogoUrl && awayLogoUrl && option && !isNaN(odds)) {
                    addToBetSlip(matchId, home, away, homeLogoUrl, awayLogoUrl, option, odds);

                    updateBadge();
                }
            });
        });
    });

    let betSlip = {
        bets: [],
        amount: 0
    };


function addToBetSlip(matchId, homeTeam, awayTeam, homeLogoUrl, awayLogoUrl, option, odds) {
    // Find if any bet for same matchId exists
    const existingBet = betSlip.bets.find(b => b.matchId === matchId);

    if (existingBet) {
        // Update existing bet with new option & odds
        const oldOption = existingBet.option;  // capture old option before change

        existingBet.option = option;
        existingBet.odds = odds;
        existingBet.homeLogoUrl = homeLogoUrl;
        existingBet.awayLogoUrl = awayLogoUrl;

        // Update server session
        removeBetFromSession(matchId, oldOption);
        addBetToSession(existingBet);
    }
    else {
        // New bet - safe to add
        const betItem = {
            matchId: matchId,
            homeTeam,
            awayTeam,
            homeLogoUrl,
            awayLogoUrl,
            option,
            odds
        };

        betSlip.bets.push(betItem);
        addBetToSession(betItem);
    }

    renderBetSlip();
    openBetSlip();
    animateLightning("lightning-flash");
}


function addBetToSession(betItem) {
    fetch('/BetSlip/Add', {
        method: 'POST',
            headers: {
        'Content-Type': 'application/json',
        'RequestVerificationToken': getAntiForgeryToken()
        },
        body: JSON.stringify({
            MatchId: betItem.matchId,
            HomeTeam: betItem.homeTeam,
            AwayTeam: betItem.awayTeam,
            HomeLogoUrl: betItem.homeLogoUrl,
            AwayLogoUrl: betItem.awayLogoUrl,
            SelectedOption: betItem.option,
            Odds: betItem.odds
        })
    }).then(res => {
        if (!res.ok) {
            console.error("Failed to add bet to server:", res.status);
        }
    }).catch(err => console.error("Fetch error adding bet:", err));
}

function renderBetSlip() {
    const container = document.getElementById("betSlipContent");
    container.innerHTML = "";

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
        return;
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
        homeLogo.style.width = "60px";
        homeLogo.style.height = "auto";
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
        awayLogo.style.width = "60px";
        awayLogo.style.height = "auto";

        teamsDiv.appendChild(homeLogo);
        teamsDiv.appendChild(versus);
        teamsDiv.appendChild(awayLogo);

        const br = document.createElement("br");
        const pick = document.createElement("span");
        pick.textContent = bet.option;

        if(bet.option === "Home"){
            pick.classList.add("pill-brand-blue");
        } else if(bet.option === "Draw"){
            pick.classList.add("pill-brand-yellow");
        } else {
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

        removeBtnSpan.appendChild(removeIconElement);
        removeBtnSpan.appendChild(customTooltipSpan);
        removeBtn.appendChild(removeBtnSpan);

        removeBtn.addEventListener("click", () => removeBet(bet.matchId, bet.option));

        right.appendChild(oddsLightning);
        right.appendChild(oddsLabel);
        right.appendChild(document.createElement("br"));
        right.appendChild(removeBtn);

        const statusBadge = document.createElement("span");
        statusBadge.classList.add("badge", "status-badge", "bg-secondary");
        statusBadge.textContent = "";
        right.appendChild(statusBadge);

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
        input2.value = bet.option;

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
    updateBadge();
}

function removeBet(matchId, selectedOption) {
    // Remove locally
    betSlip.bets = betSlip.bets.filter(b => !(b.matchId === matchId && b.option === selectedOption));

    updateBadge();

    console.log(matchId, selectedOption);
    // Remove from session server-side
    removeBetFromSession(matchId, selectedOption);

    animateLightning("lightning-fade");
    renderBetSlip();
}

function removeBetFromSession(matchId, selectedOption) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    fetch('/BetSlip/Remove', {
        method: 'POST',
        credentials: 'include',  // <--- important for cookie authentication!
        headers: {
        'Content-Type': 'application/json',
        'RequestVerificationToken': getAntiForgeryToken()
        },
        body: JSON.stringify({ MatchId: matchId, SelectedOption: selectedOption })
    })
}

function updatePotentialReturn() {
    const amount = parseFloat(document.getElementById("betAmount").value || "0");
    const totalOdds = parseFloat(document.getElementById("totalOdds").innerText || "0");
    const potential = (amount * totalOdds).toFixed(2);
    document.getElementById("potentialReturn").innerText = potential;
}

function updateBadge() {
    const betSlipBadge = document.getElementById('betCountBadge');

    if (betSlipBadge) {
        betSlipBadge.textContent = betSlip.bets.length.toString();
        betSlipBadge.style.display = betSlip.bets.length > 0 ? "inline-block" : "none";
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
    const meta = document.querySelector('meta[name="csrf-token"]');
    return meta ? meta.getAttribute('content') : null;
}