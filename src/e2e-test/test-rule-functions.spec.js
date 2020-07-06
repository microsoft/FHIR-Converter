// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

var assert = require('assert');
var testRules = require('./test-rule-functions');
var fs = require('fs');
var path  = require('path');

describe('testRule', function () {

    it('Rule fhirR4Validation should return empty string when the bundle is a standard FHIR R4 data', function () {
        var resJson = JSON.parse(fs.readFileSync(path.join(__dirname, './test-samples/FHIR R4/sample1.json')));
        assert.strictEqual(testRules.fhirR4Validation(resJson).valid, true);
        assert.strictEqual(testRules.fhirR4Validation(resJson).errorMessage, '');
    });

    it('Rule fhirR4Validation should return error message when the bundle is not a standard FHIR R4 data', function () {
        var resJson = {
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
        assert.strictEqual(testRules.fhirR4Validation(resJson).valid, false);
        assert.strictEqual(JSON.parse(testRules.fhirR4Validation(resJson).errorMessage).valid, false);
    });

    it('Rule onePatient should return empty string when there is one Patient resourse', function () {
        var resJson = {
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
        assert.strictEqual(testRules.onePatient(resJson).valid, true);
        assert.strictEqual(testRules.onePatient(resJson).errorMessage, '');
    });

    it('Rule onePatient should return error message when there are more than one Patient resourse', function () {
        var resJson = {
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
        assert.strictEqual(testRules.onePatient(resJson).valid, false);
        assert.strictEqual(testRules.onePatient(resJson).errorMessage, 'The bundle contains 2 Patient resources');
    });

    it('Rule noDefaultGuid should return empty string when there is no default Guid', function () {
        var resJson = {
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
        assert.strictEqual(testRules.noDefaultGuid(resJson).valid, true);
        assert.strictEqual(testRules.noDefaultGuid(resJson).errorMessage, '');
    });

    it('Rule noDefaultGuid should return error message when there is default Guid', function () {
        var resJson = {
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
        assert.strictEqual(testRules.noDefaultGuid(resJson).valid, false);
        assert.strictEqual(testRules.noDefaultGuid(resJson).errorMessage, 'The bundle contains 1 default Guid 4cfe8d6d-3fc8-3e41-b921-f204be18db31');
    });

    it('Rule noSameGuid should return empty string when there is no duplicate Guid', function () {
        var resJson = {
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
        assert.strictEqual(testRules.noSameGuid(resJson).valid, true);
        assert.strictEqual(testRules.noSameGuid(resJson).errorMessage, '');
    });

    it('Rule noSameGuid should return error message when there are duplicate Guids', function () {
        var resJson = {
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
                    "fullUrl": "urn:uuid:40386838-40ff-3f80-b68b-4904de7e7b7b",
                    "resource": {
                        "resourceType": "Patient",
                        "meta": {
                            "profile": [
                                "http://hl7.org/fhir/us/core/StructureDefinition/us-core-patient"
                            ]
                        },
                        "id": "40386838-40ff-3f80-b68b-4904de7e7b7b",
                    }
                }
            ]
        };
        assert.strictEqual(testRules.noSameGuid(resJson).valid, false);
        assert.strictEqual(testRules.noSameGuid(resJson).errorMessage, 'The bundle contains some duplicate Guid: 40386838-40ff-3f80-b68b-4904de7e7b7b');
    });
});