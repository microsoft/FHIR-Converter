// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

const dataFiles = [ 'C-CDA_R2-1_CCD.xml.cda', '170.314B2_Amb_CCD.cda', 'Patient-1.cda', 'sample.cda' ];
const templateFiles = [ 'ccd.hbs' ];

module.exports = () => {
    const cases = [ ];
    dataFiles.forEach(dataFile => cases.push(
        ...templateFiles.map(templateFile => 
            ({ dataFile, templateFile }))));
    return cases;
};