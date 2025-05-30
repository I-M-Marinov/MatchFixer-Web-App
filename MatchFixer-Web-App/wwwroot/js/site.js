﻿// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


/*-------------------------------------------------------------------------------------
# HANDLE THE FADE OUT EFFECT OF SUCCESS AND ERROR MESSAGES IN ALL VIEWS 
--------------------------------------------------------------------------------------*/

document.addEventListener("DOMContentLoaded", function () {
    // Error message fade-out

    var errorMessage = document.getElementById('error-message');
    if (errorMessage) {
        setTimeout(function () {
            errorMessage.classList.add('fade-out');
        }, 1000);
        setTimeout(function () {
            errorMessage.style.display = 'none';
        }, 4500);
    }

    // Success message fade-out
    var successMessage = document.getElementById('success-message');
    if (successMessage) {
        setTimeout(function () {
            successMessage.classList.add('fade-out');
        }, 1000);
        setTimeout(function () {
            successMessage.style.display = 'none';
        }, 4500);
    }

    /*--------------------------------------------------------------------------------
    |      FADE OUT MODEL ERROR MESSAGE AND RESET THE HOME AND AWAY DROPDOWNS         |
    ---------------------------------------------------------------------------------*/


    var homeTeamSelect = document.getElementById('home-team-select');
    var awayTeamSelect = document.getElementById('away-team-select');
    var homeTeamLogo = document.getElementById('home-team-logo');
    var awayTeamLogo = document.getElementById('away-team-logo');

    var modelErrorMessage = document.getElementById('model-error-add-event');
    if (modelErrorMessage) {
        setTimeout(function () {
            if (homeTeamSelect) homeTeamSelect.selectedIndex = 0;
            if (awayTeamSelect) awayTeamSelect.selectedIndex = 0;
            modelErrorMessage.classList.add('fade-out');
        }, 1000);
        setTimeout(function () {       
            modelErrorMessage.style.display = 'none';
        }, 4500);
    }
});
