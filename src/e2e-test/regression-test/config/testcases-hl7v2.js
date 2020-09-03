// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

const patterns = [
    { templateFile: 'ADT_A01.hbs', dataFiles: [ 'ADT01-23.hl7', 'ADT01-28.hl7' ] },
    { templateFile: 'VXU_V04.hbs', dataFiles: [ 'VXU.hl7' ] },
    { templateFile: 'ORU_R01.hbs', dataFiles: [
        'LAB-ORU-1.hl7', 'LAB-ORU-2.hl7', 'LRI_2.0-NG_CBC_Typ_Message.hl7', 'EKG-LAB-ORU-R01-1.hl7',
        'EKG-LAB-ORU-R01-4.hl7', 'GOOD-LAD-ORU-R01-1.hl7','ORU-R01-RMGEAD.hl7'
    ]},
    { templateFile: 'OML_O21.hbs', dataFiles: [ 'MDHHS-OML-O21-1.hl7', 'MDHHS-OML-O21-2.hl7' ] }
];

module.exports = () => {
    const cases = [ ];
    patterns.forEach(pattern => {
        for (const dataFile of pattern.dataFiles) {
            cases.push({ templateFile: pattern.templateFile, dataFile });
        }
    });
    return cases;
};