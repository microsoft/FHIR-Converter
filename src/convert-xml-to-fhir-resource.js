const fs = require("fs");
const WorkerPool = require("./lib/workers/workerPool");

function createAndUploadFhirResource()  {
    let filePath, newPath;
    const generateChirpTestFile = process.argv.slice(2)[0] == '--chirp-test-file';

    if (generateChirpTestFile) {
        filePath = "src/sample-data/cda/Chirp_CCD.cda";
        newPath = "test/e2e-test/regression-test/data/cda/ccd.hbs/ccd.hbs-Chirp_CCD.cda.json";
    } else {
        if ((process.env.npm_config_filePath || '') === '') {
            console.log("Must specify file path");
            return;
        }
        filePath = "../../" + process.env.npm_config_filePath;
        newPath = filePath.replace(".xml", "--FHIR-RESOURCE.json");
        newPath = newPath.split("tmp").pop();
        newPath = "../../tmp/" + newPath;
    }
    const xmlFile = fs.readFileSync(filePath, 'utf8');
    const workerPool = new WorkerPool('./src/lib/workers/worker.js', require('os').cpus().length);
    return workerPool.exec({
        'type': '/api/convert/:srcDataType/:template',
        'srcData': xmlFile.toString(),
        'srcDataType': "cda",
        'templateName': "ccd.hbs"
    }).then((result) => {
        let resultMessage = result.resultMsg;
        if (generateChirpTestFile) {
            resultMessage = resultMessage["fhirResource"];
        }
        fs.writeFileSync(newPath, JSON.stringify(resultMessage, null, 4));
    }).then(() => {
        workerPool.destroy();
    });
}

createAndUploadFhirResource();
