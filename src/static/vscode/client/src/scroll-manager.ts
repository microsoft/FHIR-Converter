import * as vscode from 'vscode';

export function setUpScrollSync(webview: vscode.Webview) {
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
			webview.postMessage(midLine);
			webview.postMessage({command: 'scroll'});
		  }
	});

	webview.onDidReceiveMessage (message => {
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