// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

// design the test cases
var ruleSets = require('./rule-sets'); 
var cdaTestcases = [
    { dataFile: 'C-CDA_R2-1_CCD.xml.cda', templateFile: 'ccd.hbs', testRules: ruleSets.guidRules},
    { dataFile: '170.314B2_Amb_CCD.cda', templateFile: 'ccd.hbs', testRules: ruleSets.commonRules},
    { dataFile: 'Patient-1.cda', templateFile: 'ccd.hbs', testRules: ruleSets.commonRules},
    { dataFile: 'sample.cda', templateFile: 'ccd.hbs', testRules: ruleSets.commonRules}
];

module.exports = cdaTestcases;

