/* eslint-disable no-undef */
// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

var jsonListener = require('./autogen/jsonListener').jsonListener;

jsonCustomListener = function() {
    this.stack = [];    
    jsonListener.call(this); // inherit default listener
    return this;
};

// inherit default listener
jsonCustomListener.prototype = Object.create(jsonListener.prototype);
jsonCustomListener.prototype.constructor = jsonCustomListener;

jsonCustomListener.prototype.exitJson = function() {
    let childText = this.stack.pop();
    if (childText === undefined) {
        throw "unexpected state!";
    }
    this.stack.push(childText ? childText : '{}'); // top object
};

jsonCustomListener.prototype.exitObj = function(ctx) {
    //console.log('obj');
    //this.dumpCtx(ctx);

    var pairArr = [];
    for (var i = 0; i < ctx.getChildCount(); ++i) {
        if (ctx.getChild(i).getChildCount() == 3) {
            let pairText = this.stack.pop();
            if (pairText) {
                pairArr.push(pairText);
            }
        }
    }
    let finalText = pairArr.reverse().join();
    this.stack.push(finalText ? `{${finalText}}` : null);
    //this.printState();
};

jsonCustomListener.prototype.exitArray = function(ctx) {
    //console.log('array');
    //this.dumpCtx(ctx);

    var valueArr = [];
    for (var i = 0; i < ctx.getChildCount(); ++i) {
        if (ctx.getChild(i).getChildCount() > 0) {
            let valText = this.stack.pop();
            if (valText) {
                valueArr.push(valText);
            }
        }
    }
    let finalText = valueArr.reverse().join();
    this.stack.push(finalText ? `[${finalText}]` : null);
    //this.printState();
};

jsonCustomListener.prototype.exitPair = function(ctx) {
    //console.log('pair');
    //this.dumpCtx(ctx);

    if (ctx.getChildCount() == 3) {
        let valueText = this.stack.pop();
        this.stack.push(valueText ? `${ctx.getChild(0).getText()}:${valueText}` : null);
    }
    //this.printState();
};

jsonCustomListener.prototype.exitValue = function(ctx) {
    //console.log('value');
    //this.dumpCtx(ctx);
    
    if (1 == ctx.getChildCount()) {
        let child = ctx.getChild(0);
        if (child.getChildCount() == 0) {
            let text = child.getText();
            this.stack.push((text.length == 0 || text == '""') ? null : text);
        }
        // else keep child data as it is.
    }

    //this.printState();
};

/*jsonCustomListener.prototype.dumpCtx = function(ctx) {
    console.log(`\t data=${ctx.getText()} childCount=${ctx.getChildCount()}`);
    for (let i = 0; i < ctx.getChildCount(); ++i) {
        console.log(`\t\t data=${ctx.getChild(i).getText()} childCount=${ctx.getChild(i).getChildCount()}`);
    }
};

jsonCustomListener.prototype.printState = function() {
    console.log(`  stack size : ${this.stack.length}`);
    var top = this.stack[this.stack.length-1];
    console.log(`  ${(top ? top : 'undefined/null')}`);
};*/

jsonCustomListener.prototype.getResult = function() {
    return this.stack.pop();
};

exports.jsonCustomListener = jsonCustomListener;