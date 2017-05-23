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
function GetAllElements() {
    var allElements = document.all;
    var matches = [];

    for (var i = 0; i < allElements.length; i++) {
        matches.push(allElements[i]);
    }

    return matches;
};

function GetElementsByAttributeValues(names, values) {
    var allElements = [];
    for (var i = 0, n = names.length; i < n; i++) {
        var elementsFoundByAttributeValue = findByAttributeValue(names[i], values[i]);
        if (elementsFoundByAttributeValue != null) {
            allElements = allElements.concat(Array.prototype.slice.call(elementsFoundByAttributeValue));
        }
    }
    return allElements;
}

//WrappedByWebdriverExtension
function GetElementByCoordinates(x, y) {

    var hitElements = [];

    var allElements = document.all;
    for (var i = 0; i < allElements.length; i++) {

        var el = allElements[i];
        var rect = el.getBoundingClientRect();

        if (rect.left < x && (rect.left + rect.width > x) && (rect.top < y && (rect.top + rect.height > y))) {

            hitElements.push(el);

        }

    };

    return hitElements;
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
function FilterElementsByText(searchedText, allElements, matchWhenTextIsInChild) {
    
    var matches = [];

    for (var i = 0; i < allElements.length; i++) {

        var element = allElements[i];

        if (isHidden(element)) {
            continue;
        }

        if (matchWhenTextIsInChild) {
            if (isExactMatchOrChild(element, searchedText.toLowerCase())) {
                matches.push(element);
            }
        } else {
            if (isExactMatch(element, searchedText.toLowerCase())) {
                matches.push(element);
            }
        }
    }

    console.log("GetElementsByText: " + matches.length);

    return matches;
}



//WrappedByWebdriverExtension
function FilterElementsByPartialText(searchedText, allElements, matchWhenTextIsInChild) {

    var matches = [];

    for (var i = 0; i < allElements.length; i++) {

        var element = allElements[i];

        if (isHidden(element)) {
            continue;
        }

        if (matchWhenTextIsInChild) {
            if (isPartialMatchOrChild(element, searchedText.toLowerCase())) {
                matches.push(element);
            }
        } else {
            if (isPartialMatch(element, searchedText.toLowerCase())) {
                matches.push(element);
            }
        }
    }

    console.log("GetElementsByText: " + matches.length);

    return matches;
}

//WrappedByWebdriverExtension
function FilterOrthogonalElements(allElements, element, xTolerance, yTolerance) {
    var matches = [];
    
    for (var i = 0; i < allElements.length; i++) {
        if (!isHidden(allElements[i]) && areOrthogonal(allElements[i], element, xTolerance, yTolerance)) {
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

//WrappedByWebdriverExtension
function SelectWithLocation(elements) {

    var matches = [];
    for (var i = 0; i < elements.length; i++) {
        matches.push([elements[i], elements[i].getBoundingClientRect()]);
    }
    return matches;
}

//WrappedByWebdriverExtension
function Click(theElement) {

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


function findByAttributeValue(attribute, value) {
    var All = document.getElementsByTagName('*');
    var matches = [];
    for (var i = 0; i < All.length; i++) {
        if (All[i].getAttribute(attribute) == value) {
            matches.push(All[i]);
        }
    }
    return matches;
}
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

function isExactMatchOrChild(element, searchedText) {
    
    if (isExactMatch(element, searchedText)) {
        return true;
    }
    var childNodes = element.childNodes;
    for (var n = 0; n < childNodes.length; n++) {
        var curNode = childNodes[n];
        if (curNode.nodeType === 1 && isExactMatchOrChild(curNode, searchedText)) {
            return true;
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

function isPartialMatchOrChild(element, searchedText) {

    if (isPartialMatch(element, searchedText)) {
        return true;
    }
    var childNodes = element.childNodes;
    for (var n = 0; n < childNodes.length; n++) {
        var curNode = childNodes[n];
        if (curNode.nodeType === 1 && !isHidden(curNode) && isPartialMatchOrChild(curNode, searchedText)) {
            return true;
        }
    }
    return false;
}

function isPartialMatch(element, searchedText) {

    var childNodes = element.childNodes;
    for (var n = 0; n < childNodes.length; n++) {
        var curNode = childNodes[n];
        if (curNode.nodeName === "#text") {
            if (curNode.nodeValue.toLowerCase().indexOf(searchedText) !== -1) {
                return true;
            }
        }
    }

    var allAttributes = element.attributes;
    for (var a = 0; a < allAttributes.length; a++) {
        var attributeValue = allAttributes[a].nodeValue.toLowerCase();

        if (attributeValue.indexOf(searchedText) !== -1) {
            return true;
        }
    }

    return false;
}

function areOrthogonal(input, element, xTolerance, yTolerance) {
    var dif = Math.abs(input.getBoundingClientRect().bottom - element.getBoundingClientRect().bottom);
    if (dif < yTolerance) return true;

    dif = Math.abs(input.getBoundingClientRect().left - element.getBoundingClientRect().left);
    if (dif < xTolerance) return true;

    return false;
}

function isHidden(el) {

    if (el.tagName.toLowerCase() == 'code') {
        return true;
    }

    if (el.offsetParent === null) {
        return true;
    }

    var rect = el.getBoundingClientRect();

    if (el.style.opacity === 0) {
        return false;
    }

    if (
        rect.top < 0 ||
            rect.left < 0 ||
            rect.bottom > (window.innerHeight || document.documentElement.clientHeight) || /*or $(window).height() */
            rect.right > (window.innerWidth || document.documentElement.clientWidth) /*or $(window).width() */
    ) {
        return false;
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





