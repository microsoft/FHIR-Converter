// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

var assert = require('assert');
var utils = require('./testUtils');

describe('Regression test testUtils - getGroundTruthFileName', () => {
    it ('should generate normal ground truth file name', () => {
        const testCase = { dataFile: 'sample.cda', templateFile: 'ccd.hbs' };
        const fileName = utils.getGroundTruthFileName(testCase);
        assert.strictEqual(fileName, 'ccd.hbs-sample.cda.json');
    });
    it ('should throw appropriate error when encountering invalid input.', () => {
        const testCases = [
            null,
            undefined,
            true,
            'invalid-input',
            [],
            [ 'sample.cda', 'ccd.hbs' ],
            {},
            { dataFile_invalid: 'sample.cda', templateFile_invalid: 'ccd.hbs' }
        ];
        const expectError = {
            name: 'Error',
            message: 'The testCase should both have property [templateFile] and [dataFile].'
        };
        for (const testCase of testCases) {
            assert.throws(() => utils.getGroundTruthFileName(testCase), expectError);
        }
    });
});

describe('Regression test testUtils - compareContent', () => {
});