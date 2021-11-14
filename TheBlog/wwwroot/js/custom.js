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


/*
==========================================
Tags section (backend logic)
==========================================
*/

let index = 0;

function AddTag() {
    const tagEntry = document.getElementById("TagEntry");

    //search for error (validate tagEntry)
    const searchResult = search(tagEntry.value);
    if (searchResult !== null) {
        //trigger sweet alert

        swalWithDarkButton.fire({
            html: `<span class='fw-bolder'>${searchResult.toUpperCase()}</span>`
        });

    } else {
        const newOption = new Option(tagEntry.value, tagEntry.value);
        document.getElementById("TagList").options[index++] = newOption;
    }


    tagEntry.value = "";
    return true;
}

function DeleteTag() {
    let tagCount = 1;
    const tagList = document.getElementById("TagList");

    if (!tagList) return false;

    if (tagList.selectedIndex === -1) {
        swalWithDarkButton.fire({
            html: `<span class='fw-bolder'>CHOOSE A TAG BEFORE DELETING</span>`
        });
        return true;
    }


    while (tagCount > 0) {
        if (tagList.selectedIndex >= 0) {
            tagList.options[tagList.selectedIndex] = null;
            --tagCount;
        } else {
            tagCount = 0;
        }

        index--;
    }
}

$("form").on("submit",
    function() {
        $("#TagList option").prop("selected", "selected");
    });

//look for the tagValues variable to see if it has data
if (tagValues !== "") {
    let tagArray = tagValues.split(",");
    for (let loop = 0; loop < tagArray.length; loop++) {
        // Load up or replace all options that we have
        ReplaceTag(tagArray[loop], loop);
        index++;
    }
}

function ReplaceTag(tag, index) {
    const newOption = new Option(tag, tag);
    document.getElementById("TagList").options[index] = newOption;
}

//The search function will detect either an aempty or a duplicate Tag(return error string if detected)
function search(str) {
    if (str === '') {
        return "Empty tags are not permitted";
    }

    var tags = document.getElementById('TagList');
    if (tags) {
        const options = tags.options;
        for (let i = 0; i < options.length; i++) {
            if (options[i].value === str)
                return `Duplicate tag: #${str} is not permitted`;
        }
    }

    return null;
}

const swalWithDarkButton = Swal.mixin({
    customClass: {
        confirmButton: 'btn btn-danger btn-outline-dark border-radius-10'
    },
    icon: 'error',
//    imageUrl: '/assets/img/post-sample-image.jpg',
    timer: 4000,
    buttonsStyling: false
});