// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

if(process.env.APPINSIGHTS_INSTRUMENTATIONKEY) { 
    const appInsights = require('applicationinsights');
    appInsights.setup().start(); // assuming ikey is in env var APPINSIGHTS_INSTRUMENTATIONKEY
}

var express = require('express');
var app = require('./routes')(express());

var port = process.env.PORT || 2019;

var server = app.listen(port, function () {
    var host = server.address().address;
    console.log("HealthConverter listening at http://%s:%s", host, port);
});