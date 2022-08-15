const fs = require("fs");
const WorkerPool = require("./lib/workers/workerPool");

function createAndUploadFhirResource()  {
    if(process.env.npm_config_myVar === '' || process.env.npm_config_myVar === null || process.env.npm_config_myVar === undefined) {
        console.log("Must specify file path");
    } else {
        const xmlFile = fs.readFileSync(process.env.npm_config_myVar, 'utf8');
        const workerPool = new WorkerPool('./src/lib/workers/worker.js', require('os').cpus().length);
        return workerPool.exec({
            'type': '/api/convert/:srcDataType/:template',
            'srcData': xmlFile.toString(),
            'srcDataType': "cda",
            'templateName': "ccd.hbs"
        }).then((result) => {
            const resultMessage = result.resultMsg;
            let newPath = process.env.npm_config_myVar.slice(0, -4);
            newPath += "--FHIR-RESOURCE.json";
            fs.writeFileSync(newPath, JSON.stringify(resultMessage));
        }).then(() => {
            workerPool.destroy();
        });
    }
}

createAndUploadFhirResource();