// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

var testUtils = require('./testUtils');
var fhirModule = require('fhir');
var fhir = new fhirModule.Fhir();

var fhirR4Validation =  function(resJson){
    var result = fhir.validate(resJson);
    if (!result.valid)
        return { valid: false, errorMessage: JSON.stringify(result, null, "\t")};
    return { valid: true, errorMessage: ''};
};

var onePatient = function(resJson){
    var resources = fhir.evaluate(resJson, 'Bundle.entry.resource.resourceType');
    var patientCount = testUtils.countOccurences(resources,'Patient');
    if( patientCount !== 1)
        return { valid: false, errorMessage: 'The bundle contains ' + patientCount + ' Patient resources'};
    else
        return { valid: true, errorMessage: ''};
};

var noDefaultGuid = function(resJson){
    var ids = fhir.evaluate(resJson, 'Bundle.entry.resource.id');
    var defaultGuidCount = testUtils.countOccurences(ids, testUtils.defaultGuid);
    if(defaultGuidCount > 0)
        return { valid: false, errorMessage: 'The bundle contains ' + defaultGuidCount + ' default Guid ' + testUtils.defaultGuid};
    else
        return { valid: true, errorMessage: ''};
};

var noSameGuid = function(resJson){
    var ids = fhir.evaluate(resJson, 'Bundle.entry.resource.id');
    var duplicates = testUtils.findDuplicates(ids);
    if(duplicates.length !== 0)
        return { valid: false, errorMessage: 'The bundle contains some duplicate Guid: ' + duplicates.toString()};
    else
        return { valid: true, errorMessage: ''};
};

module.exports = {
    fhirR4Validation: fhirR4Validation,
    onePatient: onePatient,
    noDefaultGuid: noDefaultGuid,
    noSameGuid: noSameGuid
};

