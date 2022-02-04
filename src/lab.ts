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
