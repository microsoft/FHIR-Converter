# FHIR Converter VS Code Extension Proof of Concept 

This extension is a POC for making a VS Code extension for the FHIR Converter.

## Features and Limitations
The following features from the web editor are supported by the POC VS Code extension:
- Converting Hl7 messages into FHIR resources
- Editing templates and message (locally only)
- Jumping to partial template definitions (Ctrl+click)
- Auto completion of partial template names
- Scroll sync (doesn't work great)
   
Features present that aren't in the web editor:
- Check partial templates exist
  
Features that are missing:
- Highlighting of message sections that were not used
- Saving to the server
- Editing multiple templates
- Collapsing sections on the FHIR resource
     
## Running the extension

- Run `npm install` in this folder. This installs all necessary npm modules in both the client and server folder
- Open VS Code on this folder.
- Press Ctrl+Shift+B to compile the client and server.
- Switch to the Debug viewlet.
- Select `Launch Client` from the drop down.
- Run the launch config.
- If you want to debug the server as well use the launch configuration `Attach to Server`
- Additional features will be present when editing handlebars files
  - Auto completion for partial template names
  - Goto file for partial templates (Ctrl+click)
  - Name check for partial templates
- To convert a message/template pair press F1 and run the command 'Convert to FHIR'.
  - A prompt will pop up for the server's name, API key, the top level template folder, and the messages folder. This prompt will not appear if you have previously set these settings.
    - These values can also be set under the extension's settings (File > Preferences > Settings)
  - File selection windows will open to allow the template and message to be selected. 
  - The template, message, and FHIR response should be displayed in new windows.
    - This feature currently only sends the selected template. It does not support editing of multiple templates, or editing of partial templates

## Future improvement

- Launch VS Code from a hyperlink: https://stackoverflow.com/questions/56471739/html-link-starting-with-vscode-to-open-a-file-in-visual-studio-code
- Display the HL7 message in a tree view.
  - A custom webview could do this and allow us to highlight the unused sections
