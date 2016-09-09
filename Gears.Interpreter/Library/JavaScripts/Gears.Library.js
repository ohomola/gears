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
var EXACT_MATCH_BY_TEXT = "Exact_Match_by_Text";
var PARTIAL_MATCH_BY_TEXT = "Partial_Match_by_Text";
var EXACT_MATCH_BY_ATTRIBUTE = "Exact_Match_by_Attribute";
var PARTIAL_MATCH_BY_ATTRIBUTE = "Partial_Match_by_Attribute";

function untagMatches() {
    for (var i = 0; i < allElements.length; i++) {

        var element = allElements[i];

        if (element.hasAttribute(EXACT_MATCH_BY_TEXT)) element.removeAttribute(EXACT_MATCH_BY_TEXT);
        if (element.hasAttribute(PARTIAL_MATCH_BY_TEXT)) element.removeAttribute(PARTIAL_MATCH_BY_TEXT);
        if (element.hasAttribute(EXACT_MATCH_BY_ATTRIBUTE)) element.removeAttribute(EXACT_MATCH_BY_ATTRIBUTE);
        if (element.hasAttribute(PARTIAL_MATCH_BY_ATTRIBUTE)) element.removeAttribute(PARTIAL_MATCH_BY_ATTRIBUTE);
    }
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
    var theElement = matches[0];
    theElement.dispatchEvent(new MouseEvent("mousedown", { bubbles: true }));
    theElement.dispatchEvent(new MouseEvent("mouseup", { bubbles: true }));
    try {
        theElement.click();
    } catch (err) {
        console.log("WARNING: click has caused error: " + err);
    }
}

function isHidden(el) {
    return (el.offsetParent === null);
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

//function getSiblingExactMatches(searchedText) {

//    searchedText = searchedText.toLowerCase();

//    var allElements = document.all;
//    var matches = [];

//    for (var i = 0; i < allElements.length; i++) {

//        var element = allElements[i];

//        if (element.parentNode != null) {
//            var siblings = getSiblings(element);
//            for (var s = 0; s < siblings.length; s++) {
//                if (isExactMatch(siblings[s], searchedText)) {
//                    matches.push(element);
//                    break;
//                }
//            }
//        }
//    }
//    return matches;
//}

//function getChildren(n, skipMe) {
//    var r = [];
//    for (; n; n = n.nextSibling)
//        if (n.nodeType == 1 && n != skipMe)
//            r.push(n);
//    return r;
//};

//function getSiblings(n) {
//    return getChildren(n.parentNode.firstChild, n);
//}

function getExactMatches(searchedText) {

    searchedText = searchedText.toLowerCase();

    var allElements = document.all;
    var matches = [];

    for (var i = 0; i < allElements.length; i++) {

        var element = allElements[i];

        if (isHidden(element)) {
            continue;
        }

        if (isExactMatch(element, searchedText)) {
            matches.push(element);
        }
    }

    console.log("Exact: " + matches.length);

    return matches;
}

function getOrthogonalInputs(elements) {

    var matches = [];
    var allElements = document.getElementsByTagName("input");
    //allElements.concat(document.getElementsByTagName("textArea"));
    for (var input = 0; input < allElements.length; input++) {
        for (var i = 0; i < elements.length; i++) {
            if (areOrthogonal(allElements[input], elements[i])) {
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
