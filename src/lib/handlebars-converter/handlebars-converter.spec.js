// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

var assert = require('assert');
var constants = require('../constants/constants');
var fs = require('fs');
var hl7 = require('../hl7v2/hl7v2');
var path = require('path');
var HandlebarsConverter = require('./handlebars-converter');
var jsonProcessor = require('../outputProcessor/jsonProcessor');
var templatePreprocessor = require('../inputProcessor/templatePreprocessor');

describe('HandlebarsConverter', function () {
    var handlerbarInstance = HandlebarsConverter.instance(true, path.join(__dirname, '../../templates'));
    var handlerbarExistingInstance = HandlebarsConverter.instance(false, path.join(__dirname, '../../templates'));

    var templatesMap = {};
    templatesMap['Resources/Patient.hbs'] = '{"key1":"value1"}';
    templatesMap['Resources/Encounter.hbs'] = '{"key2":"value2"}';
    var handlerbarInstanceWithTemplatesMap = HandlebarsConverter.instance(true, path.join(__dirname, '../../templates'), templatesMap);

    it('should compile the ADT_A01.hbs template, which includes nested partials (using handlebar new insatnce)', function (done) {
        fs.readFile(path.join(constants.TEMPLATE_FILES_LOCATION, 'ADT_A01.hbs'), function(tErr, templateContent) {
            if (tErr) done(tErr);
            fs.readFile(path.join(constants.SAMPLE_DATA_LOCATION, 'ADT01-23.hl7'), function(mErr, messageContent) {
                if (mErr) done(mErr);

                var template;
                try {
                    template = handlerbarInstance.compile(templatePreprocessor.Process(templateContent.toString()));
                } catch(ex) {
                    done(ex);
                }

                var context = { msg: hl7.parseHL7v2(messageContent.toString()) };

                try {
                    JSON.parse(jsonProcessor.Process(template(context)));
                    done();
                } catch (ex) {
                    done(ex);
                }
            });
        });
    });

    it('should compile the ADT_A01.hbs template, which includes nested partials (using handlebar existing instance)', function (done) {
        fs.readFile(path.join(constants.TEMPLATE_FILES_LOCATION, 'ADT_A01.hbs'), function(tErr, templateContent) {
            if (tErr) done(tErr);
            fs.readFile(path.join(constants.SAMPLE_DATA_LOCATION, 'ADT01-23.hl7'), function(mErr, messageContent) {
                if (mErr) done(mErr);

                var template;
                try {
                    template = handlerbarExistingInstance.compile(templatePreprocessor.Process(templateContent.toString()));
                    var context = { msg: hl7.parseHL7v2(messageContent.toString()) };
                    JSON.parse(jsonProcessor.Process(template(context)));
                    done();
                } catch(ex) {
                    done(ex);
                }
            });
        });
    });

    it('should compile the ADT_A01.hbs template, which includes nested partials (using templatesMap)', function (done) {
        fs.readFile(path.join(constants.TEMPLATE_FILES_LOCATION, 'ADT_A01.hbs'), function(tErr, templateContent) {
            if (tErr) done(tErr);
            fs.readFile(path.join(constants.SAMPLE_DATA_LOCATION, 'ADT01-23.hl7'), function(mErr, messageContent) {
                if (mErr) done(mErr);

                var template;
                try {
                    template = handlerbarInstanceWithTemplatesMap.compile(templatePreprocessor.Process(templateContent.toString()));
                    var context = { msg: hl7.parseHL7v2(messageContent.toString()) };
                    var conversionOutput = template(context);
                    if (conversionOutput.includes("key1") && conversionOutput.includes("key2")) {
                        done();
                    }
                } catch(ex) {
                    done(ex);
                }
            });
        });
    });

    it('should throw error when referencing partial that is not cached or found on disk', function() {
        var context = { msg: hl7.parseHL7v2('MSH|^~\\&|AccMgr|1|||20050110045504||ADT^A01|599102|P|2.3|||\nEVN|A01|20050110045502|||||') };
        var template = handlerbarInstance.compile('{{>nonExistingTemplate.hbs}}');
        assert.throws( () => { JSON.parse(template(context)); }, Error, 'Referenced partial template not found on disk');
    });
});