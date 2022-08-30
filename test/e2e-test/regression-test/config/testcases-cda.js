// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

const dataFiles = ['Chirp_CCD.cda'];
const templateFiles = [ 'ccd.hbs' ];

module.exports = () => {
    const cases = [ ];
    dataFiles.forEach(dataFile => cases.push(
        ...templateFiles.map(templateFile =>
            ({ dataFile, templateFile }))));
    return cases;
};
