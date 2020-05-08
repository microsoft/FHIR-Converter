/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */

import {
	createConnection,
	TextDocuments,
	Diagnostic,
	DiagnosticSeverity,
	ProposedFeatures,
	InitializeParams,
	CompletionItem,
	CompletionItemKind,
	TextDocumentPositionParams,
	TextDocumentSyncKind,
	InitializeResult,
	DefinitionParams,
	DefinitionLink
} from 'vscode-languageserver';

import {
	TextDocument
} from 'vscode-languageserver-textdocument';
import * as path from 'path';
import * as glob from 'glob';
import { SettingsManager } from './settings';

// Create a connection for the server. The connection uses Node's IPC as a transport.
// Also include all preview / proposed LSP features.
let connection = createConnection(ProposedFeatures.all);

// Create a simple text document manager. The text document manager
// supports full document sync only
let documents: TextDocuments<TextDocument> = new TextDocuments(TextDocument);

let settingsManager: SettingsManager;

connection.onInitialize((params: InitializeParams) => {
	let capabilities = params.capabilities;

	settingsManager = new SettingsManager(connection, capabilities, validateTextDocument);

	const hasWorkspaceFolderCapability = !!(
		capabilities.workspace && !!capabilities.workspace.workspaceFolders
	);

	const result: InitializeResult = {
		capabilities: {
			textDocumentSync: TextDocumentSyncKind.Full,
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

async function validateTextDocument(textDocument: TextDocument): Promise<void> {
	// The validator creates diagnostics for all uppercase words length 2 and more
	let text = textDocument.getText();
	const pattern = /\{\{\>([^\s\}]*)/g;
	let m: RegExpExecArray | null;

	const templateFolder = (await settingsManager.getDocumentSettings(textDocument.uri)).templateFolder
	const templates = await getAllTemplatePaths(templateFolder);

	let diagnostics: Diagnostic[] = [];
	while ((m = pattern.exec(text))) {
		if (!templates.some(uri => uri === (m as RegExpExecArray)[1])) {
			let diagnostic: Diagnostic = {
				severity: DiagnosticSeverity.Error,
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
}

// This handler provides the initial list of the completion items.
connection.onCompletion(
	async (_textDocumentPosition: TextDocumentPositionParams): Promise<CompletionItem[]> => {
		const partialFilePath = getSuroundingText(_textDocumentPosition);

		// This isn't working...
		//if (partialFilePath.startsWith('{{>')) {
			const templates = await getAllTemplatePaths((await settingsManager.getDocumentSettings(_textDocumentPosition.textDocument.uri)).templateFolder);
			return templates.map((path, index) => {
				return {
					label: path,
					kind: CompletionItemKind.Text,
					data: index
				}
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
	}
);

// This handler resolves additional information for the item selected in
// the completion list.
connection.onCompletionResolve(
	(item: CompletionItem): CompletionItem => {
		return item;
	}
);

connection.onDefinition(async (params: DefinitionParams): Promise<DefinitionLink[]> => {
	const contextString = getSuroundingText(params);
	if (contextString === '') {
		return [];
	}

	const pattern = /\{\{\>([^\s\}]*)/g;
	let match: RegExpExecArray | null;
	if ((match = pattern.exec(contextString)) !== null) {
		const relativeFilePath = match[1];
		const templateFolder = (await settingsManager.getDocumentSettings(params.textDocument.uri)).templateFolder
		if (getAllTemplatePaths(templateFolder).some(uri => uri === relativeFilePath)) {
			const fileUri = 'file:///' + templateFolder + '/' + relativeFilePath;
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
			]
		}
	}

	return [];
});

// This does not currently work properly as it only finds templates that are in the same folder or below the current template.
function getAllTemplatePaths(directory: string): string[] {
	/*
	// Strips off the file:/// at the start of the uri
	fileUri = fileUri.substring(8).replace('%3A',':'); 

	const directory = path.dirname(fileUri);
	*/
	const searchPattern = directory + '/**/*.hbs';
	const files: string[] = glob.sync(searchPattern, {}).map(uri => path.relative(directory, uri).replace(/\\/g,'/'));

	return files;
}

function getSuroundingText(location: TextDocumentPositionParams): string {
	const start = {
		line: location.position.line,
		character: 0
	}
	const end = {
		line: location.position.line + 1,
		character: 0
	} 

	const targetLine = documents.get(location.textDocument.uri)?.getText({start, end}).trimRight();
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
