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
}