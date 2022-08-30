// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

const patterns = [];

module.exports = () => {
    const cases = [ ];
    patterns.forEach(pattern => {
        for (const dataFile of pattern.data) {
            cases.push({ templateFile: pattern.template, dataFile });
        }
    });
    return cases;
};
