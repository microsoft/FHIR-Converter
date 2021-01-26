// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------
const Promise = require('promise');
const fs = require('fs');
const path = require('path');
const directory = path.join(__dirname, '../parsers/');
var map = [];

module.exports = class dataHandlerFactory {

    static createDataHandler(dataType) {

        let parsersList = this.listParsers(directory);
        parsersList.forEach(function (file) {
            // since loader has same name as directory
            let parser = require(directory + file + "/" + file);

            map[file] = new parser();
        });

        return map[dataType];
    }

    static listParsers(location) {
        let parsers = [];
        fs.readdirSync(location).forEach(file => {
            if (fs.lstatSync(path.resolve(location, file)).isDirectory()) {
                parsers.push(file);
            }
        });

        return parsers;
    }
};




