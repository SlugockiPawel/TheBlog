/*
==========================================
Variables used
==========================================
*/

let lastScrollTop = 0;

/*
==========================================
Html elements
==========================================
*/

const searchIconDesktop = document.querySelector("#searchIconDesktop");
const searchIconMobile = document.querySelector("#searchIconMobile");
const searchBox = document.querySelector("#searchBox");
const navBar = document.querySelector("#navBar");
const togglerIcon = document.querySelector("#togglerIcon");


/*
==========================================
Event Listeners
==========================================
*/

searchIconDesktop.addEventListener('click', toggleSearchBox);
searchIconMobile.addEventListener('click', toggleSearchBox);
window.addEventListener('scroll', slideNavBar);
togglerIcon.addEventListener('click', hideMobileViewSearchBox);

/*
==========================================
Functions
==========================================
*/

function fillFilePath() {
    document.getElementById("uploadFile").value = this.value;
}

function hideMobileViewSearchBox() {
    hideElement(searchBox);
    resetSerachIconToDefault();
}

function slideNavBar() {
    let st = window.scrollY || document.body.scrollTop;
    if (lastScrollTop === 0 && st > lastScrollTop) {
        // first, hide search bar and reset search icon
        if (searchBoxVisible()) {
            hideElement(searchBox);
            resetSerachIconToDefault();
        }

        hideElement(navBar);

        $("#navBar").slideDown(900);
    }

    lastScrollTop = st <= 0 ? 0 : st;
}

function resetSerachIconToDefault() {
    swapClasses(searchIconDesktop, 'fa-search', 'fa-times');
    swapClasses(searchIconMobile, 'fa-search', 'fa-times');
}

function searchBoxVisible() {
    return searchBox.style.display === "block";
}

function hideElement(element) {
    element.style.display = "none";
}

function toggleSearchBox() {
    $("#searchBox").slideToggle(900, toggleSearchIcons(searchIconDesktop, searchIconMobile));
}

function toggleSearchIcons(searchIconDesktop, searchIconMobile) {
    if (searchBox.style.display === "block") {
        swapClasses(searchIconDesktop, 'fa-search', 'fa-times');
        swapClasses(searchIconMobile, 'fa-search', 'fa-times');
    } else {
        swapClasses(searchIconDesktop, 'fa-times', 'fa-search');
        swapClasses(searchIconMobile, 'fa-times', 'fa-search');
    }
}

function swapClasses(element, classToAdd, classToRemove) {
    element.classList.add(classToAdd);
    element.classList.remove(classToRemove);
}

