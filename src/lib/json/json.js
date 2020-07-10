// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

let dataHandler = require('../dataHandler/dataHandler');

module.exports = class json extends dataHandler {
    constructor() {
        super("json");
    }

    parseSrcData(data) {
        return new Promise((fulfill) => {
            fulfill(JSON.parse(data));
        });
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
