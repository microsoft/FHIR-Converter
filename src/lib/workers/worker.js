// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

var WorkerUtils = require('./workerUtils');
var convert = require('./conversion.js').convert;

WorkerUtils.workerTaskProcessor((msg) => {
    return convert(msg);
});


