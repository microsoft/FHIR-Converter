// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

var ruleSets = require('./rule-sets'); 
var hl7v2Testcases = [
    { dataFile: 'ADT01-28.hl7', templateFile: 'ORU_R01.hbs', testRules: ruleSets.commonRules},

    // error templeates with default GUID
    { dataFile: 'ADT01-23.hl7', templateFile: 'ADT_A01.hbs', testRules: ruleSets.commonRules},
    { dataFile: 'ADT04-23.hl7', templateFile: 'ADT_A01.hbs', testRules: ruleSets.commonRules},
    { dataFile: 'ADT04-251.hl7', templateFile: 'ADT_A01.hbs', testRules: ruleSets.commonRules},
];

module.exports = hl7v2Testcases;

