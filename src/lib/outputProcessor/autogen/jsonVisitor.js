// Generated from C:\src2\conversion-pilot\src\lib\outputProcessor\json.g4 by ANTLR 4.7.2
// jshint ignore: start
var antlr4 = require('antlr4/index');

// This class defines a complete generic visitor for a parse tree produced by jsonParser.

function jsonVisitor() {
	antlr4.tree.ParseTreeVisitor.call(this);
	return this;
}

jsonVisitor.prototype = Object.create(antlr4.tree.ParseTreeVisitor.prototype);
jsonVisitor.prototype.constructor = jsonVisitor;

// Visit a parse tree produced by jsonParser#json.
jsonVisitor.prototype.visitJson = function(ctx) {
  return this.visitChildren(ctx);
};


// Visit a parse tree produced by jsonParser#obj.
jsonVisitor.prototype.visitObj = function(ctx) {
  return this.visitChildren(ctx);
};


// Visit a parse tree produced by jsonParser#pair.
jsonVisitor.prototype.visitPair = function(ctx) {
  return this.visitChildren(ctx);
};


// Visit a parse tree produced by jsonParser#array.
jsonVisitor.prototype.visitArray = function(ctx) {
  return this.visitChildren(ctx);
};


// Visit a parse tree produced by jsonParser#value.
jsonVisitor.prototype.visitValue = function(ctx) {
  return this.visitChildren(ctx);
};



exports.jsonVisitor = jsonVisitor;