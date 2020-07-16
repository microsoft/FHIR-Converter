// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

const _ = require('lodash');

const getGroundTruthFileName = testCase => 
    `${testCase.templateFile}-${testCase.dataFile}.json`;

// To make sure the error message is in a relatively fine grain,
// manually detect the json properties in the first level.
const compareContent = (content, groundTruth) => {
    const left = JSON.parse(content);
    const right = JSON.parse(groundTruth);
    const leftPros = Object.keys(left);
    const rightPros = Object.keys(right);

    const totalPros = _.union(leftPros, rightPros);
    const leftDiffs = _.xor(leftPros, totalPros);
    const rightDiffs = _.xor(rightPros, totalPros);

    if (!_.isEmpty(leftDiffs)) {
        return new Error(`The conversion result lacks these properties: ${leftDiffs}`);
    }
    else if (!_.isEmpty(rightDiffs)) {
        return new Error(`The conversion result has these extra properties: ${rightDiffs}`);
    }
    else {
        for (let prop of totalPros) {
            if (!_.isEqual(left[prop], right[prop])) {
                return new Error(`The conversion result has different property: [${prop}]`);
            }
        }
        return true;
    }
};

module.exports = {
    getGroundTruthFileName,
    compareContent
};