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
        return JSON.stringify(result, null, "\t");
    return '';
};

var onePatient = function(resJson){
    var resources = fhir.evaluate(resJson, 'Bundle.entry.resource.resourceType');
    var patientCount = testUtils.countOccurences(resources,'Patient');
    if( patientCount !== 1)
        return 'The bundle contains ' + patientCount + ' Patient resources';
    else
        return '';
};

var noDefaultGUID = function(resJson){
    var ids = fhir.evaluate(resJson, 'Bundle.entry.resource.id');
    var defaultGUIDCount = testUtils.countOccurences(ids, testUtils.defaultGUID);
    if(defaultGUIDCount > 0)
        return 'The bundle contains ' + defaultGUIDCount + ' default GUID ' + testUtils.defaultGUID;
    else
        return '';
};

var noSameGUID = function(resJson){
    var ids = fhir.evaluate(resJson, 'Bundle.entry.resource.id');
    var duplicates = testUtils.findDuplicates(ids);
    if(duplicates.length !== 0)
        return 'The bundle contains some duplicate GUID: ' + duplicates.toString();
    else
        return '';
};

module.exports = {
    fhirR4Validation: fhirR4Validation,
    onePatient: onePatient,
    noDefaultGUID: noDefaultGUID,
    noSameGUID: noSameGUID
};

