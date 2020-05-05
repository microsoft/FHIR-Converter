# FHIR Converter VS Code Extension Proof of Concept 

This extension is a POC for making a VS Code extension for the FHIR Converter.

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
    - These features currently treats partial template names as relative paths to the template file, not relative to the top level template folder.
- To convert a message/template pair press F1 and run the command 'Convert to FHIR'.
  - A prompt will pop up for the server's name and API key
    - These values can also be set under the extension's settings (File > Preferences > Settings)
  - Open a template file (.hbs)
  - Open a message file (.hl7)
  - The response should be displayed in the 'FHIR Resource' window
    - This feature currently only sends the last selected template. It does not support editing of multiple templates, or editing of partial templates
