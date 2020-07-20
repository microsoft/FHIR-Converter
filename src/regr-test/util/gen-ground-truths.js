// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

const path  = require('path');
const fs = require("fs");
const http = require('http');
const cases = require('../config');
const express = require('express');
const routes = require('../../routes');
const utils = require('./utils');

const API_KEY_HEADER = 'X-MS-CONVERSION-API-KEY';
const API_KEY = '=2&jcNFjtsvp=V97mBcH%T_kU=5SMGm=';
const GEN_SERVER_PORT = 2019;

const initEnvironment = app => {
    app.setValidApiKeys([ API_KEY ]);

    const port = process.env.PORT || GEN_SERVER_PORT;
    const server = app.listen(port);
    return server;
};

const truthsExist = basePath => {
    const cdaPath = path.join(basePath, 'cda');
    const hl7v2Path = path.join(basePath, 'hl7v2');

    const promises = [cdaPath, hl7v2Path].map(subPath => 
        new Promise(fulfill => fs.exists(subPath, fulfill)));
    return Promise.all(promises).then(flags => flags.every(x => x));
};

const convertData = (url, payload) => {
    const options = {
        host: process.env.HOST || 'localhost',
        port: process.env.PORT || GEN_SERVER_PORT,
        path: url,
        method: 'POST',
        headers: {
            [API_KEY_HEADER]: API_KEY,
            'Content-Type': 'application/json'
        }
    };

    return new Promise((fulfill, reject) => {
        const request = http.request(options, response => {
            let result = '';
            response.resume();
            response.on('data', chunk => result += chunk);
            response.on('end', () => fulfill(result));
            response.on('error', reject);
        });

        request.on('error', reject);
        request.write(JSON.stringify(payload));
        request.end();
    });
};

const generateTruths = (basePath, domain, subCases) => {
    const templateBasePath = path.join(__dirname, '../../service-templates', domain);
    const dataBasePath = path.join(__dirname, '../../sample-data', domain);
    const subPath = path.join(basePath, domain);

    return subCases.map(subCase => new Promise((fulfill, reject) => {
        const templateContent = fs.readFileSync(path.join(templateBasePath, subCase.templateFile));
        const dataContent = fs.readFileSync(path.join(dataBasePath, subCase.dataFile));
        const subTemplatePath = path.join(subPath, subCase.templateFile);
        
        if (!fs.existsSync(subTemplatePath)) {
            fs.mkdirSync(subTemplatePath, { recursive: true });
        }

        const payload = {
            templateBase64: Buffer.from(templateContent).toString('base64'),
            srcDataBase64: Buffer.from(dataContent).toString('base64')
        };

        convertData(`/api/convert/${domain}`, payload)
            .then(result => {
                const filePath = path.join(subTemplatePath, utils.getGroundTruthFileName(subCase));
                fs.writeFile(filePath, JSON.stringify(JSON.parse(result), null, 4), 'UTF8', error => {
                    if (error) {
                        return reject(error);
                    }
                    fulfill(filePath);
                });
            })
            .catch(reject);
    }));
};

const main = () => {
    const app = routes(express());
    const server = initEnvironment(app);
    const basePath = path.join(__dirname, '../data');
    const prompt = `
        The truths files are already exist in ${basePath}. 
        Please remove them manually for the normal operation of the program.
    `;
    
    truthsExist(basePath).then(flag => {
        if (flag) {
            console.log(prompt);
            server.close(() => process.exit(0));
            return;
        }
        const cdaPromises = generateTruths(basePath, 'cda', cases.cdaCases);
        const hl7v2Promises = generateTruths(basePath, 'hl7v2', cases.hl7v2Cases);

        const cdaFinalPromise = Promise.all(cdaPromises);
        const hl7v2FinalPromise = Promise.all(hl7v2Promises);

        Promise.all([ cdaFinalPromise, hl7v2FinalPromise ])
            .then(console.log)
            .catch(console.error)
            .finally(() => server.close(() => process.exit(0)));
    });
};

// Be very careful before running this file and make sure that you indeed want
// to generate new ground truths.
// command: `node --experimental-worker .\src\regr-test\util\gen-ground-truths.js`
main();