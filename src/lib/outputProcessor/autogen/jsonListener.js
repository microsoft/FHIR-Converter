// Generated from C:\src2\conversion-pilot\src\lib\outputProcessor\json.g4 by ANTLR 4.7.2
// jshint ignore: start
var antlr4 = require('antlr4/index');

// This class defines a complete listener for a parse tree produced by jsonParser.
function jsonListener() {
	antlr4.tree.ParseTreeListener.call(this);
	return this;
}

jsonListener.prototype = Object.create(antlr4.tree.ParseTreeListener.prototype);
jsonListener.prototype.constructor = jsonListener;

// Enter a parse tree produced by jsonParser#json.
jsonListener.prototype.enterJson = function(ctx) {
};

// Exit a parse tree produced by jsonParser#json.
jsonListener.prototype.exitJson = function(ctx) {
};


// Enter a parse tree produced by jsonParser#obj.
jsonListener.prototype.enterObj = function(ctx) {
};

// Exit a parse tree produced by jsonParser#obj.
jsonListener.prototype.exitObj = function(ctx) {
};


// Enter a parse tree produced by jsonParser#pair.
jsonListener.prototype.enterPair = function(ctx) {
};

// Exit a parse tree produced by jsonParser#pair.
jsonListener.prototype.exitPair = function(ctx) {
};


// Enter a parse tree produced by jsonParser#array.
jsonListener.prototype.enterArray = function(ctx) {
};

// Exit a parse tree produced by jsonParser#array.
jsonListener.prototype.exitArray = function(ctx) {
};


// Enter a parse tree produced by jsonParser#value.
jsonListener.prototype.enterValue = function(ctx) {
};

// Exit a parse tree produced by jsonParser#value.
jsonListener.prototype.exitValue = function(ctx) {
};



exports.jsonListener = jsonListener;