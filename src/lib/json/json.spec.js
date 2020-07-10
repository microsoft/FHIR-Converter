// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

var assert = require('assert');
var json = require('./json');

describe('json', function () {
    it('should preprocess template correctly.', function () {
        var result = new json().preProcessTemplate('{{PID-2}}');
        assert.equal(result, '{{PID-2}}');
    });

    it('should postprocess result correctly.', function () {
        var result = new json().postProcessResult('{"a":"b",,,,}');
        assert.equal(JSON.stringify(result), JSON.stringify({ 'a': 'b' }));
    });

    it('should generate conversion metadata correctly.', function () {
        let data = 'dummy';
        let result = new json().getConversionResultMetadata(data);
        assert.equal(JSON.stringify({}), JSON.stringify(result));
    });

    it('should successfully parse correct data.', function (done) {
        let data = '{ "a":"b"}';
        new json().parseSrcData(data)
            .then((result) => {
                if (JSON.stringify(result) === JSON.stringify(JSON.parse(data)))
                {
                    done();
                }
                else {
                    done(new Error(`_originalData doesn't match with data`));
                }
            })
            .catch((err) => done(err));
    });

    it('should fail while parsing incorrect data.', function (done) {
        new json().parseSrcData('{ "a":"b"')
            .then(() => done(new Error(`parseSrcData should have failed!`)))
            .catch(() => done());
    });
});
