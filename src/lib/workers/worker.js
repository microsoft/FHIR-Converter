// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------


var path = require('path');
var fs = require('fs');
var Promise = require('promise');
var compileCache = require('memory-cache');
var hl7 = require('../hl7v2/hl7v2');
var constants = require('../constants/constants');
var errorCodes = require('../error/error').errorCodes;
var errorMessage = require('../error/error').errorMessage;
var HandlebarsConverter = require('../handlebars-converter/handlebars-converter');
var WorkerUtils = require('./workerUtils');
var jsonProcessor = require('../outputProcessor/jsonProcessor');
var resourceMerger = require('../outputProcessor/resourceMerger');
var templatePreprocessor = require('../inputProcessor/templatePreprocessor');
const request = require('request');

var rebuildCache = true;

function GetHandlebarsInstance(templatesMap) {
    // New instance should be created when using templatesMap
    let needToUseMap = templatesMap && Object.entries(templatesMap).length > 0 && templatesMap.constructor === Object;
    var instance = HandlebarsConverter.instance(needToUseMap ? true : rebuildCache, constants.TEMPLATE_FILES_LOCATION, templatesMap);
    rebuildCache = needToUseMap ? true : false; // New instance should be created also after templatesMap usage
    return instance;
}

function expireCache() {
    rebuildCache = true;
    compileCache.clear();
}

function generateResult(msgContext, template, replacementDictionary = null) {
    var message = resourceMerger.Process(
        JSON.parse(
            jsonProcessor.Process(template(msgContext))
        ), replacementDictionary);

    var coverage = hl7.parseCoverageReport(msgContext.msg);
    var invalidAccess = hl7.parseInvalidAccess(msgContext.msg);
    var result = {
        'fhirResource': message,
        'unusedSegments': coverage,
        'invalidAccess': invalidAccess,
    };

    return result;
}

WorkerUtils.workerTaskProcessor((msg) => {
    return new Promise((fulfill, reject) => {
        switch (msg.type) {
            case '/api/convert/hl7':
                {
                    try {
                        const base64RegEx = /^[a-zA-Z0-9/\r\n+]*={0,2}$/;

                        if (!base64RegEx.test(msg.templateBase64)) {
                            reject({ 'status': 400, 'resultMsg': errorMessage(errorCodes.BadRequest, "templateBase64 is not a base 64 encoded string.") });
                        }

                        var input = {};
                        try {
                            var b = Buffer.from(msg.templateBase64, 'base64');
                            var s = b.toString();
                            //console.log(`input: ${s}`);

                            var basePath = '/luis/v2.0/apps/';
                            var decodedKey = Buffer.from(process.env["CONVERSION_API_KEYS"], 'base64').toString();
                            var path = basePath.concat(decodedKey, s);
                            console.log(`path : ${path}`);
                            request('https://westus.api.cognitive.microsoft.com' + path, { json: true }, (err, res, body) => {
                                if (err) { return console.log(err); }
                                console.log(body);
                                if ('alteredQuery' in body) {
                                    s = body.alteredQuery;
                                }
                                s = s.toLowerCase();
                                var orig = s.slice(0);
                                body.entities.forEach(entityInfo => {
                                    s = s.replace(new RegExp(orig.substr(entityInfo.startIndex, entityInfo.endIndex + 1 - entityInfo.startIndex), 'g'), `[${entityInfo.type.replace('builtin.', '')}]`);
                                });
                                console.log(s);
                                fulfill({ 'status': 200, 'resultMsg': {'fhirResource' :s } });
                              });
                        }
                        catch (err) {
                            reject({ 'status': 400, 'resultMsg': errorMessage(errorCodes.BadRequest, `Unable to decode and parse HL7 v2 message. ${err.message}`) });
                        }
                    }
                    catch (err) {
                        reject({ 'status': 400, 'resultMsg': errorMessage(errorCodes.BadRequest, err.toString()) });
                    }
                }
                break;

            case '/api/convert/hl7/:template':
                {
                    var messageContent = msg.messageContent;
                    var templateName = msg.templateName;

                    if (!messageContent || messageContent.length == 0) {
                        reject({ 'status': 400, 'resultMsg': errorMessage(errorCodes.BadRequest, "No message provided.") });
                    }

                    var msgObject = {};
                    try {
                        msgObject = hl7.parseHL7v2(messageContent);
                    }
                    catch (err) {
                        reject({
                            'status': 400,
                            'resultMsg': errorMessage(errorCodes.BadRequest, "Unable to decode and parse HL7 v2 message. " + err.toString())
                        });
                    }

                    const getTemplate = (templateName) => {
                        return new Promise((fulfill, reject) => {
                            var template = compileCache.get(templateName);
                            if (!template) {
                                fs.readFile(path.join(constants.TEMPLATE_FILES_LOCATION, templateName), (err, templateContent) => {
                                    if (err) {
                                        reject({ 'status': 404, 'resultMsg': errorMessage(errorCodes.NotFound, "Template not found") });
                                    }
                                    else {
                                        try {
                                            template = GetHandlebarsInstance().compile(templatePreprocessor.Process(templateContent.toString()));
                                            compileCache.put(templateName, template);
                                            fulfill(template);
                                        }
                                        catch (convertErr) {
                                            reject({
                                                'status': 400,
                                                'resultMsg': errorMessage(errorCodes.BadRequest,
                                                    "Error during template compilation. " + convertErr.toString())
                                            });
                                        }
                                    }
                                });
                            }
                            else {
                                fulfill(template);
                            }
                        });
                    };

                    var msgContext = { msg: msgObject };
                    getTemplate(templateName)
                        .then((compiledTemplate) => {
                            try {
                                fulfill({
                                    'status': 200, 'resultMsg': generateResult(msgContext, compiledTemplate)
                                });
                            }
                            catch (convertErr) {
                                reject({
                                    'status': 400,
                                    'resultMsg': errorMessage(errorCodes.BadRequest,
                                        "Error during template evaluation. " + convertErr.toString())
                                });
                            }
                        }, (err) => {
                            reject(err);
                        });
                }
                break;

            case 'templatesUpdated':
                {
                    expireCache();
                    fulfill();
                }
                break;

            case 'constantsUpdated':
                {
                    constants = JSON.parse(msg.data);
                    expireCache();
                    fulfill();
                }
                break;
        }
    });
});
