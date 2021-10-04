let index = 0;

function AddTag() {
    const tagEntry = document.getElementById("TagEntry");

    const newOption = new Option(tagEntry.value, tagEntry.value);
    document.getElementById("TagList").options[index++] = newOption;

    tagEntry.value = "";
    return true;
}

function DeleteTag() {
    let tagCount = 1;

    while (tagCount > 0) {
        const tagList = document.getElementById("TagList");
        const selectedIndex = tagList.selectedIndex;

        if (selectedIndex >= 0) {
            tagList.options[selectedIndex] = null;
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
}}

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