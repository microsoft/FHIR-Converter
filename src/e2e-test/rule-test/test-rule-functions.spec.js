// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

var assert = require('assert');
var testRules = require('./test-rule-functions');
var fs = require('fs');
var path  = require('path');

var onePatientBundle = {
    "resourceType": "Bundle",
    "type": "batch",
    "entry": [
        {
            "fullUrl": "urn:uuid:40386838-40ff-3f80-b68b-4904de7e7b7b",
            "resource": {
                "resourceType": "Composition",
                "id": "40386838-40ff-3f80-b68b-4904de7e7b7b",
                "identifier": {
                    "use": "official",
                    "value": "2.16.840.1.113883.19.5.99999.1"
                }
            }
        },
        {
            "fullUrl": "urn:uuid:2745d583-e3d1-3f88-8b21-7b59adb60779",
            "resource": {
                "resourceType": "Patient",
                "meta": {
                    "profile": [
                        "http://hl7.org/fhir/us/core/StructureDefinition/us-core-patient"
                    ]
                },
                "id": "2745d583-e3d1-3f88-8b21-7b59adb60779",
            }
        }
    ]
};

var twoPatientSameGuidBundle = {
    "resourceType": "Bundle",
    "type": "batch",
    "entry": [
        {
            "fullUrl": "urn:uuid:40386838-40ff-3f80-b68b-4904de7e7b7b",
            "resource": {
                "resourceType": "Composition",
                "id": "40386838-40ff-3f80-b68b-4904de7e7b7b",
                "identifier": {
                    "use": "official",
                    "value": "2.16.840.1.113883.19.5.99999.1"
                }
            }
        },
        {
            "fullUrl": "urn:uuid:2745d583-e3d1-3f88-8b21-7b59adb60779",
            "resource": {
                "resourceType": "Patient",
                "meta": {
                    "profile": [
                        "http://hl7.org/fhir/us/core/StructureDefinition/us-core-patient"
                    ]
                },
                "id": "2745d583-e3d1-3f88-8b21-7b59adb60779",
            }
        },
        {
            "fullUrl": "urn:uuid:2745d583-e3d1-3f88-8b21-7b59adb60779",
            "resource": {
                "resourceType": "Patient",
                "meta": {
                    "profile": [
                        "http://hl7.org/fhir/us/core/StructureDefinition/us-core-patient"
                    ]
                },
                "id": "2745d583-e3d1-3f88-8b21-7b59adb60779",
            }
        }
    ]
};

var patientEncounterSameGuidBundle = {
    "resourceType": "Bundle",
    "type": "batch",
    "entry": [
        {
            "fullUrl": "urn:uuid:40386838-40ff-3f80-b68b-4904de7e7b7b",
            "resource": {
                "resourceType": "Composition",
                "id": "40386838-40ff-3f80-b68b-4904de7e7b7b",
                "identifier": {
                    "use": "official",
                    "value": "2.16.840.1.113883.19.5.99999.1"
                }
            }
        },
        {
            "fullUrl": "urn:uuid:2745d583-e3d1-3f88-8b21-7b59adb60779",
            "resource": {
                "resourceType": "Patient",
                "meta": {
                    "profile": [
                        "http://hl7.org/fhir/us/core/StructureDefinition/us-core-patient"
                    ]
                },
                "id": "2745d583-e3d1-3f88-8b21-7b59adb60779",
            }
        },
        {
            "fullUrl": "urn:uuid:2745d583-e3d1-3f88-8b21-7b59adb60779",
            "resource": {
                "resourceType": "Encounter",
                "meta": {
                    "profile": [
                        "http://hl7.org/fhir/us/core/StructureDefinition/us-core-encounter"
                    ]
                },
                "id": "2745d583-e3d1-3f88-8b21-7b59adb60779",
            }
        }
    ]
};

var defaultGuidBundle = {
    "resourceType": "Bundle",
    "type": "batch",
    "entry": [
        {
            "fullUrl": "urn:uuid:40386838-40ff-3f80-b68b-4904de7e7b7b",
            "resource": {
                "resourceType": "Composition",
                "id": "40386838-40ff-3f80-b68b-4904de7e7b7b",
                "identifier": {
                    "use": "official",
                    "value": "2.16.840.1.113883.19.5.99999.1"
                }
            }
        },
        {
            "fullUrl": "urn:uuid:4cfe8d6d-3fc8-3e41-b921-f204be18db31",
            "resource": {
                "resourceType": "Patient",
                "meta": {
                    "profile": [
                        "http://hl7.org/fhir/us/core/StructureDefinition/us-core-patient"
                    ]
                },
                "id": "4cfe8d6d-3fc8-3e41-b921-f204be18db31",
            }
        }
    ]
};

describe('testRule', function () {

    it('Rule fhirR4Validation should return a object with valid status and empty string when the bundle is a standard FHIR R4 data', function () {
        var resJson = JSON.parse(fs.readFileSync(path.join(__dirname, './test-samples/FHIR R4/sample1.json')));
        assert.strictEqual(testRules.fhirR4Validation(resJson).valid, true);
        assert.strictEqual(testRules.fhirR4Validation(resJson).errorMessage, '');
    });

    it('Rule fhirR4Validation should return a object with invalid status and error message when the bundle is not a standard FHIR R4 data', function () {
        assert.strictEqual(testRules.fhirR4Validation(onePatientBundle).valid, false);
        assert.strictEqual(JSON.parse(testRules.fhirR4Validation(onePatientBundle).errorMessage).valid, false);
    });

    it('Rule onePatient should return a object with valid status and empty string when there is one Patient resourse', function () {
        assert.strictEqual(testRules.onePatient(onePatientBundle).valid, true);
        assert.strictEqual(testRules.onePatient(onePatientBundle).errorMessage, '');
    });

    it('Rule onePatient should return a object with invalid status and error message when there are more than one Patient resourse', function () {
        assert.strictEqual(testRules.onePatient(twoPatientSameGuidBundle).valid, false);
        assert.strictEqual(testRules.onePatient(twoPatientSameGuidBundle).errorMessage, 'The bundle contains 2 Patient resources');
    });

    it('Rule noDefaultGuid should return a object with valid status and empty string when there is no default Guid', function () {
        assert.strictEqual(testRules.noDefaultGuid(onePatientBundle).valid, true);
        assert.strictEqual(testRules.noDefaultGuid(onePatientBundle).errorMessage, '');
    });

    it('Rule noDefaultGuid should return a object with invalid status and error message when there is default Guid', function () {
        assert.strictEqual(testRules.noDefaultGuid(defaultGuidBundle).valid, false);
        assert.strictEqual(testRules.noDefaultGuid(defaultGuidBundle).errorMessage, 'The bundle contains 1 default Guid(s) 4cfe8d6d-3fc8-3e41-b921-f204be18db31');
    });

    it('Rule noSameGuid should return a object with valid status and empty string when there is no duplicate Guid', function () {
        assert.strictEqual(testRules.noSameGuid(onePatientBundle).valid, true);
        assert.strictEqual(testRules.noSameGuid(onePatientBundle).errorMessage, '');
        assert.strictEqual(testRules.noSameGuid(patientEncounterSameGuidBundle).valid, true);
        assert.strictEqual(testRules.noSameGuid(patientEncounterSameGuidBundle).errorMessage, '');
    });

    it('Rule noSameGuid should return a object with invalid status and error message when there are duplicate Guids', function () {
        assert.strictEqual(testRules.noSameGuid(twoPatientSameGuidBundle).valid, false);
        assert.strictEqual(testRules.noSameGuid(twoPatientSameGuidBundle).errorMessage, 'The bundle contains some duplicate Guids: Patient/2745d583-e3d1-3f88-8b21-7b59adb60779');
    });
});