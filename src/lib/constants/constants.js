// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

var path = require('path');

module.exports.BASE_TEMPLATE_FILES_LOCATION = path.join(__dirname, '../../templates');
module.exports.TEMPLATE_FILES_LOCATION = path.join(__dirname, '../../service-templates');
module.exports.SAMPLE_DATA_LOCATION =  path.join(__dirname, '../../sample-data');
module.exports.STATIC_LOCATION = path.join(__dirname, '../../static');
module.exports.CODE_MIRROR_LOCATION = path.join(__dirname, '../../../node_modules/codemirror/');
module.exports.MOVE_TO_GLOBAL_KEY_NAME = "_moveResourceToGlobalScope";