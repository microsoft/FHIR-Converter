// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

const dataFiles = [ 'ADT01-28.hl7' ];
const templateFiles = [ 'ORU_R01.hbs' ];

module.exports = () => {
    const cases = [ ];
    dataFiles.forEach(dataFile => cases.push(
        ...templateFiles.map(templateFile => 
            ({ dataFile, templateFile }))));
    return cases;
};