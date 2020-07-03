// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

var rules = require("../test-rule-functions");
var commonRules = [rules.onePatient, rules.noDefaultGUID, rules.noSameGUID];

module.exports = {
    commonRules: commonRules,
};