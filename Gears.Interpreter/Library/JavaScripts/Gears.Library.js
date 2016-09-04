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

function getMatches(searchedText) {

    searchedText = searchedText.toLowerCase();
    
    var allElements = document.all;
    var matches =[];

    for (var i = 0; i < allElements.length; i++) {

        var element = allElements[i];

        if (isHidden(element)) {
            continue;
        }

        var childNodes = element.childNodes;
        for (var n = 0; n < childNodes.length; n++) {
            var curNode = childNodes[n];
            if (curNode.nodeName === "#text") {
                if (curNode.nodeValue.toLowerCase() === searchedText) {
                    matches.push(element);
                    break;
                }
            }
        }
        
        var allAttributes = element.attributes;
        for (var a = 0; a < allAttributes.length; a++) {
            var attributeValue = allAttributes[a].nodeValue.toLowerCase();

            if (attributeValue === searchedText) {
                matches.push(element);
                break;
            }
        }
    }

    return matches;
}



