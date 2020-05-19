// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------


var path = require('path');
var fs = require('fs');
var Promise = require('promise');
var compileCache = require('memory-cache');
var hl7 = require('../hl7v2/hl7v2');
var cda = require('../cda/cda');
var constants = require('../constants/constants');
var errorCodes = require('../error/error').errorCodes;
var errorMessage = require('../error/error').errorMessage;
var HandlebarsConverter = require('../handlebars-converter/handlebars-converter');
var WorkerUtils = require('./workerUtils');
var jsonProcessor = require('../outputProcessor/jsonProcessor');
var resourceMerger = require('../outputProcessor/resourceMerger');

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

    //var coverage = hl7.parseCoverageReport(msgContext.msg);
    //var invalidAccess = hl7.parseInvalidAccess(msgContext.msg);
    var result = {
        'fhirResource': message,
        //'unusedSegments': coverage,
        //'invalidAccess': invalidAccess,
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

                        if (!base64RegEx.test(msg.messageBase64)) {
                            reject({ 'status': 400, 'resultMsg': errorMessage(errorCodes.BadRequest, "Message is not a base 64 encoded string.") });
                        }

                        if (!base64RegEx.test(msg.templateBase64)) {
                            reject({ 'status': 400, 'resultMsg': errorMessage(errorCodes.BadRequest, "Template is not a base 64 encoded string.") });
                        }

                        var replacementDictionary = {};
                        if (msg.replacementDictionaryBase64) {
                            if (!base64RegEx.test(msg.replacementDictionaryBase64)) {
                                reject({ 'status': 400, 'resultMsg': errorMessage(errorCodes.BadRequest, "replacementDictionary is not a base 64 encoded string.") });
                            }
                            replacementDictionary = JSON.parse(Buffer.from(msg.replacementDictionaryBase64, 'base64').toString());
                        }

                        var templatesMap = undefined;
                        if (msg.templatesMapBase64) {
                            if (!base64RegEx.test(msg.templatesMapBase64)) {
                                reject({ 'status': 400, 'resultMsg': errorMessage(errorCodes.BadRequest, "templatesMap is not a base 64 encoded string.") });
                            }
                            templatesMap = JSON.parse(Buffer.from(msg.templatesMapBase64, 'base64').toString());
                        }

                        
                        var templateString = "";
                        if (msg.templateBase64) {
                            templateString = Buffer.from(msg.templateBase64, 'base64').toString();
                        }

                        var b = Buffer.from(msg.messageBase64, 'base64');
                        var s = b.toString();
                        cda.parseCDA(s)
                        .then((msgObj) => {
                            var context = { msg: msgObj };
                            if (templateString == null || templateString.length == 0) {
                                var coverage = hl7.parseCoverageReport(context.msg);
                                var invalidAccess = hl7.parseInvalidAccess(context.msg);
                                var result = {
                                    'fhirResource': JSON.parse(JSON.stringify(context.msg)),
                                    'unusedSegments': coverage,
                                    'invalidAccess': invalidAccess,
                                };
    
                                fulfill({ 'status': 200, 'resultMsg': result });
                            }
                            else {
                                var template = GetHandlebarsInstance(templatesMap).compile(templateString);
    
                                try {
                                    fulfill({ 'status': 200, 'resultMsg': generateResult(context, template, replacementDictionary) });
                                }
                                catch (err) {
                                    reject({ 'status': 400, 'resultMsg': errorMessage(errorCodes.BadRequest, "Unable to create result: " + err.toString()) });
                                }
                            }

                        })
                        .catch(err => {
                            //console.log(err);
                            reject({ 'status': 400, 'resultMsg': errorMessage(errorCodes.BadRequest, `Unable to decode and parse input data. ${err.message}`) });
                        });
                    }
                    catch (err) {
                        //console.log(err);
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
                                            template = GetHandlebarsInstance().compile(templateContent.toString());
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
                
                    cda.parseCDA(messageContent)
                    .then((msgObject) => {
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
                    })
                    .catch(err => {
                        reject({ 'status': 400, 'resultMsg': errorMessage(errorCodes.BadRequest, `Unable to parse input data. ${err.message}`) });
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
