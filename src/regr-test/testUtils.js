// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

const _ = require('lodash');
const MAX_COMPARISION_DEPTH = 10;

const getGroundTruthFileName = testCase => {
    if (!!testCase && !!testCase.templateFile && !!testCase.dataFile) {
        return `${testCase.templateFile}-${testCase.dataFile}.json`;
    }
    throw new Error(`The testCase should both have property [templateFile] and [dataFile].`);
}

const __compareContent = (propPrefix, left, right, depth) => {
    if (depth >= MAX_COMPARISION_DEPTH) {
        if (!_.isEqual(left, right)) {
            throw new Error(`The conversion result has different property: [${propPrefix}]`);
        }
        return true;
    }

    const objectFlag = _.isPlainObject(left) && _.isPlainObject(right);
    const arrayFlag = _.isArray(left) && _.isArray(right);
    
    if (!objectFlag && !arrayFlag) {
        throw new Error('Inner comparator\'s parameters should both be plain object or array.');
    }

    if (objectFlag) {
        const leftPros = Object.keys(left);
        const rightPros = Object.keys(right);
    
        const totalPros = _.union(leftPros, rightPros);
        const leftDiffs = _.xor(leftPros, totalPros);
        const rightDiffs = _.xor(rightPros, totalPros);
    
        if (!_.isEmpty(leftDiffs)) {
            throw new Error(`The conversion result lacks these properties: [${propPrefix}[${leftDiffs.toString()}]]`);
        }
        else if (!_.isEmpty(rightDiffs)) {
            throw new Error(`The conversion result has these extra properties: [${propPrefix}[${rightDiffs.toString()}]]`);
        }
        else {
            for (const prop of totalPros) {
                const subObjectFlag = _.isPlainObject(left[prop]) && _.isPlainObject(right[prop]);
                const subArrayFlag = _.isArray(left[prop]) && _.isArray(right[prop]);
                
                if (subObjectFlag || subArrayFlag) {
                    __compareContent(`${propPrefix}${prop}.`, left[prop], right[prop], depth + 1);
                }
                else if (!_.isEqual(left[prop], right[prop])) {
                    throw new Error(`The conversion result has different property: [${propPrefix}${prop}]`);
                }
            }
            return true;
        }
    }

    // TODO: The array comparision can be done in a
    // finer granularity
    else if (arrayFlag) {
        if (!_.isEqual(left, right)) {
            throw new Error(`The conversion result has different property: [${propPrefix}Array]`);
        }
        return true;
    }
};

const compareContent = (content, groundTruth) => {
    if (typeof content !== 'string' || typeof groundTruth !== 'string') {
        throw new Error('The parameters must be both string type.');
    }

    const left = JSON.parse(content);
    const right = JSON.parse(groundTruth);
    __compareContent('', left, right, 0);
};

module.exports = {
    getGroundTruthFileName,
    compareContent
};