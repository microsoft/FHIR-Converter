/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */

import * as path from 'path';
import * as vscode from 'vscode';

import {
	LanguageClient,
	LanguageClientOptions,
	ServerOptions,
	TransportKind
} from 'vscode-languageclient';

import * as fs from 'fs';

var XMLHttpRequest = require('xmlhttprequest').XMLHttpRequest;

let client: LanguageClient;
var outputmsg = "outputmsg";
var linePosition: number | undefined;
var libraries = {};
var apiKey: string;
var serviceName: string;
var panel: vscode.WebviewPanel;

export function activate(context: vscode.ExtensionContext) {
	let disposable = vscode.commands.registerCommand('dc_extension.dataConversion', async () => {
        vscode.window.showInformationMessage('data conversion starting...');
        libraries = {
            'prismJSuri': vscode.Uri.file(path.join(context.extensionPath, 'src', 'prism.js')).with({scheme:'vscode-resource'}),
            'prismCSSuri': vscode.Uri.file(path.join(context.extensionPath, 'src', 'prism.css')).with({scheme:'vscode-resource'})
        };
        var inputName = "";
		var templateName = "";
		
		serviceName = vscode.workspace.getConfiguration('fhirConverter').get('serverName');
		if(!serviceName){
			await vscode.window.showInputBox({placeHolder: "Your service's name"}).then((input) => {
				if (input !== undefined) {
					serviceName = input;    
				}
				else {
					vscode.window.showErrorMessage("Name missing.");
				}
			});
		}
		
		apiKey = vscode.workspace.getConfiguration('fhirConverter').get('apiKey');
		if(!apiKey) {
      	  await vscode.window.showInputBox({placeHolder: "Your API key"}).then((input) => {
				if (input !== undefined) {
					apiKey = input;
				}
				else {
					vscode.window.showErrorMessage("API key missing. Authentication failed.");
				}
			});
		}

		console.log(serviceName + " " + apiKey);

		panel = vscode.window.createWebviewPanel (
			'fhirOutputWindow',
			'FHIR Resource',
			vscode.ViewColumn.Two,
			{
				enableScripts: true,
				localResourceRoots: [
					vscode.Uri.file(path.join(context.extensionPath, 'src'))    
				]
			}
		);     

        vscode.window.onDidChangeActiveTextEditor((textEditor) => {
			console.log("Changed text editor");
            if (!textEditor) {
                return;
            }
            if (textEditor.document.fileName.endsWith(".hl7")) {
                inputName = textEditor.document.fileName;
            }
            if (textEditor.document.fileName.endsWith(".hbs")) {
                templateName = textEditor.document.fileName;
            }
            if (inputName.endsWith("hl7") && templateName.endsWith("hbs")) {
                readFiletoConvert(panel, inputName, templateName, apiKey);
            } 
        });
        
        vscode.workspace.onDidChangeTextDocument((e) => {
            vscode.workspace.saveAll();
            readFiletoConvert(panel, inputName, templateName, apiKey);
        });
       
        vscode.window.onDidChangeTextEditorVisibleRanges((event) => {
            if (!event.textEditor.visibleRanges.length) {
                return undefined;
              } else {
                const topLine = getTopVisibleLine(event.textEditor);
                const bottomLine = getBottomVisibleLine(event.textEditor);
                let midLine;
                if (topLine === 0) {
                  midLine = 0;
                } else if (
                  Math.floor(bottomLine) ===
                  event.textEditor.document.lineCount - 1
                ) {
                  midLine = bottomLine;
                } else {
                  midLine = Math.floor((topLine + bottomLine) / 2);
                }
                linePosition = midLine;
                panel.webview.postMessage(linePosition);
                panel.webview.postMessage({command: 'scroll'});
              }
        });

        panel.webview.onDidReceiveMessage (message => {
            switch(message.command) {
                case 'editor-scroll':
                    var start = Math.round(message.pos/8);
                    vscode.window.visibleTextEditors.filter(
                        (editor) => editor.document.fileName.endsWith('hbs')
                    ).forEach((editor) => editor.revealRange (
                        new vscode.Range(start, 0, start, 0),
                            vscode.TextEditorRevealType.InCenter
                    ));   
            }
		});
        context.subscriptions.push(disposable);
	});
	
	// The server is implemented in node
	let serverModule = context.asAbsolutePath(
		path.join('server', 'out', 'server.js')
	);
	// The debug options for the server
	// --inspect=6009: runs the server in Node's Inspector mode so VS Code can attach to the server for debugging
	let debugOptions = { execArgv: ['--nolazy', '--inspect=6009'] };

	// If the extension is launched in debug mode then the debug server options are used
	// Otherwise the run options are used
	let serverOptions: ServerOptions = {
		run: { module: serverModule, transport: TransportKind.ipc },
		debug: {
			module: serverModule,
			transport: TransportKind.ipc,
			options: debugOptions
		}
	};

	// Options to control the language client
	let clientOptions: LanguageClientOptions = {
		// Register the server for plain text documents
		documentSelector: [{ scheme: 'file', language: 'handlebars' }],
	};

	// Create the language client and start the client.
	client = new LanguageClient(
		'languageServerExample',
		'Language Server Example',
		serverOptions,
		clientOptions
	);

	// Start the client. This will also launch the server
	client.start();
}

export function deactivate(): Thenable<void> | undefined {
	if (!client) {
		return undefined;
	}
	return client.stop();
}

function getTopVisibleLine (editor: vscode.TextEditor) {
    const firstVisiblePosition = editor["visibleRanges"][0].start;
    const lineNumber = firstVisiblePosition.line;
    const line = editor.document.lineAt(lineNumber);
    const progress = firstVisiblePosition.character / (line.text.length + 2);
    return lineNumber + progress;
}

function getBottomVisibleLine (editor: vscode.TextEditor) {
    if (!editor["visibleRanges"].length) {
      return undefined;
    }
    const firstVisiblePosition = editor["visibleRanges"][0].end;
    const lineNumber = firstVisiblePosition.line;
    let text = "";
    if (lineNumber < editor.document.lineCount) {
      text = editor.document.lineAt(lineNumber).text;
    }
    const progress = firstVisiblePosition.character / (text.length + 2);
    return lineNumber + progress;
}

function readFiletoConvert(webViewPanel: any, inputName: any, templateName: any, apiKey: string) {
    var sendJson = {
        templateBase64: "",
        messageBase64: ""
    };
    sendJson.messageBase64 = Buffer.from(fs.readFileSync(inputName)).toString('base64');
    sendJson.templateBase64 = Buffer.from(fs.readFileSync(templateName)).toString('base64');

    var request = new XMLHttpRequest();
    request.open('POST', `https://${serviceName}.azurewebsites.net/api/convert/hl7`, true);
    request.setRequestHeader("Content-Type", "application/json");
    request.setRequestHeader("X-MS-CONVERSION-API-KEY", apiKey);
    request.onreadystatechange = function () {
        if (request.readyState >= 3 && request.status >= 200) {
            outputmsg = request.responseText.replace(/\\/g, "");
            outputmsg = JSON.parse(outputmsg);
            outputmsg = JSON.stringify(outputmsg, undefined, 2);
            webViewPanel.webview.html = getWebviewContent(libraries);
        }
        else {
            //console.log('request not working:\r\n' + JSON.stringify(request));
        }
    };
	request.send(JSON.stringify(sendJson));
}

function getWebviewContent(libraries: any) {
    return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">   
    <title>Data Conversion Extension</title>
</head>
<body>
    <script>
    const vscode = acquireVsCodeApi();
    var scrollPos = 0;
        window.addEventListener('message', event => {
            const message = event.data;
            
            if (typeof event.data === "number") {
                scrollPos = event.data;
            }
            switch (message.command) {
                case 'scroll':
                    window.scrollTo(60, scrollPos*8);
            }
        });
        document.onscroll = function() {scrollfunc()};
        function scrollfunc() {
            vscode.postMessage({
                command: 'editor-scroll',
                pos: document.getElementsByTagName("html")[0].scrollTop
            })
        }
    </script>
    <pre>
        <code class="language-json">
${outputmsg}
        </code>
    </pre> 
</body>
</html>`;
}
