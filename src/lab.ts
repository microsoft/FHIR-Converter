"use strict"

import { R4 } from "@ahryman40k/ts-fhir-types";
import { IBundle } from "@ahryman40k/ts-fhir-types/lib/R4";
import fhirClient from 'fhirclient';
import Client from "fhirclient/lib/Client";
import { either as E } from 'fp-ts'
import { now } from "fp-ts/lib/Date";

const { HL7Message } = require("hl7v2");

export function parseHL7Message(message: string):JSON {
    return HL7Message.parse(message.toString());
}

export function translateLabOrderBundle(json: R4.IBundle): string {
    console.log("First Try");
    console.log(json);
    
    let patient: R4.IPatient = <R4.IPatient>json.entry![1].resource!;
    let task: R4.ITask = <R4.ITask>json.entry![3].resource!;
    let serviceRequest: R4.IServiceRequest = <R4.IServiceRequest>json.entry![4].resource!;

    let patientId: string = patient.id!;
    let patientOmang: string = patient.identifier![0].value!
    let patientFirstName: string = patient.name![0].given![0]
    let patientLastName: string = patient.name![0].family!
    let patientDoB: string =  patient.birthDate!.split('-').join('');
    let sex: string = patient.gender!;
    let labOrderId: string = serviceRequest.identifier ? serviceRequest.identifier![0].value! : "";
    let labOrderDatetime: string = serviceRequest.authoredOn ? serviceRequest.authoredOn!.split('-').join('') : "";
    let labOrderType: string = serviceRequest.code!.coding![0].code!;

    let template = `MSH|^~\&|PIMS||IPMS||${new Date().toISOString().slice(0,10).split('-').join('')}||ORM^O01|${ Math.random() * 10000}|D|2.4|||AL|NE\nPID|1||${patientId}||${patientLastName}^${patientFirstName}^^^^^L||${patientDoB}|${sex == "female" ? "F" : "M"}|||||||||||${patientOmang}\nORC|NW|${labOrderId}^LAB|||||^^^^^R||${labOrderDatetime}|||\nOBR|1| ${labOrderId}^LAB||${labOrderType}|R||${labOrderDatetime}|||||||||||`;

    return template;
}