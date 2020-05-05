"use strict";
/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const vscode_languageserver_1 = require("vscode-languageserver");
const vscode_languageserver_textdocument_1 = require("vscode-languageserver-textdocument");
const path = require("path");
const glob = require("glob");
// Create a connection for the server. The connection uses Node's IPC as a transport.
// Also include all preview / proposed LSP features.
let connection = vscode_languageserver_1.createConnection(vscode_languageserver_1.ProposedFeatures.all);
// Create a simple text document manager. The text document manager
// supports full document sync only
let documents = new vscode_languageserver_1.TextDocuments(vscode_languageserver_textdocument_1.TextDocument);
let hasConfigurationCapability = false;
let hasWorkspaceFolderCapability = false;
let hasDiagnosticRelatedInformationCapability = false;
connection.onInitialize((params) => {
    let capabilities = params.capabilities;
    // Does the client support the `workspace/configuration` request?
    // If not, we will fall back using global settings
    hasConfigurationCapability = !!(capabilities.workspace && !!capabilities.workspace.configuration);
    hasWorkspaceFolderCapability = !!(capabilities.workspace && !!capabilities.workspace.workspaceFolders);
    hasDiagnosticRelatedInformationCapability = !!(capabilities.textDocument &&
        capabilities.textDocument.publishDiagnostics &&
        capabilities.textDocument.publishDiagnostics.relatedInformation);
    const result = {
        capabilities: {
            textDocumentSync: vscode_languageserver_1.TextDocumentSyncKind.Full,
            // Tell the client that the server supports code completion
            completionProvider: {
                resolveProvider: true
            },
            definitionProvider: true
        }
    };
    if (hasWorkspaceFolderCapability) {
        result.capabilities.workspace = {
            workspaceFolders: {
                supported: true
            }
        };
    }
    return result;
});
// The content of a text document has changed. This event is emitted
// when the text document first opened or when its content has changed.
documents.onDidChangeContent(change => {
    validateTextDocument(change.document);
});
function validateTextDocument(textDocument) {
    return __awaiter(this, void 0, void 0, function* () {
        // The validator creates diagnostics for all uppercase words length 2 and more
        let text = textDocument.getText();
        const pattern = /\{\{\>([^\s\}]*)/g;
        let m;
        const templates = yield getAllTemplatePaths(textDocument.uri);
        let diagnostics = [];
        while ((m = pattern.exec(text))) {
            if (!templates.some(uri => uri === m[1])) {
                let diagnostic = {
                    severity: vscode_languageserver_1.DiagnosticSeverity.Error,
                    range: {
                        start: textDocument.positionAt(m.index),
                        end: textDocument.positionAt(m.index + m[0].length)
                    },
                    message: `${m[1]} is not a valid partial template.`,
                    source: 'ex'
                };
                diagnostics.push(diagnostic);
            }
        }
        // Send the computed diagnostics to VSCode.
        connection.sendDiagnostics({ uri: textDocument.uri, diagnostics });
    });
}
// This handler provides the initial list of the completion items.
connection.onCompletion((_textDocumentPosition) => __awaiter(void 0, void 0, void 0, function* () {
    const partialFilePath = getSuroundingText(_textDocumentPosition);
    // This isn't working...
    //if (partialFilePath.startsWith('{{>')) {
    const templates = yield getAllTemplatePaths(_textDocumentPosition.textDocument.uri);
    return templates.map((path, index) => {
        return {
            label: path,
            kind: vscode_languageserver_1.CompletionItemKind.Text,
            data: index
        };
    });
    /*
    }
    else if (partialFilePath.startsWith('{{#') || partialFilePath.startsWith('{{/')) {
        return [
            {
                label: 'if',
                kind: CompletionItemKind.Text,
                data: 0
            },
            {
                label: 'else',
                kind: CompletionItemKind.Text,
                data: 1
            },
            {
                label: 'with',
                kind: CompletionItemKind.Text,
                data: 2
            },
            {
                label: 'each',
                kind: CompletionItemKind.Text,
                data: 3
            }
        ]
    }
    return [];
    */
}));
// This handler resolves additional information for the item selected in
// the completion list.
connection.onCompletionResolve((item) => {
    return item;
});
connection.onDefinition((params) => {
    const contextString = getSuroundingText(params);
    if (contextString === '') {
        return [];
    }
    const pattern = /\{\{\>([^\s\}]*)/g;
    let match;
    if ((match = pattern.exec(contextString)) !== null) {
        const relativeFilePath = match[1];
        if (getAllTemplatePaths(params.textDocument.uri).some(uri => uri === relativeFilePath)) {
            const fileUri = path.dirname(params.textDocument.uri) + '/' + relativeFilePath;
            const firstChar = {
                start: {
                    line: 0,
                    character: 0
                },
                end: {
                    line: 0,
                    character: 1
                }
            };
            return [
                {
                    targetUri: fileUri,
                    targetRange: firstChar,
                    targetSelectionRange: firstChar
                }
            ];
        }
    }
    return [];
});
// This does not currently work properly as it only finds templates that are in the same folder or below the current template.
function getAllTemplatePaths(fileUri) {
    // Strips off the file:/// at the start of the uri
    fileUri = fileUri.substring(8).replace('%3A', ':');
    const directory = path.dirname(fileUri);
    const searchPattern = directory + '/**/*.hbs';
    const files = glob.sync(searchPattern, {}).map(uri => path.relative(directory, uri).replace(/\\/g, '/'));
    return files;
}
function getSuroundingText(location) {
    var _a;
    const start = {
        line: location.position.line,
        character: 0
    };
    const end = {
        line: location.position.line + 1,
        character: 0
    };
    const targetLine = (_a = documents.get(location.textDocument.uri)) === null || _a === void 0 ? void 0 : _a.getText({ start, end }).trimRight();
    if (!targetLine) {
        return '';
    }
    let endIndex = targetLine.indexOf(' ', location.position.character);
    if (endIndex === -1) {
        endIndex = targetLine.length;
    }
    let startIndex = targetLine.lastIndexOf(' ', location.position.character);
    if (startIndex === -1) {
        startIndex = 0;
    }
    return targetLine.substring(startIndex, endIndex);
}
// Make the text document manager listen on the connection
// for open, change and close text document events
documents.listen(connection);
// Listen on the connection
connection.listen();
//# sourceMappingURL=server.js.map