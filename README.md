<div align="center">
  <img src="./assets/matchFixer-logo.png" width="220px" alt="MatchFixer Logo" />
  <h1>MatchFixer</h1>
  <p>A football sports betting web application built with ASP.NET Core MVC</p>
</div>

---

<div align="center">

## Overview

</div>

<p align="center">
MatchFixer is a football sports betting platform where users can browse upcoming match events, build and submit betslips, track their winnings and unlock trophies. The application supports domestic leagues, UEFA club competitions (Champions League, Europa League, Conference League), international matches and the FIFA World Cup 2026, with live score updates pulled directly from external APIs.
</p>

<p align="center">
Users can set their favourite teams and receive email notifications whenever one of them is about to play. A dedicated World Cup 2026 tab provides a full tournament tracker — group stage standings and a live knockout bracket that updates as results come in. Admins have a full back-office: creating and managing match events manually or importing them from the API, setting odds, applying odds boosts, managing teams, viewing user bet history and sending email blasts to the entire user base.
</p>

<p align="center">
The application also features two mini-games — <strong>The MatchFixer Game</strong> (predict match outcomes) and the <strong>Logo Quiz</strong> (identify football club badges) — adding a gamified layer on top of the core betting experience.
</p>

---

<div align="center">

## Technology Stack

</div>

<div align="center">
<h3>Front End</h3>
</div>

<ul>
  <li>HTML5 &amp; CSS3</li>
  <li>SCSS (compiled via LibSassBuilder)</li>
  <li>Bootstrap 5</li>
  <li>Bootstrap Icons</li>
  <li>JavaScript &amp; jQuery 3.7</li>
  <li>AJAX</li>
  <li>Animate.css</li>
  <li>Select2 (enhanced dropdowns)</li>
  <li>SortableJS (drag-and-drop bracket reordering)</li>
</ul>

<div align="center">
<h3>Back End</h3>
</div>

<ul>
  <li>C#</li>
  <li>.NET 8.0</li>
  <li>ASP.NET Core with MVC pattern</li>
  <li>Microsoft SQL Server</li>
  <li>Entity Framework Core 8</li>
  <li>ASP.NET Core Identity</li>
  <li>NodaTime</li>
  <li>ISO3166</li>
  <li>API-Football (api-sports.io) — upcoming matches, live odds, fixture data</li>
  <li>TheSportsDB API — World Cup fixtures, group standings and live scores</li>
  <li>Cloudinary — cloud-hosted team logo management</li>
</ul>

<div align="center">
<h3>Source Control</h3>
</div>

<ul>
  <li>Git / GitHub</li>
</ul>

---

<div align="center">

## User Guide

</div>

<details>
  <summary><h2>Home Page</h2></summary>
  <div align="center">
    <p>
      - The Home Page displays all currently active match events grouped by league, with styled event cards per competition type.<br/>
      - Users can browse events, click into a match to view details or add selections to their betslip directly from the board.<br/>
      - Any active Odds Boosts are highlighted in the boost banner at the top of the page.<br/>
      - Events that have already kicked off are visually distinguished with a different style to indicate they are live or in progress.
    </p>
    <img src="./assets/new-home-page-view.png" width="800px" />
    <br/><br/>
    <img src="./assets/matchfixer-started-events.png" width="800px" />
  </div>
</details>

<details>
  <summary><h2>Match Event Cards</h2></summary>
  <div align="center">
    <p>
      - Each competition has its own distinct card style and colour palette.<br/>
      - Derby matches and boosted events receive additional visual treatment to stand out on the board.
    </p>
    <h4>UEFA Champions League</h4>
    <img src="./assets/updated-ucl-event-cards.png" width="700px" />
    <h4>UEFA Europa League</h4>
    <img src="./assets/updated-uel-event-cards.png" width="700px" />
    <h4>UEFA Conference League</h4>
    <img src="./assets/updated-uecl-event-cards.png" width="700px" />
    <h4>FIFA World Cup</h4>
    <img src="./assets/world-cup-event-cards.png" width="700px" />
    <h4>International Friendlies</h4>
    <img src="./assets/international-event-cards.png" width="700px" />
    <h4>Derby Match</h4>
    <img src="./assets/updated-derby-event-card.png" width="700px" />
    <h4>Boosted Event</h4>
    <img src="./assets/boosted-event-card.png" width="700px" />
  </div>
</details>

<details>
  <summary><h2>Betslip</h2></summary>
  <div align="center">
    <p>
      - Users build a betslip by selecting outcomes (Home / Draw / Away) from events on the board.<br/>
      - The betslip panel shows the current selections, the combined odds and the potential payout.<br/>
      - Upon submission the betslip is saved and the user can track its result once the events are settled.
    </p>
    <img src="./assets/submit-betslip.gif" width="500px" />
    <br/><br/>
    <img src="./assets/winning-betslip.png" width="500px" />
  </div>
</details>

<details>
  <summary><h2>Profile Page</h2></summary>
  <div align="center">
    <p>
      - After logging in, users land on their Profile Page.<br/>
      - The page shows the user's personal stats, bet history summary and trophy cabinet.<br/>
      - Users can edit their profile info, upload a profile picture and change their password from dedicated tabs.
    </p>
    <h4>Profile Overview</h4>
    <img src="./assets/profile-page-final.png" width="800px" />
    <h4>Favourite Teams</h4>
    <p>
      - Users can set their favourite teams from the profile page.<br/>
      - Favourite teams are highlighted on the Events Board whenever they are playing.<br/>
      - An automated email notification is sent to the user before each match involving one of their favourite teams.
    </p>
    <img src="./assets/favorite-teams-profile-page.png" width="800px" />
    <br/><br/>
    <img src="./assets/favorite-teams.gif" width="800px" />
    <br/><br/>
    <img src="./assets/favorite-team-playing-email.png" width="300px" />
    <img src="./assets/favorite-team-playing-email2.png" width="300px" />
    <br/><br/>
    <img src="./assets/favorite-team-playing-email3.png" width="700px" />
    <h4>Trophies</h4>
    <p>- Users unlock trophies based on their betting activity and milestones reached on the platform.</p>
    <img src="./assets/rigged-to-win-trophy.png" width="800px" />
  </div>
</details>

<details>
  <summary><h2>Odds Boosts</h2></summary>
  <div align="center">
    <p>
      - Admins can create time-limited odds boosts on any active match event.<br/>
      - Boosted events are promoted on the Home Page with a dedicated banner.<br/>
      - Users receive a real-time notification in the app when a boost goes live.
    </p>
    <img src="./assets/odds-boosts-home-screen-new.png" width="700px" />
    <br/><br/>
    <img src="./assets/boost-notifications.png" width="700px" />
  </div>
</details>

<details>
  <summary><h2>League Tables</h2></summary>
  <div align="center">
    <p>- Live standings for the top 7 domestic leagues, pulled from the API and displayed in a clean table view.</p>
    <img src="./assets/league-table-view.png" width="700px" />
  </div>
</details>

<details>
  <summary><h2>Latest Results</h2></summary>
  <div align="center">
    <p>- Displays the most recently concluded matches with final scores across all supported competitions.</p>
    <img src="./assets/latest-results.png" width="700px" />
  </div>
</details>

<details>
  <summary><h2>World Cup 2026 Tracker</h2></summary>
  <div align="center">
    <p>
      - A dedicated tab for the FIFA World Cup 2026 with two sub-sections: Group Stage and Knockout Bracket.<br/>
      - Group standings are fetched from TheSportsDB API and cached in the database.<br/>
      - The knockout bracket visualises all rounds from Round of 32 through to the Final, updating as results come in.<br/>
      - The Champion slot is automatically populated with the winner of the Final once the result is confirmed.
    </p>
    <img src="./assets/world-cup-view.gif" width="700px" />
    <br/><br/>
    <img src="./assets/world-cup-full-time-matches.png" width="700px" />
    <br/><br/>
    <img src="./assets/World-cup-matches-example.png" width="700px" />
    <br/><br/>
    <img src="./assets/world-cup-results-decided-on-penalties .png" width="700px" />
  </div>
</details>

<details>
  <summary><h2>Mini-Games</h2></summary>
  <div align="center">
    <h4>The MatchFixer Game</h4>
    <p>- Predict the outcome of upcoming matches and compete for the top spot.</p>
    <img src="./assets/matfixer-game.jpg" width="600px" />
    <h4>Logo Quiz</h4>
    <p>- Identify football club badges from across the world. Your score is tracked on your profile.</p>
    <img src="./assets/LogoQuiz.jpg" width="600px" />
  </div>
</details>

---

<div align="center">
  <h3>If you like this project 💯 please give it a star ⭐</h3>
</div>
