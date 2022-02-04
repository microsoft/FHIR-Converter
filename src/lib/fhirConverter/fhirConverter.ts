import { R4 } from "@ahryman40k/ts-fhir-types"
import { instance as hb } from '../handlebars-converter/handlebars-converter'
import * as constants from '../constants/constants'

import { readFile } from 'fs/promises'
import { errorCodes, errorMessage } from '../error/error'
import path from "path"

let dataHandlerFactory = require('../dataHandler/dataHandlerFactory')

export default class FhirConverter {
    bundle: R4.IBundle
    templateName: string

    constructor(json: R4.IBundle, templateName: string) {
        this.bundle = json
        this.templateName = templateName
    }

    async translateBundle() {
        console.log("Translating Bundle")
        console.log(`Template: ${this.templateName}\nBundle:\n${this.bundle}\n`)

        try {
            let patient: R4.IPatient = <R4.IPatient>this.bundle.entry![1].resource!
            let task: R4.ITask = <R4.ITask>this.bundle.entry![3].resource!
            let serviceRequest: R4.IServiceRequest = <R4.IServiceRequest>this.bundle.entry![4].resource!
    
            let patientId: string = patient.id!
            let patientOmang: string = patient.identifier![0].value!
            let patientFirstName: string = patient.name![0].given![0]
            let patientLastName: string = patient.name![0].family!
            let patientDoB: string = patient.birthDate!.split('-').join('')
            let sex: string = patient.gender!
            let labOrderId: string = serviceRequest.identifier ? serviceRequest.identifier![0].value! : ""
            let labOrderDatetime: string = serviceRequest.authoredOn ? serviceRequest.authoredOn!.split('-').join('') : ""
            let labOrderType: string = serviceRequest.code!.coding![0].code!
            
            let dataTypeHandler =  dataHandlerFactory.createDataHandler("fhir")
            let hbInstance = hb(true, dataTypeHandler, path.join(constants.TEMPLATE_FILES_LOCATION, dataTypeHandler.dataType))
    
            let template = await this.getTemplateFromFile("fhir", this.templateName, dataTypeHandler, hbInstance)
    
            let compiledTemplate = hbInstance.compile(dataTypeHandler.preProcessTemplate(template.toString()));
    
            let parsedData: R4.IBundle = await dataTypeHandler.parseSrcData(this.bundle);
            let dataContext = { msg: parsedData };
        
            let result = this.generateResult(dataTypeHandler, dataContext, compiledTemplate);
        
            return { status: 200, resultMsg: result }
        } catch (error) {
            console.log("Error during bundle translation: " + error)
            return {
                'status': 400,
                'resultMsg': errorMessage(errorCodes.BadRequest,
                    "Error during template evaluation:\n" + (error as Error).message)
            }
        }



        // let template = `MSH|^~\&|PIMS||IPMS||${new Date().toISOString().slice(0, 10).split('-').join('')}||ORM^O01|${Math.random() * 10000}|D|2.4|||AL|NE\nPID|1||${patientId}||${patientLastName}^${patientFirstName}^^^^^L||${patientDoB}|${sex == "female" ? "F" : "M"}|||||||||||${patientOmang}\nORC|NW|${labOrderId}^LAB|||||^^^^^R||${labOrderDatetime}|||\nOBR|1| ${labOrderId}^LAB||${labOrderType}|R||${labOrderDatetime}|||||||||||`

        // return template
    }

    private async generateResult(dataTypeHandler: any, dataContext: any, template: any) {
        let result = dataTypeHandler.postProcessResult(template(dataContext));
        return Object.assign(dataTypeHandler.getConversionResultMetadata(dataContext.msg), { 'fhirResource': result });
    }

    private async getTemplateFromFile(srcDataType: string, templateName: string, dtHandler: any, hbInstance: any) {
        try {
            const result = await readFile(path.join(constants.TEMPLATE_FILES_LOCATION, srcDataType, templateName),'binary')
            return hbInstance.compile(dtHandler.preProcessTemplate(result.toString()));
        } catch (error) {
            console.log("Template not found!")
            throw Error;
        }

    }
}