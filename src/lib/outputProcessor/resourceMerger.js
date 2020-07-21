// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

var deepmerge = require('deepmerge');

module.exports.Process = function (jsonObj, baseData) {
    try {
        let mergedEntry = [];
        let resourceKeyToIndexMap = {};

        if (baseData) {

            mergedEntry.push(baseData);
            resourceKeyToIndexMap[getKey(baseData)] = mergedEntry.length - 1;
        }

        if (Object.prototype.hasOwnProperty.call(jsonObj, 'entry')) {

            for (var item of jsonObj.entry) {
                let resourceKey = getKey(item);
                if (Object.prototype.hasOwnProperty.call(resourceKeyToIndexMap, resourceKey)) {
                    let index = resourceKeyToIndexMap[resourceKey];
                    mergedEntry[index] = merge(mergedEntry[index], item);
                }
                else {
                    mergedEntry.push(item);
                    resourceKeyToIndexMap[resourceKey] = mergedEntry.length - 1;
                }
            }
            delete jsonObj.entry;
            jsonObj['entry'] = mergedEntry;
        }
        else {

            let resourceKey = getKey(jsonObj);
            if (Object.prototype.hasOwnProperty.call(resourceKeyToIndexMap, resourceKey)) {
                let index = resourceKeyToIndexMap[resourceKey];
                mergedEntry[index] = merge(mergedEntry[index], jsonObj);
            }
            else {
                mergedEntry.push(jsonObj);
                resourceKeyToIndexMap[resourceKey] = mergedEntry.length - 1;
            }

            if (mergedEntry.length === 1) {
                jsonObj = mergedEntry[0];
            }
            else {
                jsonObj = mergedEntry;
            }
        }

        return jsonObj;
    }
    catch (err) {
        return jsonObj;
    }
};

function merge(r1, r2) {
    const merged = deepmerge(r1, r2, {
        arrayMerge: mergeArr
    });

    return merged;
}

// handle "__arrIndex"="fixed", "auto", "last" (fixed not implemented yet)
// we can also think about generating JSON patch config for the document. (Note that JSON patch is more powerful since base information is not required in some ops like move/copy, though
// we can handle that by providing base as input to template)
// JSON patch : https://tools.ietf.org/html/rfc6902
const mergeArr = (target, source) => {
    let internalArrIndexKeyName = "__arrIndex";

    for (var srcIndex = 0; srcIndex < source.length; srcIndex++) {
        let tgtToMerge = target.length;
        if (!source[srcIndex].hasOwnProperty(internalArrIndexKeyName) || source[srcIndex][internalArrIndexKeyName]!=="last") {
            let maxOverlap = 0;
            for (var tgtIndex = 0; tgtIndex < target.length; tgtIndex++) {
                let currOverlap = overlap(target[tgtIndex], source[srcIndex]);
                if (currOverlap > maxOverlap) {
                    maxOverlap = currOverlap;
                    tgtToMerge = tgtIndex;
                }
            }
        }
        if (isPrimitive(source[srcIndex])) {
            target[tgtToMerge] = source[srcIndex];
        }
        else {
            delete source[srcIndex][internalArrIndexKeyName];
            target[tgtToMerge] = (tgtToMerge === target.length) ? source[srcIndex] : merge(target[tgtToMerge], source[srcIndex]);
        }
    }

    return target;
};

function overlap(o1, o2) {
    var matchCount = 0;
    for (var prop in o1) {
        if (o2.hasOwnProperty(prop)) {
            if (isObject(o1[prop])) {
                matchCount += overlap(o1[prop], o2[prop]);
            }
            else if (JSON.stringify(o1[prop]) === JSON.stringify(o2[prop])) {
                matchCount++;
            }
        }
    }
    return matchCount;
}

function isObject(val) {
    return (typeof val === 'object' && !Array.isArray(val));
}

function isPrimitive(val) {
    return (typeof val !== 'object');
}

function getKey(res) {
    if (Object.prototype.hasOwnProperty.call(res, 'resource')
        && Object.prototype.hasOwnProperty.call(res.resource, 'resourceType')) {
        let key = res.resource.resourceType;
        if (Object.prototype.hasOwnProperty.call(res.resource, 'meta')) {
            if (Object.prototype.hasOwnProperty.call(res.resource.meta, 'versionId')) {
                key = key.concat('_', res.resource.meta.versionId);
            }
        }
        if (Object.prototype.hasOwnProperty.call(res.resource, 'id')) {
            key = key.concat('_', res.resource.id);
        }
        //console.log(`getkey: ${key}`);
        return key;
    }
    return JSON.stringify(res);
}
