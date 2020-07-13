// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

var testUtils = require('./testUtils');
var fhirModule = require('fhir');
var fhir = new fhirModule.Fhir();

var response = function(status, message=''){
    return { valid: status, errorMessage: message};
};

var fhirR4Validation =  function(resJson){
    var result = fhir.validate(resJson);
    if (!result.valid)
        return response(false, JSON.stringify(result, null, "\t"));
    return response(true);
};

var onePatient = function(resJson){
    var resources = fhir.evaluate(resJson, 'Bundle.entry.resource.resourceType');
    var patientCount = testUtils.countOccurences(resources,'Patient');
    if( patientCount !== 1)
        return response(false, 'The bundle contains ' + patientCount + ' Patient resources');
    else
        return response(true);
};

var noDefaultGuid = function(resJson){
    var ids = fhir.evaluate(resJson, 'Bundle.entry.resource.id');
    var defaultGuidCount = testUtils.countOccurences(ids, testUtils.defaultGuid);
    if(defaultGuidCount >= 1)
        return response(false, 'The bundle contains ' + defaultGuidCount + ' default Guid(s) ' + testUtils.defaultGuid);
    else
        return response(true);
};

var noSameGuid = function(resJson){
    var resources = fhir.evaluate(resJson, 'Bundle.entry.resource');
    var ids = [];
    for(var index in resources){
        ids.push(resources[index].resourceType + '/' + resources[index].id);
    }
    var duplicates = testUtils.findDuplicates(ids);
    if(duplicates.length !== 0)
        return response(false, 'The bundle contains some duplicate Guids: ' + duplicates.toString());
    else
        return response(true);
};

module.exports = {
    fhirR4Validation: fhirR4Validation,
    onePatient: onePatient,
    noDefaultGuid: noDefaultGuid,
    noSameGuid: noSameGuid
};

