const fs = require("fs");
const WorkerPool = require("./lib/workers/workerPool");

function createAndUploadFhirResource()  {
    if(process.env.npm_config_filePath === '' || process.env.npm_config_filePath === null || process.env.npm_config_filePath === undefined) {
        console.log("Must specify file path");
    } else {
        let filePath = "../../" + process.env.npm_config_filePath;
        const xmlFile = fs.readFileSync(filePath, 'utf8');
        const workerPool = new WorkerPool('./src/lib/workers/worker.js', require('os').cpus().length);
        return workerPool.exec({
            'type': '/api/convert/:srcDataType/:template',
            'srcData': xmlFile.toString(),
            'srcDataType': "cda",
            'templateName': "ccd.hbs"
        }).then((result) => {
            const resultMessage = result.resultMsg;
            let newPath = filePath.replace(".xml", "--FHIR-RESOURCE.json");
            newPath = newPath.split("tmp").pop();
            fs.writeFileSync("../../tmp/" + newPath, JSON.stringify(resultMessage));
        }).then(() => {
            workerPool.destroy();
        });
    }
}

createAndUploadFhirResource();