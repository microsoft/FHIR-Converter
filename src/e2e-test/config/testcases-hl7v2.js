// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

// design the test cases
var ruleSets = require('./rule-sets'); 
var hl7v2Testcases = [
    { dataFile: 'ADT01-28.hl7', templateFile: 'ORU_R01.hbs', testRules: ruleSets.commonRules}
];

module.exports = hl7v2Testcases;

