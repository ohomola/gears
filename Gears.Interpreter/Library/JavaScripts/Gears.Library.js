//Copyright 2016 Ondrej Homola <ondra.homola@gmail.com>
//
//This file is part of Gears, a software automation and assistance framework.
//
//Gears is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//Gears is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.

//WrappedByWebdriverExtension
function SelectWithLocation(elements) {
    var matches = [];
    for (var i = 0; i < elements.length; i++) {
        matches.push([elements[i], elements[i].getBoundingClientRect()]);
    }
    return matches;
}

//WrappedByWebdriverExtension
function GetElementsByTagNames(tags) {
    var allElements = [];

    for (var i = 0, n = tags.length; i < n; i++) {
        allElements = allElements.concat(Array.prototype.slice.call(document.getElementsByTagName(tags[i])));
    }

    console.log("getElements - allElements " + allElements.length);

    var visible = [];

    for (var i = 0; i < allElements.length; i++) {

        var element = allElements[i];

        if (isHidden(element)) {
            continue;
        }

        visible.push(element);
    }

    console.log("getElements - visible " + allElements.length);

    return visible;
};

//WrappedByWebdriverExtension
function GetElementsByText(searchedText) {

    var allElements = document.all;
    var matches = [];

    for (var i = 0; i < allElements.length; i++) {

        var element = allElements[i];

        if (isHidden(element)) {
            continue;
        }

        if (isExactMatch(element, searchedText.toLowerCase())) {
            matches.push(element);
        }
    }

    console.log("GetElementsByText: " + matches.length);

    return matches;
}

//WrappedByWebdriverExtension
function FilterOrthogonalElements(allElements, element) {
    var matches = [];
    
    for (var i = 0; i < allElements.length; i++) {
        if (!isHidden(allElements[i]) && areOrthogonal(allElements[i], element)) {
            matches.push(allElements[i]);
        }
    }

    console.log("FilterOrthogonalElements: " + matches.length);

    return matches;
}

//WrappedByWebdriverExtension
function FilterDomNeighbours(allElements, element) {

    var matches = [];
    
    for (var i = 0; i < allElements.length; i++) {

        var candidate = allElements[i];

        if (candidate.parentNode != null) {
            var siblings = getSiblings(candidate);
            for (var s = 0; s < siblings.length; s++) {
                if (siblings[s] === element) {
                    matches.push(candidate);
                    break;
                }
            }
        }
    }
    return matches;
}


function getChildren(n, skipMe) {
    var r = [];
    for (; n; n = n.nextSibling)
        if (n.nodeType == 1 && n != skipMe)
            r.push(n);
    return r;
};

function getSiblings(n) {
    return getChildren(n.parentNode.firstChild, n);
}

//WrappedByWebdriverExtension
function FilterOrthogonalElements(allElements, element) {
    var matches = [];

    for (var i = 0; i < allElements.length; i++) {
        if (!isHidden(allElements[i]) && areOrthogonal(allElements[i], element)) {
            matches.push(allElements[i]);
        }
    }

    console.log("FilterOrthogonalElements: " + matches.length);

    return matches;
}

////WrappedByWebdriverExtension
//function GetOrthogonalInputs(elements) {

//    var matches = [];
//    var allElements = document.getElementsByTagNames(["input", "textarea"]);
//    for (var input = 0; input < allElements.length; input++) {
//        for (var i = 0; i < elements.length; i++) {
//            if (!isHidden(allElements[input]) && areOrthogonal(allElements[input], elements[i])) {
//                matches.push(allElements[input]);
//            }
//        }
//    }

//    console.log("GetOrthogonalInputs: " + matches.length);

//    return matches;
//}



function getElementsByTagName(tagNames) {
    var allElements = document.getElementsByTagNames(tagNames);
    var matches = [];

    for (var i = 0; i < allElements.length; i++) {

        var element = allElements[i];

        if (isHidden(element)) {
            continue;
        }

        matches.push(element);

    }

    console.log("Found By Tag: " + matches.length);

    return matches;
}

Node.prototype.getElementsByTagNames = function (tags) {
    var elements = [];

    for (var i = 0, n = tags.length; i < n; i++) {
        elements = elements.concat(Array.prototype.slice.call(this.getElementsByTagName(tags[i])));
    }

    return elements;
};



function findInput(what, where) {
    var labelCandidates = getExactMatches(what);

    try {
        // check if one of the candidates is not actually the input
        var returnValue = firstByLocation(where, labelCandidates);
        if (returnValue !== null &&
            (returnValue.nodeName.toLowerCase() === "input" || returnValue.nodeName.toLowerCase() === "textarea")) {
            return returnValue;
        }
    } catch (err) {
    }

    if (where === "") {
        returnValue = tagMatches(firstByRelativeLocation(labelCandidates[0], getOrthogonalInputs(labelCandidates)));
    } else {
        returnValue = tagMatches([firstByLocation(where, getOrthogonalInputs(labelCandidates))]); 
    }

    return returnValue;
}

function getExactTextMatches(searchedText) {

    var allElements = document.all;
    var matches = [];

    for (var i = 0; i < allElements.length; i++) {

        var element = allElements[i];

        if (isHidden(element)) {
            continue;
        }

        if (isExactTextMatch(element, searchedText.toLowerCase())) {
            matches.push(element);
        }
    }

    console.log("Exact: " + matches.length);

    return matches;
}






function tagMatches(matches) {
    for (var i = 0; i < matches.length; i++) {
        matches[i].style.backgroundColor = "cyan";
        matches[i].style.color = "blue";
        matches[i].style.borderStyle = "solid";
        matches[i].style.borderColor = "magenta";
    }
    console.log("Tagged: " + matches.length);
    return matches;
}


function clickFirstMatch(matches) {
    return click(matches[0]);
}

function clickNthMatch(matches, n) {
    return click(matches[n]);
}

function click(theElement) {

    theElement.dispatchEvent(new Event("focus", { bubbles: true }));

    theElement.dispatchEvent(new MouseEvent("mousedown", { bubbles: true }));
    
    try {
        theElement.click();
    } catch (err) {
        console.log("WARNING: click has caused error: " + err);
    }

    theElement.dispatchEvent(new MouseEvent("mouseup", { bubbles: true }));

    return theElement;
}

function isExactTextMatch(element, searchedText) {
    var childNodes = element.childNodes;
    for (var n = 0; n < childNodes.length; n++) {
        var curNode = childNodes[n];
        if (curNode.nodeName === "#text") {
            if (curNode.nodeValue.toLowerCase() === searchedText) {
                return true;
            }
        }
    }

    return false;
}
function isExactMatch(element, searchedText) {

    var childNodes = element.childNodes;
    for (var n = 0; n < childNodes.length; n++) {
        var curNode = childNodes[n];
        if (curNode.nodeName === "#text") {
            if (curNode.nodeValue.toLowerCase() === searchedText) {
                return true;
            }
        }
    }

    var allAttributes = element.attributes;
    for (var a = 0; a < allAttributes.length; a++) {
        var attributeValue = allAttributes[a].nodeValue.toLowerCase();

        if (attributeValue === searchedText) {
            return true;
        }
    }

    return false;
}




function areOrthogonal(input, element) {
    var dif = Math.abs(input.getBoundingClientRect().bottom - element.getBoundingClientRect().bottom);
    if (dif < 5) return true;

    dif = Math.abs(input.getBoundingClientRect().left - element.getBoundingClientRect().left);
    if (dif < 5) return true;

    return false;
}







function isHidden(el) {
    if (el.offsetParent === null) {
        return true;
    }

    var style = window.getComputedStyle(el);
    return (style.display === 'none') || (style.visibility === 'hidden');
}

Node.prototype.getElementsByTagNames = function (tags) {
    var elements = [];

    for (var i = 0, n = tags.length; i < n; i++) {
        elements = elements.concat(Array.prototype.slice.call(this.getElementsByTagName(tags[i])));
    }

    return elements;
};

function distance(a, b) {
    return Math.abs(a.getBoundingClientRect().bottom - b.getBoundingClientRect().bottom) +
        Math.abs(a.getBoundingClientRect().left - b.getBoundingClientRect().left);
}


function sortByLocation(isFromRight, elements) {
    if (isFromRight) {
        return elements.sort(function (a, b) {
            return (
                a.getBoundingClientRect().top - b.getBoundingClientRect().top) * 1 +
                (b.getBoundingClientRect().left - a.getBoundingClientRect().left) *100;
        });
    }

    return elements.sort(function (a, b) {
        return (
            a.getBoundingClientRect().top - b.getBoundingClientRect().top) * 1
        + (a.getBoundingClientRect().left - b.getBoundingClientRect().left) *100 });;
}

function firstByLocation(where, elements) {
    if (where.toLowerCase() === "left") {
        var sorted = elements.sort(function (a, b) { return a.getBoundingClientRect().left - b.getBoundingClientRect().left });
        return sorted[0];
    }
    else if (where.toLowerCase() === "right") {
        var sorted = elements.sort(function (a, b) { return b.getBoundingClientRect().left - a.getBoundingClientRect().left });
        return sorted[0];
    }
    if (where.toLowerCase() === "top") {
        var sorted = elements.sort(function (a, b) { return a.getBoundingClientRect().bottom - b.getBoundingClientRect().bottom });
        return sorted[0];
    }
    else if (where.toLowerCase() === "bottom") {
        var sorted = elements.sort(function (a, b) { return b.getBoundingClientRect().bottom - a.getBoundingClientRect().bottom });
        return sorted[0];
    }
    else {
        return elements[0];
    }
}




function firstByRelativeLocation(source, elements) {
    var sorted = elements.sort(function (a, b) { return getNeighborOrder(a, source) - getNeighborOrder(b, source) });

    var value = getNeighborOrder(sorted[0], source);
    var nearest = sorted[0];
    for (var i = 0; i < sorted.length; i++) {

        if (getNeighborOrder( sorted[i],source) !== value) {
            continue;
        }

        if (distance(source, nearest) > distance(source, sorted[i])) {
            nearest = sorted[i];
        }
    }

    return nearest;
}

function firstByDistance(source, elements) {
    var sorted = elements.sort(function (a, b) { return distance(a, source) - distance(b, source) });
    return sorted[0];
}

function getNeighborOrder(input, element) {
    var dif = Math.abs(input.getBoundingClientRect().bottom - element.getBoundingClientRect().bottom);
    if (dif < 5) {
        if (input.getBoundingClientRect().right > element.getBoundingClientRect().left) {
            return 1;
        } else {
            return 4;
        }
    }

    dif = Math.abs(input.getBoundingClientRect().left - element.getBoundingClientRect().left);
    if (dif < 5) {
        if (input.getBoundingClientRect().bottom > element.getBoundingClientRect().top) {
            return 2;
        } else {
            return 3;
        }
    }

    return 100;
}
