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

function getExactMatches(searchedText) {

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

function click(theElement) {
    theElement.dispatchEvent(new MouseEvent("mousedown", { bubbles: true }));
    theElement.dispatchEvent(new MouseEvent("mouseup", { bubbles: true }));
    try {
        theElement.click();
    } catch (err) {
        console.log("WARNING: click has caused error: " + err);
    }

    return theElement;
}

function isHidden(el) {
    return (el.offsetParent === null);
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

Node.prototype.getElementsByTagNames = function (tags) {
    var elements = [];

    for (var i = 0, n = tags.length; i < n; i++) {
        elements = elements.concat(Array.prototype.slice.call(this.getElementsByTagName(tags[i])));
    }

    return elements;
};

function getOrthogonalInputs(elements) {

    var matches = [];
    var allElements = document.getElementsByTagNames(["input", "textarea"]);
    for (var input = 0; input < allElements.length; input++) {
        for (var i = 0; i < elements.length; i++) {
            if (!isHidden(allElements[input]) && areOrthogonal(allElements[input], elements[i])) {
                matches.push(allElements[input]);
            }
        }
    }

    console.log("Orthogonal: "+matches.length);

    return matches;
}

function areOrthogonal(input, element) {
    var dif = Math.abs(input.getBoundingClientRect().bottom - element.getBoundingClientRect().bottom);
    if (dif < 5) return true;

    dif = Math.abs(input.getBoundingClientRect().left - element.getBoundingClientRect().left);
    if (dif < 5) return true;

    return false;
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

    var value = getNeighborOrder(source, sorted[0]);
    var nearest =  sorted[0];
    for (var i = 0; i < sorted.length; i++) {

        if (getNeighborOrder(source, sorted[i]) !== value) {
            break;
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

function distance(a, b) {
    return Math.abs(a.getBoundingClientRect().bottom - b.getBoundingClientRect().bottom) +
        Math.abs(a.getBoundingClientRect().left - b.getBoundingClientRect().left);
}

function getNeighborOrder(input, element) {
    var dif = Math.abs(input.getBoundingClientRect().bottom - element.getBoundingClientRect().bottom);
    if (dif < 5) {
        if (input.getBoundingClientRect().left > element.getBoundingClientRect().left) {
            return 1;
        } else {
            return 4;
        }
    }

    dif = Math.abs(input.getBoundingClientRect().left - element.getBoundingClientRect().left);
    if (dif < 5) {
        if (input.getBoundingClientRect().bottom > element.getBoundingClientRect().bottom) {
            return 2;
        } else {
            return 3;
        }
    }

    return false;
}


function findInput(what, where) {

    var direct = firstByLocation(where, getExactMatches(what));
    if (direct !== null && (direct.nodeName.toLowerCase() === "input" || direct.nodeName.toLowerCase() === "textarea")) {
        return direct;
    }

    if (where === "") {
        return tagMatches(click(firstByRelativeLocation(getExactMatches(what)[0],getOrthogonalInputs(getExactMatches(what)))));
    }

    return tagMatches([firstByLocation(where,getOrthogonalInputs(getExactMatches(what)))]);
}

