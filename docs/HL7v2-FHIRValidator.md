# HL7v2 to FHIR - FHIR Validator: Common Errors/Warnings and Explanations

As you run FHIR Validator against some of your FHIR output bundle resources, you will notice there are some errors or warnings that arise. Most of these have a reason: the FHIR validator was designed on a different HL7 FHIR guidelines than the FHIR standards we adhered to based on the 2-to-FHIR Project and the latest HL7 community. Here is a list of some examples of errors/warnings you might see, and the explanations for each.

## Errors:

* *The Coding provided (urn:oid:2.16.840.1.113883.6.238#2106-3) is not in the value set http://hl7.org/fhir/us/core/ValueSet/omb-race-category, and a code is required from this value set.  (error message = Not in value set http://hl7.org/fhir/us/core/ValueSet/omb-race-category)*

**Explanation:** FHIR validator giving error for this even if the codes are available under http://hl7.org/fhir/us/core/ValueSet/omb-race-category value set.

## Warnings:

* *None of the codings provided are in the value set 'Document Type Value Set' (http://hl7.org/fhir/ValueSet/c80-doc-typecodes), and a coding is recommended to come from this value set) (codes = null#DI)*

**Explanation:** Document type is created using TXA.2 which is User Defined Table. And as per FHIR, preferred mapping for Document Type is LOINC Codes. Therefore, we have not created vocabulary mapping for the same and values from HL7 are as it is used to create Document Type in FHIR.

* *None of the codings provided are in the value set 'TimingAbbreviation' (http://hl7.org/fhir/ValueSet/timing-abbreviation), and a coding is recommended to come from this value set) (codes = http://terminology.hl7.org/CodeSystem/v2-0335#P)*

**Explanation:** We were not able to have one-to-one mapping for all the available codes in v2 table 0335 with http://terminology.hl7.org/CodeSystem/v3-GTSAbbreviation. As binding strength for timing-abbreviation is preferred, we have used values from HL7 table and system equals to http://terminology.hl7.org/CodeSystem/v2-0335.
If user has fixed values to be received in HL7, it can be mapped against http://terminology.hl7.org/CodeSystem/v3-GTSAbbreviation

* *None of the codings provided are in the value set 'Provenance activity type' (http://hl7.org/fhir/ValueSet/provenance-activity-type), and a coding should come from this value set unless it has no suitable code (note that the validator cannot judge what is suitable) (codes = null#null)*

**Explanation:** We are getting this warning because we are not mapping activity.coding.code. The datatype here is CodeableConcept, so we have used activity.text in absence of activity.coding.code. 

* *None of the codings provided are in the value set 'IdentifierType' (http://hl7.org/fhir/ValueSet/identifier-type), and a coding should come from this value set unless it has no suitable code (note that the validator cannot judge what is suitable) (codes = http://terminology.hl7.org/CodeSystem/v2-0203#VN)*

**Explanation:** FHIR provided valueset (http://hl7.org/fhir/ValueSet/identifier-type) consist of very limited codes. As suggested by FHIR, we are using codes from http://terminology.hl7.org/CodeSystem/v2-0203 codesystem.

* *None of the codings provided are in the value set 'Consent Category Codes' (http://hl7.org/fhir/ValueSet/consent-category), and a coding should come from this value set unless it has no suitable code (note that the validator cannot judge what is suitable) (codes = null#001)*

**Explanation:** Consent Category is generated from CON.2 which is User Defined Table.
FHIR provided valueset (http://hl7.org/fhir/ValueSet/consent-category) consist of very limited codes and as binding is extensible, we are using values from HL7 as it is to create Consent Category in FHIR.

* *No code provided, and a code should be provided from the value set 'Provenance activity type' (http://hl7.org/fhir/ValueSet/provenance-activity-type)*

**Explanation:** We are getting this warning because we are not mapping activity.coding.code. The datatype here is CodeableConcept, so we have used activity.text in absence of activity.coding.code.

* *No code provided, and a code should be provided from the value set 'Consent PolicyRule Codes' (http://hl7.org/fhir/ValueSet/consent-policy)*

**Explanation:** We are getting this warning because we are not mapping PolicyRule.coding.code. The datatype here is a CodeableConcept, so we have used PolicyRule.text in the absence of PolicyRule.coding.code.

* *Display Name for http://terminology.hl7.org/CodeSystem/v2-0003#O01 should be one of 'ORM - Order message (also RDE, RDS, RGV, RAS), Auftrag / Verordnung' instead of 'ORM^O01^ORM_O01' for 'http://terminology.hl7.org/CodeSystem/v2-0003#O01'*

**Explanation:** This mapping is for “eventCoding” generated from MSH-9. As per community guideline, display will contain MSH-9 (MSG.1+"^"+MSG.2+"^"+MSG.3). Hence, we kept this mapping as is.

* *Display Name for http://terminology.hl7.org/CodeSystem/v2-0301#ISO should be one of 'ISO Object Identifier, ISO-ID' instead of 'An International Standards Organization Object Identifier (OID), in accordance with ISO/IEC 8824. Formatted as decimal digits separated by periods; recommended limit of 64 characters' for 'http://terminology.hl7.org/CodeSystem/v2-0301#ISO'*

**Explanation:** We are getting this warning because we are displaying “Definition” which consist of information coming in Display as well.
