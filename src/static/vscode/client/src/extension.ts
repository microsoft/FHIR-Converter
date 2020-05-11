/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */

import * as path from 'path';
import * as vscode from 'vscode';

import { LanguageClient } from 'vscode-languageclient';

import { setUpScrollSync } from './scroll-manager';
import { generateLanguageClient } from './language-client';

var XMLHttpRequest = require('xmlhttprequest').XMLHttpRequest;

let client: LanguageClient;
var apiKey: string;
var serviceName: string;

interface FhirConversion {
    templatePath: vscode.Uri,
    templateDoc: vscode.TextDocument,
    messagePath: vscode.Uri,
    messageDoc: vscode.TextDocument,
    panel: vscode.WebviewPanel
}
const openFhirConversions: FhirConversion[] = [];

export function activate(context: vscode.ExtensionContext) {
	let disposable = vscode.commands.registerCommand('dc_extension.dataConversion', async () => {
        const config = vscode.workspace.getConfiguration('fhirConverter');

        serviceName = config.get('serverName');
		while(!serviceName){
			await vscode.window.showInputBox({placeHolder: "Your service's name"}).then((input) => {
				if (input !== undefined) {
                    serviceName = input;    
                    config.update('serverName', serviceName, true);
				}
				else {
					vscode.window.showErrorMessage("Name missing.");
				}
			});
		}
		
		apiKey = config.get('apiKey');
		while(!apiKey) {
      	  await vscode.window.showInputBox({placeHolder: "Your API key"}).then((input) => {
				if (input !== undefined) {
                    apiKey = input;
                    config.update('apiKey', apiKey, true);
				}
				else {
					vscode.window.showErrorMessage("API key missing.");
				}
			});
        }
        
        let templateFolder = config.get('templateFolder');
		while(!templateFolder) {
      	  await vscode.window.showInputBox({placeHolder: "Path of the template folder"}).then((input) => {
				if (input !== undefined) {
                    templateFolder = input;
                    config.update('templateFolder', templateFolder, true);
				}
				else {
					vscode.window.showErrorMessage("Template folder missing.");
				}
			});
        }
        
        let messageFolder = config.get('messageFolder');
		while(!messageFolder) {
      	  await vscode.window.showInputBox({placeHolder: "Path of the message folder"}).then((input) => {
				if (input !== undefined) {
					messageFolder = input;
                    config.update('messageFolder', messageFolder, true);
				}
				else {
					vscode.window.showErrorMessage("Message folder missing.");
				}
			});
        }
        
        let templatePath: vscode.Uri;
        while(!templatePath) {
            //await vscode.window.showInputBox({placeHolder: "Name of the tempalte file"}).then((input) => {
            await vscode.window.showOpenDialog({canSelectMany: false, defaultUri: vscode.Uri.parse('file:///' + templateFolder, true)}).then((input) => {
                if (input && input.length !== 0) {
                    templatePath = input[0];
                }
                else {
                    vscode.window.showErrorMessage("No template provided.");
                }
            });
        }

        let messagePath: vscode.Uri;
        while(!messagePath) {
            //await vscode.window.showInputBox({placeHolder: "Name of the message file"}).then((input) => {
            await vscode.window.showOpenDialog({canSelectMany: false, defaultUri: vscode.Uri.parse('file:///' + messageFolder, true)}).then((input) => {
                if (input && input.length !== 0) {
                    messagePath = input[0];
                }
                else {
                    vscode.window.showErrorMessage("No message provided.");
                }
            });
        }

        const templateDoc = (await vscode.window.showTextDocument(templatePath, {
            viewColumn: vscode.ViewColumn.One
        })).document;

        const messageDoc = (await vscode.window.showTextDocument(messagePath, {
            viewColumn: vscode.ViewColumn.Two
        })).document;

        const messageName = path.basename(messagePath.path);
        const templateName = path.basename(templatePath.path);
		const panel = vscode.window.createWebviewPanel (
			'fhirOutput_' + messageName + '_' + templateName,
			messageName + ': ' + templateName,
			vscode.ViewColumn.Three,
			{
				enableScripts: true,
				localResourceRoots: [
					vscode.Uri.file(path.join(context.extensionPath, 'src'))    
				]
			}
        );   
        
        openFhirConversions.push({templatePath, templateDoc, messagePath, messageDoc, panel});
        readFileToConvert(panel, messageDoc.getText(), templateDoc.getText(), apiKey);
       
        setUpScrollSync(panel.webview)

        context.subscriptions.push(disposable);
    });
    
    vscode.workspace.onDidChangeTextDocument((e: vscode.TextDocumentChangeEvent) => {
        const conversions = openFhirConversions.filter(conversion => conversion.messagePath.path.toLowerCase() === e.document.uri.path.toLowerCase() || conversion.templatePath.path.toLowerCase() === e.document.uri.path.toLowerCase());
        conversions.forEach(conversion => {
            readFileToConvert(conversion.panel, conversion.messageDoc.getText(), conversion.templateDoc.getText(), apiKey);
        });
    });

    client = generateLanguageClient(context);
	// Start the client. This will also launch the server
	client.start();
}

export function deactivate(): Thenable<void> | undefined {
    // Stops the language client if it was created
	if (!client) {
		return undefined;
	}
	return client.stop();
}

function readFileToConvert(webViewPanel: vscode.WebviewPanel, messageText: string, templateText: string, apiKey: string) {
    var sendJson = {
        templateBase64: "",
        messageBase64: ""
    };
    //sendJson.messageBase64 = Buffer.from(fs.readFileSync(inputName)).toString('base64');
    //sendJson.templateBase64 = Buffer.from(fs.readFileSync(templateName)).toString('base64');
    sendJson.messageBase64 = Buffer.from(messageText).toString('base64');
    sendJson.templateBase64 = Buffer.from(templateText).toString('base64');

    var request = new XMLHttpRequest();
    request.open('POST', `https://${serviceName}.azurewebsites.net/api/convert/hl7`, true);
    request.setRequestHeader("Content-Type", "application/json");
    request.setRequestHeader("X-MS-CONVERSION-API-KEY", apiKey);
    request.onreadystatechange = function () {
        if (request.readyState >= 3 && request.status >= 200) {
            let outputmsg = request.responseText.replace(/\\/g, "");
            outputmsg = JSON.parse(outputmsg);
            outputmsg = JSON.stringify(outputmsg, undefined, 2);
            webViewPanel.webview.html = getWebviewContent(outputmsg);
        }
        else {
            //console.log('request not working:\r\n' + JSON.stringify(request));
        }
    };
	request.send(JSON.stringify(sendJson));
}

function getWebviewContent(outputmsg: string) {
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
