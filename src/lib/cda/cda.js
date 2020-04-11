// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------
var parseString = require('xml2js').parseString;

function parseCDA(msg) {
    return new Promise((fulfill, reject) => {
        parseString(msg, { trim: true, explicitCharkey :true, mergeAttrs: true, explicitArray:false }, function (err, result) {
            if (err) {
                reject(err);
            }
            result._originalData=msg;
            fulfill(result);
        });
    });
}

module.exports.parseCDA = parseCDA;
