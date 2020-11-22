// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

let dataHandler = require('../dataHandler/dataHandler');

module.exports = class cda extends dataHandler {
    constructor() {
        super("redox");
    }

    parseSrcData(data) {
        let result = JSON.parse(data)
        result._originalData=data;

        return Promise.resolve(result);
    }

    preProcessTemplate(templateStr) {
        return super.preProcessTemplate(templateStr);
    }

    postProcessResult(inResult) {
        return super.postProcessResult(inResult);
    }

    getConversionResultMetadata(context) {
        return super.getConversionResultMetadata(context);
    }
};
