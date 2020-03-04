// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

var uuidv3 = require('uuid/v3');
var HandlebarsUtils = require('handlebars').Utils;
var constants = require('../constants/constants');
var HandlebarsConverter = require('./handlebars-converter');
var fs = require('fs');
var templatePreprocessor = require('../inputProcessor/templatePreprocessor');
var jsonProcessor = require('../outputProcessor/jsonProcessor');
var resourceMerger = require('../outputProcessor/resourceMerger');
var specialCharProcessor = require('../inputProcessor/specialCharProcessor');

// Some helpers will be referenced in other helpers and declared outside the export below.
var getSegmentListsInternal = function (msg, ...segmentIds) {
    var ret = {};
    for (var s = 0; s < segmentIds.length - 1; s++) { //-1 because segmentsIds includes the full message at the end
        var segOut = [];
        for (var i = 0; i < msg.meta.length; i++) {
            if (msg.meta[i] == segmentIds[s]) {
                segOut.push(msg.data[i]);
            }
        }
        ret[segmentIds[s]] = segOut;
    }
    return ret;
};

var getDate = function (dateTimeString) {
    var ds = dateTimeString.toString();
    ds = ds.padEnd(17, '0');
    var year = ds.slice(0, 4);
    var monthIndex = ds.slice(4, 6) - 1;
    if (monthIndex < 0) {
        monthIndex = 0;
    }
    var day = ds.slice(6, 8);
    var hour = ds.slice(8, 10);
    var minute = ds.slice(10, 12);
    var second = ds.slice(12, 14);
    var millisecond = ds.slice(14, 17);
    return (new Date(Date.UTC(year, monthIndex, day, hour, minute, second, millisecond)));
};

module.exports.internal = {
    getDate: getDate
};

module.exports.external = [
    {
        name: 'if',
        description: 'Checks a conditional and then follows a positive or negative path based on its value: if conditional',
        func: function (conditional, options) {
            if (arguments.length != 2) {
                throw '#if requires exactly one argument';
            }

            if (HandlebarsUtils.isFunction(conditional)) {
                conditional = conditional.call(this);
            }

            // Forces all elements of the conditional to be touched.
            JSON.stringify(conditional);

            if ((!options.hash.includeZero && !conditional) || HandlebarsUtils.isEmpty(conditional)) {
                return options.inverse(this);
            } else {
                return options.fn(this);
            }
        }
    },
    {
        name: 'eq',
        description: 'Equals at least one of the values: eq x a b …',
        func: function (x, ...values) {
            return Array.prototype.slice.call(values.slice(0, -1)).some(a => x == a); //last element is full msg
        }
    },
    {
        name: 'ne',
        description: 'Not equal to any value: ne x a b …',
        func: function (x, ...values) {
            return Array.prototype.slice.call(values.slice(0, -1)).every(a => x != a); //last element is full msg
        }
    },
    {
        name: 'lt',
        description: 'Less than: lt a b',
        func: function (v1, v2) {
            return v1 < v2;
        }
    },
    {
        name: 'gt',
        description: 'Greater than: gt a b',
        func: function (v1, v2) {
            return v1 > v2;
        }
    },
    {
        name: 'lte',
        description: 'Less than or equal: lte a b',
        func: function (v1, v2) {
            return v1 <= v2;
        }
    },
    {
        name: 'gte',
        description: 'Greater than or equal: gte a b',
        func: function (v1, v2) {
            return v1 >= v2;
        }
    },
    {
        name: 'not',
        description: 'Not true: not x',
        func: function (val) {
            return (!val);
        }
    },
    {
        name: 'and',
        description: 'Checks if all input arguments are true: and a b …',
        func: function (...inputElements) {
            return Array.prototype.slice.call(inputElements.slice(0, -1)).every(Boolean); //last element is full msg
        }
    },
    {
        name: 'or',
        description: 'Check if all input arguments are false: or a b …',
        func: function (...inputElements) {
            return Array.prototype.slice.call(inputElements.slice(0, -1)).some(Boolean); //last element is full msg
        }
    },
    {
        name: 'elementAt',
        description: 'Returns array element at position index: elementAt array index',
        func: function (arr, index) {
            return arr[index];
        }
    },
    {
        name: 'charAt',
        description: 'Returns char at position index: charAt string index',
        func: function (stringOrArray, index) {
            try {
                return stringOrArray.toString().charAt(index);
            }
            catch (err) {
                throw `helper "charAt" : ${err}`;
            }
        }
    },
    {
        name: 'length',
        description: 'Returns array length: length array',
        func: function (arr) {
            return arr ? arr.length : 0;
        }
    },
    {
        name: 'strLength',
        description: 'Returns string length: strLength string',
        func: function (str) {
            return str ? str.toString().length : 0;
        }
    },
    {
        name: 'slice',
        description: 'Returns part of an array between start and end positions (end not included): slice array start end',
        func: function (arr, start, end) {
            try {
                return arr.slice(start, end);
            }
            catch (err) {
                throw `helper "slice" : ${err}`;
            }
        }
    },
    {
        name: 'strSlice',
        description: 'Returns part of string between start and end positions (end not included): strSlice string start end',
        func: function (str, start, end) {
            try {
                return str.toString().slice(start, end);
            }
            catch (err) {
                throw `helper "strSlice" : ${err}`;
            }
        }
    },
    {
        name: 'split',
        description: 'Splits the string based on regex. e.g (split "a,b,c" ","): split string regex',
        func: function (str, regexStr) {
            try {
                return str.toString().split(new RegExp(regexStr));
            }
            catch (err) {
                throw `helper "split" : ${err}`;
            }
        }
    },
    {
        name: 'replace',
        description: 'Replaces text in a string using a regular expression: replace string searchRegex replaceStr',
        func: function (str, searchRegex, replaceStr) {
            try {
                return str.toString().replace(new RegExp(searchRegex, 'g'), replaceStr);
            }
            catch (err) {
                throw `helper "replace" : ${err}`;
            }
        }
    },
    {
        name: 'match',
        description: 'Returns an array containing matches with a regular expression: match string regex',
        func: function (str, regexStr) {
            try {
                return str.toString().match(new RegExp(regexStr, 'g'));
            }
            catch (err) {
                throw `helper "match" : ${err}`;
            }
        }
    },
    {
        name: 'base64Encode',
        description: 'Returns base64 encoded string: base64Encode string',
        func: function (str) {
            try {
                return Buffer.from(str.toString()).toString('base64');
            }
            catch (err) {
                throw `helper "base64Encode" : ${err}`;
            }
        }
    },
    {
        name: 'base64Decode',
        description: 'Returns base64 decoded string: base64Decode string',
        func: function (str) {
            try {
                return Buffer.from(str.toString(), 'base64').toString();
            }
            catch (err) {
                throw `helper "base64Decode" : ${err}`;
            }
        }
    },
    {
        name: 'escapeSpecialChars',
        description: 'Returns string with special chars escaped: escapeSpecialChars string',
        func: function (str) {
            try {
                return specialCharProcessor.Escape(str.toString());
            }
            catch (err) {
                throw `helper "escapeSpecialChars" : ${err}`;
            }
        }
    },
    {
        name: 'unescapeSpecialChars',
        description: 'Returns string after removing escaping of special char: unescapeSpecialChars string',
        func: function (str) {
            try {
                return specialCharProcessor.Unescape(str.toString());
            }
            catch (err) {
                throw `helper "unescapeSpecialChars" : ${err}`;
            }
        }
    },
    {
        name: 'assert',
        description: 'Fails with message if predicate is false: assert predicate message',
        func: function (predicate, message) {
            if (!predicate) {
                throw message;
            }
            return '';
        }
    },
    {
        name: 'evaluate',
        description: 'Returns template result object: evaluate templatePath inObj',
        func: function (templatePath, inObj) {
            try {
                let templateLocation = constants.TEMPLATE_FILES_LOCATION;

                // Safe to use existing instance here, since the instance under which this helper 
                // is being executed was refreshed when cache expired.
                var handlebarsInstance = HandlebarsConverter.instance(false, templateLocation);

                var partial = handlebarsInstance.partials[templatePath];

                if (typeof partial !== 'function') {
                    var content = fs.readFileSync(templateLocation + "/" + templatePath);

                    // register partial with compilation output
                    handlebarsInstance.registerPartial(templatePath, handlebarsInstance.compile(templatePreprocessor.Process(content.toString())));
                    partial = handlebarsInstance.partials[templatePath];
                }
                return resourceMerger.Process(JSON.parse(jsonProcessor.Process(partial(inObj.hash))));
            }
            catch (err) {
                throw `helper "evaluate" : ${err}`;
            }
        }
    },
    {
        name: 'getFieldRepeats',
        description: 'Returns repeat list for a field: getFieldRepeats fieldData',
        func: function getFieldRepeats(fieldData) {
            try {
                if (fieldData) {
                    // Mark all sub fields accessed.
                    fieldData.forEach(() => {});
                    return fieldData.repeats;
                }
                return fieldData;
            }
            catch (err) {
                throw `helper "getFieldRepeats" : ${err}`;
            }
        }
    },
    {
        name: 'getFirstSegments',
        description: "Returns first instance of the segments e.g. getFirstSegments msg.v2 'PID' 'PD1': getFirstSegments message segment1 segment2 …",
        func: function getFirstSegments(msg, ...segmentIds) {
            try {
                var ret = {};
                var inSegments = {};
                for (var s = 0; s < segmentIds.length - 1; s++) { //-1 because segmentsIds includes the full message at the end
                    inSegments[segmentIds[s]] = true;
                }
                for (var i = 0; i < msg.meta.length; i++) {
                    if (inSegments[msg.meta[i]] && !ret[msg.meta[i]]) {
                        ret[msg.meta[i]] = msg.data[i];
                    }
                }
                return ret;
            }
            catch (err) {
                throw `helper "getFirstSegments" : ${err}`;
            }
        }
    },
    {
        name: 'getSegmentLists',
        description: 'Extract HL7 v2 segments: getSegmentLists message segment1 segment2 …',
        func: function getSegmentLists(msg, ...segmentIds) {
            try {
                return getSegmentListsInternal(msg, ...segmentIds);
            }
            catch (err) {
                throw `helper "getSegmentLists" : ${err}`;
            }
        }
    },
    {
        name: 'getRelatedSegmentList',
        description: 'Given a segment name and index, return the collection of related named segments: getRelatedSegmentList message parentSegmentName parentSegmentIndex childSegmentName',
        func: function getRelatedSegmentList(msg, parentSegment, parentIndex, childSegment) {
            try {
                var ret = {};
                var segOut = [];
                var parentFound = false;
                var childIndex = -1;

                for (var i = 0; i < msg.meta.length; i++) {
                    if (msg.meta[i] == parentSegment && msg.data[i][0] == parentIndex) {
                        parentFound = true;
                    }
                    else if (msg.meta[i] == childSegment && parentFound == true) {
                        childIndex = i;
                        break;
                    }
                }

                if (childIndex > -1) {
                    do {
                        segOut.push(msg.data[childIndex]);
                        childIndex++;
                    } while (childIndex < msg.meta.length && msg.meta[childIndex] == childSegment);
                }

                ret[childSegment] = segOut;
                return ret;
            }
            catch (err) {
                throw `helper "getRelatedSegmentList" : ${err}`;
            }
        }

    },
    {
        name: 'getParentSegment',
        description: 'Given a child segment name and overall message index, return the first matched parent segment: getParentSegment message childSegmentName childSegmentIndex parentSegmentName',
        func: function getParentSegment(msg, childSegment, childIndex, parentSegment) {
            try {
                var ret = {};
                var msgIndex = -1;
                var parentIndex = -1;
                var foundChildSegmentCount = -1;

                for (var i = 0; i < msg.meta.length; i++) {
                    if (msg.meta[i] == childSegment) {
                        // count how many segments of the child type that we have found
                        // as the passed in child index is relative to the entire message
                        foundChildSegmentCount++;
                        if (foundChildSegmentCount == childIndex) {
                            msgIndex = i;
                            break;
                        }
                    }
                }

                // search backwards from the found child for the first instance
                // of the parent segment type
                for (i = msgIndex; i > -1; i--) {
                    if (msg.meta[i] == parentSegment) {
                        parentIndex = i;
                        break;
                    }
                }

                if (parentIndex > -1) {
                    ret[parentSegment] = msg.data[parentIndex];
                }

                return ret;
            }
            catch (err) {
                throw `helper "getParentSegment" : ${err}`;
            }
        }
    },
    {
        name: 'hasSegments',
        description: 'Check if HL7 v2 message has segments: hasSegments message segment1 segment2 …',
        func: function (msg, ...segmentIds) {
            try {
                var exSeg = getSegmentListsInternal(msg, ...segmentIds);
                for (var s = 0; s < segmentIds.length - 1; s++) { //-1 because segmentsIds includes the full message at the end
                    if (!exSeg[segmentIds[s]] || exSeg[segmentIds[s]].length == 0) {
                        return false;
                    }
                }
                return true;
            }
            catch (err) {
                throw `helper "hasSegments" : ${err}`;
            }
        }
    },
    {
        name: 'concat',
        description: 'Returns the concatenation of provided strings: concat aString bString cString …',
        func: function (...values) {
            return ''.concat(...(values.slice(0, -1))); //last element is full msg
        }
    },
    {
        name: 'generateUUID',
        description: 'Generates a guid based on a URL: generateUUID url',
        func: function (urlNamespace) {
            return uuidv3(''.concat(urlNamespace), uuidv3.URL);
        }
    },
    {
        name: 'addHyphensSSN',
        description: 'Adds hyphens to a SSN without hyphens: addHyphensSSN SSN',
        func: function (ssn) {
            try {
                ssn = ssn.toString();

                // Should be 9 digits
                if (!/^\d{9}$/.test(ssn)) {
                    return ssn;
                }

                return ssn.substring(0, 3) + '-' + ssn.substring(3, 5) + '-' + ssn.substring(5, 9);
            }
            catch (err) {
                return '';
            }
        }
    },
    {
        name: 'addHyphensDate',
        description: 'Adds hyphens to a date without hyphens: addHyphensDate date',
        func: function (date) {
            try {
                var bd = date.toString();

                // Should be 8 digits
                if (!/^\d{8}$/.test(bd)) {
                    return bd;
                }

                return bd.substring(0, 4) + '-' + bd.substring(4, 6) + '-' + bd.substring(6, 8);
            }
            catch (err) {
                return '';
            }
        }
    },
    {
        name: 'now',
        description: 'Provides current UTC time in YYYYMMDDHHmmssSSS format: now',
        func: function () {
            return (new Date()).toISOString().replace(/[^0-9]/g, "").slice(0, 17);
        }
    },
    {
        name: 'formatAsDateTime',
        description: 'Converts an YYYYMMDDHHmmssSSS string, e.g. 20040629175400000 to dateTime format, e.g. 2004-06-29T17:54:00.000z: formatAsDateTime(dateTimeString)',
        func: function (dateTimeString) {
            try {
                return getDate(dateTimeString).toJSON();
            }
            catch (err) {
                return '';
            }
        }
    },
    {
        // Deprecated since,
        // 1. Applying server timezone offset to client time (without timezone info) doesn't make sense.
        // 2. And passing timezone info (inside dateTimeString) unnecessarily adds complexity as compared to using only GMT/UTC.
        // Note that user has the option of using Math helpers to adjust timezones.
        name: 'convertDateTimeStringToUTC',
        description: '[Deprecated] convertDateTimeStringToUTC(dateTimeString) : converts an YYYYMMDDHHmmssSSS string, e.g. 20040629175400000 to dateTime format, e.g. 2004-06-29T17:54:00.000Z.',
        func: function (dateTimeString) {
            try {
                // "new Date(...)" behavior (local vs UTC) is OS dependent
                // (see https://stackoverflow.com/questions/22947175/new-date-operating-system-dependent).
                // Since this helper is anyway deprecated, for now, 
                // keeping time unchanged (same as before for linux instances in private preview)
                return getDate(dateTimeString).toJSON();
            }
            catch (err) {
                return '';
            }
        }
    },
    {
        name: 'toString',
        description: 'Converts to string: toString object',
        func: function (o) {
            return o.toString();
        }
    },
    {
        name: 'toJsonString',
        description: 'Converts to JSON string: toJsonString object',
        func: function (o) {
            return JSON.stringify(o);
        }
    },
    {
        name: 'toLower',
        description: 'Converts string to lower case: toLower string',
        func: function (o) {
            try {
                return o.toString().toLowerCase();
            }
            catch (err) {
                return '';
            }
        }
    },
    {
        name: 'toUpper',
        description: 'Converts string to upper case: toUpper string',
        func: function (o) {
            try {
                return o.toString().toUpperCase();
            }
            catch (err) {
                return '';
            }
        }
    },
    {
        name: 'isNaN',
        description: 'Checks if the object is not a number using JavaScript isNaN: isNaN object',
        func: function (o) {
            return isNaN(o);
        }
    }
];
