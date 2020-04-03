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
            //console.log(msg);
            //console.log(`submitted xmltojson`);
            //console.log(result);
            //console.log(typeof result);
            //console.log(`before fulfill in parsecda`);
            fulfill(result);
            //return result; //JSON.parse(result);
        });
    });
}

module.exports.parseCDA = parseCDA;
