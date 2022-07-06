# HL7v2 Templates

*This document applies to the Liquid engine only. Follow [this](https://github.com/microsoft/FHIR-Converter/tree/handlebars) link for the documentation on Handlebars engine.*

For HL7v2 to FHIR conversion, we provide a total of **57 HL7v2 conversion templates**. Here, you can find a detailed information about each template, such as the mapped FHIR resource types, segments, and extensions.

## Templates

| **Message Type** | **Description**                                                              |
|------------------|------------------------------------------------------------------------------|
| ADT_A01          | Admit/visit   notification                                                   |
| ADT_A02          | Transfer a   patient                                                         |
| ADT_A03          | Discharge/end   visit                                                        |
| ADT_A04          | Register a   patient                                                         |
| ADT_A05          | Pre-admit a   patient                                                        |
| ADT_A06          | Change an outpatient to   an inpatient                                       |
| ADT_A07          | Change an   inpatient to an outpatient                                       |
| ADT_A08          | Update patient   information                                                 |
| ADT_A09          | Patient departing   - tracking                                               |
| ADT_A10          | Patient arriving   - tracking                                                |
| ADT_A11          | Cancel   admit/visit notification                                            |
| ADT_A13          | Cancel   discharge/end visit                                                 |
| ADT_A14          | Pending admit                                                                |
| ADT_A15          | Pending transfer                                                             |
| ADT_A16          | Pending discharge                                                            |
| ADT_A25          | Cancel pending   discharge                                                   |
| ADT_A26          | Cancel pending   transfer                                                    |
| ADT_A27          | Cancel pending   admit                                                       |
| ADT_A28          | Add person   information                                                     |
| ADT_A29          | Delete person   information                                                  |
| ADT_A31          | Update person   information                                                  |
| ADT_A40          | Merge patient -   patient identifier list                                    |
| ADT_A41          | Merge account -   patient account number                                     |
| ADT_A45          | Move visit information - visit number                                        |
| ADT_A47          | Change patient   identifier list                                             |
| ADT_A60          | Update allergy   information                                                 |
| BAR_P01          | Add patient accounts                                                         |
| BAR_P02          | Purge patient accounts                                                       |
| BAR_P12          | Update diagnosis/procedure                                                   |
| DFT_P03          | Post detail financial transaction                                            |
| DFT_P11          | Post detail financial transactions - expanded                                |
| MDM_T01          | Original document   notification                                             |
| MDM_T02          | Original document   notification and content                                 |
| MDM_T05          | Document addendum   notification                                             |
| MDM_T06          | Document addendum   notification and content                                 |
| MDM_T09          | Document   replacement notification                                          |
| MDM_T10          | Document   replacement notification and content                              |
| OMG_O19          | General clinical order                                                       |
| OML_O21          | Laboratory order                                                             |
| ORM_O01          | General Order   Message                                                      |
| ORU_R01          | Unsolicited   Observation Message                                            |
| OUL_R22          | Unsolicited   Specimen Oriented Observation Message                          |
| OUL_R23          | Unsolicited   Specimen Container Oriented Observation Message                |
| OUL_R24          | Unsolicited Order   Oriented Observation Message                             |
| RDE_O11          | Pharmacy/treatment   encoded order                                           |
| RDE_O25          | Pharmacy/treatment   refill authorization request                            |
| RDS_O13          | Pharmacy/treatment   dispense                                                |
| REF_I12          | Patient referral                                                             |
| REF_I14          | Cancel patient referral                                                      |
| SIU_S12          | Notification of   new appointment booking                                    |
| SIU_S13          | Notification of   appointment rescheduling                                   |
| SIU_S14          | Notification of   appointment modification                                   |
| SIU_S15          | Notification of   appointment cancellation                                   |
| SIU_S16          | Notification of   appointment discontinuation                                |
| SIU_S17          | Notification of   appointment deletion                                       |
| SIU_S26          | SIU/ACK   Notification that patient did not show up for schedule appointment |
| VXU_V04          | Unsolicited   vaccination record update                                      |

## Segments

| Segment | Short Description                            |
|---------|----------------------------------------------|
| MSH     | Message Header                               |
| EVN     | Event Type                                   |
| PID     | Patient Identification                       |
| PV1     | Patient Visit                                |
| SFT     | Software Segment                             |
| PD1     | Additional Demographics                      |
| NK1     | Next of Kin                                  |
| PV2     | Patient Visit, additional info               |
| GT1     | Guarantor                                    |
| NTE     | Notes and Comments                           |
| OBX     | Observation/Result                           |
| AL1     | Allergy Info                                 |
| DG1     | Diagnosis info                               |
| PR1     | Procedures                                   |
| IAR     | Allergy Reaction Segment                     |
| DB1     | Disability info                              |
| IN1     | Insurance                                    |
| IN2     | Insurance Additional info                    |
| RF1     | Referral info                                |
| SCH     | Scheduling Activity Information              |
| TQ1     | Timing/quantity                              |
| AIS     | Appointment Information                      |
| AIG     | Appointment Information - General Resource   |
| AIL     | Appointment Information - Location Resource  |
| AIP     | Appointment Information - Personnel Resource |
| ORC     | Common Order                                 |
| OBR     | Observation Request                          |
| CTD     | Contact Data                                 |
| TXA     | Transcription Document Header                |
| SPM     | Specimen                                     |
| IAM     | Patient Adverse Reaction Information         |
| ACC     | Accident                                     |
| ARV     | Access Restriction                           |
| AUT     | Authorization Information                    |
| CON     | Consent Segment                              |
| MRG     | Merge Patient Information                    |
| PDA     | Patient Death and autopsy                    |
| RGS     | Resource Group                               |
| RXA     | Pharmacy/treatment Administration            |
| RXR     | Pharmacy/treatment Route                     |
| TQ1     | Timing/quantity                              |
| ODS     | Dietary Orders, Supplements, and Preferences |
| RQ1     | Requisition Detail-1                         |
| RQD     | Requisition Detail                           |
| RXC     | Pharmacy/treatment Component Order           |
| RXD     | Pharmacy/treatment Dispense                  |
| RXE     | Pharmacy/treatment Encoded Order             |
| RXO     | Pharmacy prescription order segment          |
| SAC     | Specimen Container Detail                    |
| RXR     | Pharmacy/treatment Route                     |

## Extensions

For the fields which could not be mapped directly from HL7 to FHIR, we created Liquid templates with extensions. The extensions are identified by the type of FHIR resource they are associated with, such as Patient resource extensions, Encounter extensions, Observation extensions, etc.

## ADT Message Templates

### ADT_A01

**Usage**: An A01 event is intended to be used for "Admitted" patients only. An A01 event is sent as a result of a patient undergoing the admission process which assigns the patient to a bed. It signals the beginning of a patient's stay in a healthcare facility.

**Mapped segment list**: MSH, SFT, EVN, PID, PD1, NK1, PV1, PV2, DB1, OBX, AL1, DG1, PR1, GT1, IN1, IN2, RF1, ACC, PDA, ARV, AUT

**FHIR resource types and their mapped segments:**

|     FHIR Resource         |     Segment Mapped                                                                                |
|---------------------------|---------------------------------------------------------------------------------------------------|
|     MessageHeader         |     MSH, SFT                                                                                      |
|     Patient               |     PID, PD1,   GT1, NK1, IN1, IN2, DB1, PDA, ARV                                                 |
|     Provenance            |     MSH, EVN                                                                                      |
|     Organization          |     MSH, PID, GT1,   PD1, PV1, PV2, EVN, PR1, NK1, DB1, OBX, DG1, RF1, IN1, IN2, ACC, PDA, AUT    |
|     Device                |     MSH, SFT, OBX                                                                                 |
|     Account               |     PID, GT1                                                                                      |
|     RelatedPerson         |     NK1, GT1, PID,   IN1, IN2                                                                     |
|     Practitioner          |     PD1, PV1, PV2,   PR1, OBX, DG1, RF1, ACC                                                      |
|     Location              |     PV1, PV2,   EVN, PR1, PDA, ACC                                                                |
|     EpisodeOfCare         |     PV1, DG1                                                                                      |
|     Encounter             |     PV1, PV2, DG1,   PDA, ARV                                                                     |
|     Procedure             |     PR1, PDA                                                                                      |
|     PractitionerRole      |     OBX                                                                                           |
|     Practitioner          |     PDA                                                                                           |
|     Observation           |     OBX, ACC, PDA                                                                                 |
|     AllergyIntolerance    |     AL1                                                                                           |
|     Condition             |     DG1, PDA                                                                                      |
|     ServiceRequest        |     RF1                                                                                           |
|     Coverage              |     IN1, IN2                                                                                      |
|     ClaimResponse         |     AUT                                                                                           |

**Extensions**:

| FHIR Extension                      | Segment Mapped       |
|-------------------------------------|----------------------|
| Patient-   PatientExtension         | PID, PD1, PV1,   DB1 |
| Encounter-   EncounterExtension     | PV1, PV2             |
| RelatedPerson-   RelatedPerson      | NK1                  |
| Observation-   ObservationExtension | OBX, ACC             |
| Condition-   ConditionExtension     | DG1                  |

### ADT_A02

**Usage**: An A02 event is issued because of the patient changing his or her assigned physical location.

**Mapped segment list**: MSH, SFT, EVN, PID, PD1, PV1, PV2, DB1, OBX, ARV, PDA

**FHIR resource types and their mapped segments:**

| FHIR Resource    | Segment Mapped                              |
|------------------|---------------------------------------------|
| MessageHeader    | MSH, SFT                                    |
| Patient          | PID, PD1, DB1, ARV, PDA                     |
| Provenance       | MSH, EVN                                    |
| Organization     | MSH, PID, PD1, PV1, PV2, EVN, DB1, OBX, PDA |
| Device           | MSH, SFT, OBX                               |
| Account          | PID                                         |
| RelatedPerson    | PID                                         |
| Practitioner     | PD1, PV1, PV2, OBX, PDA                     |
| Location         | PV1, PV2, EVN, PDA                          |
| EpisodeOfCare    | PV1                                         |
| Encounter        | PV1, PV2, ARV, PDA                          |
| Procedure        | PR1, PDA                                    |
| PractitionerRole | OBX                                         |
| Observation      | OBX                                         |
| Condition        | PDA                                         |

**Extensions**:

| FHIR Extension                    | Segment Mapped     |
|-----------------------------------|--------------------|
| Patient- PatientExtension         | PID, PD1, PV1, DB1 |  
| Encounter- EncounterExtension     | PV1, PV2           |
| Observation- ObservationExtension | OBX                |

### ADT_A03

**Usage:** An A03 event signals the end of a patient's stay in a healthcare facility. It signals that the patient's status has changed to "discharged" and that a discharge date has been recorded. The patient is no longer in the facility.

**Mapped segment list**: MSH, SFT, EVN, PID, PD1, NK1, PV1, PV2, DB1, OBX, AL1, DG1, PR1, GT1, IN1, IN2, RF1, ACC, ARV, AUT, PDA

**FHIR resource types and their mapped segments:**

| FHIR Resource      | Segment Mapped                                                                           |
|--------------------|------------------------------------------------------------------------------------------|
| MessageHeader      | MSH, SFT                                                                                 |
| Patient            | PID, PD1, GT1, NK1, IN1, IN2, DB1, ARV, PDA                                              |
| Provenance         | MSH, EVN                                                                                 |
| Organization       | MSH, PID, GT1, PD1, PV1, PV2, EVN, PR1, NK1, DB1, OBX, DG1, RF1, IN1, IN2, ACC, AUT, PDA |
| Device             | MSH, SFT, OBX                                                                            |
| Account            | PID, GT1                                                                                 |
| RelatedPerson      | NK1, GT1, PID, IN1, IN2                                                                  |
| Practitioner       | PD1, PV1, PV2, PR1, OBX, DG1, RF1, ACC, PDA                                              |
| Location           | PV1, PV2, EVN, PR1, ACC, PDA                                                             |
| EpisodeOfCare      | PV1, DG1                                                                                 |
| Encounter          | PV1, PV2, DG1, ARV, PDA                                                                  |
| Procedure          | PR1, PDA                                                                                 |
| PractitionerRole   | OBX                                                                                      |
| Observation        | OBX                                                                                      |
| AllergyIntolerance | AL1                                                                                      |
| Condition          | DG1, PDA                                                                                 |
| ServiceRequest     | RF1                                                                                      |
| Coverage           | IN1, IN2                                                                                 |
| ClaimResponse      | AUT                                                                                      |

**Extensions**:

| FHIR Extension                    | Segment Mapped     |
|-----------------------------------|--------------------|
| Patient- PatientExtension         | PID, PD1, PV1, DB1 |
| Encounter- EncounterExtension     | PV1, PV2           |
| RelatedPerson- RelatedPerson      | NK1                |
| Observation- ObservationExtension | OBX, ACC           |
| Condition- ConditionExtension     | DG1                |

### ADT_A04

**Usage:** An A04 event signals that the patient has arrived or checked in as a one-time, or recurring outpatient, and is not assigned to a bed.

**Mapped segment list**: MSH, SFT, EVN, PID, PD1, NK1, PV1, PV2, DB1, OBX, AL1, DG1, PR1, GT1, IN1, IN2, RF1, ARV, AUT, PDA

**FHIR resource types and their mapped segments:**

| FHIR Resource      | Segment Mapped                                                                           |
|--------------------|------------------------------------------------------------------------------------------|
| MessageHeader      | MSH, SFT                                                                                 |
| Patient            | PID, PD1, GT1, NK1, IN1, IN2, DB1, ARV, PDA                                              |
| Provenance         | MSH, EVN                                                                                 |
| Organization       | MSH, PID, GT1, PD1, PV1, PV2, EVN, PR1, NK1, DB1, OBX, DG1, RF1, IN1, IN2, ACC, AUT, PDA |
| Device             | MSH, SFT, OBX                                                                            |
| Account            | PID, GT1                                                                                 |
| RelatedPerson      | NK1, GT1, PID, IN1, IN2                                                                  |
| Practitioner       | PD1, PV1, PV2, PR1, OBX, DG1, RF1, ACC, PDA                                              |
| Location           | PV1, PV2, EVN, PR1, ACC, PDA                                                             |
| EpisodeOfCare      | PV1, DG1                                                                                 |
| Encounter          | PV1, PV2, DG1, ARV, PDA                                                                  |
| Procedure          | PR1, PDA                                                                                 |
| PractitionerRole   | OBX                                                                                      |
| Observation        | OBX                                                                                      |
| AllergyIntolerance | AL1                                                                                      |
| Condition          | DG1, PDA                                                                                 |
| ServiceRequest     | RF1                                                                                      |
| Coverage           | IN1, IN2                                                                                 |
| ClaimResponse      | AUT                                                                                      |

**Extensions**:

| FHIR Extension                    | Segment Mapped     |
|-----------------------------------|--------------------|
| Patient- PatientExtension         | PID, PD1, PV1, DB1 |
| Encounter- EncounterExtension     | PV1, PV2           |
| RelatedPerson- RelatedPerson      | NK1                |
| Observation- ObservationExtension | OBX, ACC           |
| Condition- ConditionExtension     | DG1                |

### ADT_A05

**Usage:** An A05 event is sent when a patient undergoes the pre-admission process. This event can also be used to pre-register a non-admitted patient.

**Mapped segment list**: MSH, SFT, EVN, PID, PD1, NK1, PV1, PV2, DB1, OBX, AL1, DG1, PR1, GT1, IN1, IN2, RF1, ACC, ARV, AUT

**FHIR resource types and their mapped segments:**

| FHIR Resource      | Segment Mapped                                                                      |
|--------------------|-------------------------------------------------------------------------------------|
| MessageHeader      | MSH, SFT                                                                            |
| Patient            | PID, PD1, GT1, NK1, IN1, IN2, DB1, ARV                                              |
| Provenance         | MSH, EVN                                                                            |
| Organization       | MSH, PID, GT1, PD1, PV1, PV2, EVN, PR1, NK1, DB1, OBX, DG1, RF1, IN1, IN2, ACC, AUT |
| Device             | MSH, SFT, OBX                                                                       |
| Account            | PID, GT1                                                                            |
| RelatedPerson      | NK1, GT1, PID, IN1, IN2                                                             |
| Practitioner       | PD1, PV1, PV2, PR1, OBX, DG1, RF1, ACC                                              |
| Location           | PV1, PV2, EVN, PR1, ACC                                                             |
| EpisodeOfCare      | PV1, DG1                                                                            |
| Encounter          | PV1, PV2, DG1, ARV                                                                  |
| Procedure          | PR1                                                                                 |
| PractitionerRole   | OBX                                                                                 |
| Observation        | OBX                                                                                 |
| AllergyIntolerance | AL1                                                                                 |
| Condition          | DG1                                                                                 |
| ServiceRequest     | RF1                                                                                 |
| Coverage           | IN1, IN2                                                                            |
| ClaimResponse      | AUT                                                                                 |

**Extensions**:

| FHIR Extension                    | Segment Mapped     |
|-----------------------------------|--------------------|
| Patient- PatientExtension         | PID, PD1, PV1, DB1 |
| Encounter- EncounterExtension     | PV1, PV2           |
| RelatedPerson- RelatedPerson      | NK1                |
| Observation- ObservationExtension | OBX, ACC           |
| Condition- ConditionExtension     | DG1                |

### ADT_A06

**Usage:** An A06 event is sent when a patient who was present for a non-admitted visit is being admitted after an evaluation of the seriousness of the patient's condition. This event changes a patient's status from non-admitted to admitted.

**Mapped segment list**: MSH, SFT, EVN, PID, PD1, NK1, PV1, PV2, DB1, OBX, AL1, DG1, PR1, GT1, IN1, IN2, ACC, ARV, MRG, ROL

**FHIR resource types and their mapped segments:**

| FHIR Resource      | Segment Mapped                                                                 |
|--------------------|--------------------------------------------------------------------------------|
| MessageHeader      | MSH, SFT                                                                       |
| Patient            | PID, PD1, GT1, NK1, IN1, IN2, DB1, ARV                                         |
| Provenance         | MSH, EVN                                                                       |
| Organization       | MSH, PID, GT1, PD1, PV1, PV2, EVN, PR1, NK1, DB1, OBX, DG1, IN1, IN2, ACC, ROL |
| Device             | MSH, SFT, OBX                                                                  |
| Account            | PID, GT1, MRG                                                                  |
| RelatedPerson      | NK1, GT1, PID, IN1, IN2                                                        |
| Practitioner       | PD1, PV1, PV2, PR1, OBX, DG1, ACC, ROL                                         |
| Location           | PV1, PV2, EVN, PR1, ACC, ROL                                                   |
| EpisodeOfCare      | PV1, DG1                                                                       |
| Encounter          | PV1, PV2, DG1, ARV                                                             |
| Procedure          | PR1                                                                            |
| PractitionerRole   | OBX, ROL                                                                       |
| Observation        | OBX                                                                            |
| AllergyIntolerance | AL1                                                                            |
| Condition          | DG1                                                                            |
| Coverage           | IN1, IN2                                                                       |

**Extensions**:

| FHIR Extension                    | Segment Mapped     |
|-----------------------------------|--------------------|
| Patient- PatientExtension         | PID, PD1, PV1, DB1 |
| Encounter- EncounterExtension     | PV1, PV2           |
| RelatedPerson- RelatedPerson      | NK1                |
| Observation- ObservationExtension | OBX, ACC           |
| Condition- ConditionExtension     | DG1                |

### ADT_A07

**Usage:** An A07 event is sent when a patient who was admitted changes his/her status to "no longer admitted" but is still being seen for this episode of care. This event changes a patient from an "admitted" to a "non-admitted" status.

**Mapped segment list**: MSH, SFT, EVN, PID, PD1, NK1, PV1, PV2, DB1, OBX, AL1, DG1, PR1, GT1, IN1, IN2, ACC, ARV, MRG, ROL

**FHIR resource types and their mapped segments:**

| FHIR Resource      | Segment Mapped                                                                 |
|--------------------|--------------------------------------------------------------------------------|
| MessageHeader      | MSH, SFT                                                                       |
| Patient            | PID, PD1, GT1, NK1, IN1, IN2, DB1, ARV                                         |
| Provenance         | MSH, EVN                                                                       |
| Organization       | MSH, PID, GT1, PD1, PV1, PV2, EVN, PR1, NK1, DB1, OBX, DG1, IN1, IN2, ACC, ROL |
| Device             | MSH, SFT, OBX                                                                  |
| Account            | PID, GT1, MRG                                                                  |
| RelatedPerson      | NK1, GT1, PID, IN1, IN2                                                        |
| Practitioner       | PD1, PV1, PV2, PR1, OBX, DG1, ACC, ROL                                         |
| Location           | PV1, PV2, EVN, PR1, ACC, ROL                                                   |
| EpisodeOfCare      | PV1, DG1                                                                       |
| Encounter          | PV1, PV2, DG1, ARV                                                             |
| Procedure          | PR1                                                                            |
| PractitionerRole   | OBX, ROL                                                                       |
| Observation        | OBX                                                                            |
| AllergyIntolerance | AL1                                                                            |
| Condition          | DG1                                                                            |
| Coverage           | IN1, IN2                                                                       |

**Extensions**:

| FHIR Extension                    | Segment Mapped     |
|-----------------------------------|--------------------|
| Patient- PatientExtension         | PID, PD1, PV1, DB1 |
| Encounter- EncounterExtension     | PV1, PV2           |
| RelatedPerson- RelatedPerson      | NK1                |
| Observation- ObservationExtension | OBX, ACC           |
| Condition- ConditionExtension     | DG1                |

### ADT_A08

**Usage:** An A08 event is used when any patient information has changed but when no other trigger event has occurred.

**Mapped segment list**: MSH, SFT, EVN, PID, PD1, NK1, PV1, PV2, DB1, OBX, AL1, DG1, PR1, GT1, IN1, IN2, RF1, ACC, ARV, AUT, PDA

**FHIR resource types and their mapped segments:**

| FHIR Resource      | Segment Mapped                                                                      |
|--------------------|-------------------------------------------------------------------------------------|
| MessageHeader      | MSH, SFT                                                                            |
| Patient            | PID, PD1, GT1, NK1, IN1, IN2, DB1, ARV, PDA                                         |
| Provenance         | MSH, EVN                                                                            |
| Organization       | MSH, PID, GT1, PD1, PV1, PV2, EVN, PR1, NK1, DB1, OBX, DG1, RF1, IN1, IN2, ACC, PDA |
| Device             | MSH, SFT, OBX                                                                       |
| Account            | PID, GT1                                                                            |
| RelatedPerson      | NK1, GT1, PID, IN1, IN2                                                             |
| Practitioner       | PD1, PV1, PV2, PR1, OBX, DG1, RF1, ACC                                              |
| Location           | PV1, PV2, EVN, PR1, PDA                                                             |
| EpisodeOfCare      | PV1, DG1                                                                            |
| Encounter          | PV1, PV2, DG1, ARV, PDA                                                             |
| Procedure          | PR1, PDA                                                                            |
| PractitionerRole   | OBX                                                                                 |
| Observation        | OBX                                                                                 |
| AllergyIntolerance | AL1                                                                                 |
| Condition          | DG1, PDA                                                                            |
| ServiceRequest     | RF1                                                                                 |
| Coverage           | IN1, IN2                                                                            |
| ClaimResponse      | AUT                                                                                 |

**Extensions**:

| FHIR Extension                    | Segment Mapped     |
|-----------------------------------|--------------------|
| Patient- PatientExtension         | PID, PD1, PV1, DB1 |
| Encounter- EncounterExtension     | PV1, PV2           |
| RelatedPerson- RelatedPerson      | NK1                |
| Observation- ObservationExtension | OBX, ACC           |
| Condition- ConditionExtension     | DG1                |

### ADT_A09

**Usage:** The A09 event is used when there is a change in a patient's physical location (inpatient or outpatient) and when this is NOT a change in the official census bed location, as in the case of an outpatient setting.

**Mapped segment list**: MSH, SFT, EVN, PID, PD1, PV1, PV2, DB1, OBX

**FHIR resource types and their mapped segments:**

| FHIR Resource    | Segment Mapped                              |
|------------------|---------------------------------------------|
| MessageHeader    | MSH, SFT                                    |
| Patient          | PID, PD1, DB1                               |
| Provenance       | MSH, EVN                                    |
| Organization     | MSH, PID, PD1, PV1, PV2, EVN, DB1, OBX, RF1 |
| Device           | MSH, SFT, OBX                               |
| Account          | PID                                         |
| RelatedPerson    | PID                                         |
| Practitioner     | PD1, PV1, PV2, PR1, OBX                     |
| Location         | PV1, PV2, EVN, PR1                          |
| EpisodeOfCare    | PV1                                         |
| Encounter        | PV1, PV2                                    |
| PractitionerRole | OBX                                         |
| Observation      | OBX                                         |

**Extensions**:

| FHIR Extension                    | Segment Mapped |
|-----------------------------------|----------------|
| Patient- PatientExtension         | PID, PD1, PV1  |
| Encounter- EncounterExtension     | PV1, PV2       |
| Observation- ObservationExtension | OBX            |

### ADT_A10

**Usage:** The A10 event is sent when a patient arrives at a new location in the healthcare facility (inpatient or outpatient). The A09 (tracks patient departing) and A10 events are used together when there is a change in a patient's physical location and when this is NOT a change in the official census bed location, as in the case of an outpatient setting.

**Mapped segment list**: MSH, SFT, EVN, PID, PD1, PV1, PV2, DB1, OBX

**FHIR resource types and their mapped segments:**

| FHIR Resource    | Segment Mapped                              |
|------------------|---------------------------------------------|
| MessageHeader    | MSH, SFT                                    |
| Patient          | PID, PD1, DB1                               |
| Provenance       | MSH, EVN                                    |
| Organization     | MSH, PID, PD1, PV1, PV2, EVN, DB1, OBX, RF1 |
| Device           | MSH, SFT, OBX                               |
| Account          | PID                                         |
| RelatedPerson    | PID                                         |
| Practitioner     | PD1, PV1, PV2, PR1, OBX                     |
| Location         | PV1, PV2, EVN, PR1                          |
| EpisodeOfCare    | PV1                                         |
| Encounter        | PV1, PV2                                    |
| PractitionerRole | OBX                                         |
| Observation      | OBX                                         |

**Extensions**:

| FHIR Extension                    | Segment Mapped |
|-----------------------------------|----------------|
| Patient- PatientExtension         | PID, PD1, PV1  |
| Encounter- EncounterExtension     | PV1, PV2       |
| Observation- ObservationExtension | OBX            |

### ADT_A11

**Usage:** The A11 event is used for "admitted" patients. It is sent when an A01 (admit/visit notification) event is cancelled, either because of an erroneous entry of the A01 event, or because of a decision not to admit the patient after all. The "Encounter.status" is based on the ADT_A11 trigger event (e.g. cancel admit/visit notification updates the status field to 'canceled').

**Mapped segment list**: MSH, EVN, PID, PD1, PV1, PV2, SFT, DB1, OBX

**FHIR resource types and their mapped segments:**

| FHIR Resource    | Segment Mapped                         |
|------------------|----------------------------------------|
| MessageHeader    | MSH, SFT                               |
| Patient          | PID, PD1, DB1                          |
| Provenance       | MSH, EVN                               |
| Organization     | MSH, EVN, PID, PD1, PV1, PV2, DB1, OBX |
| Device           | MSH, SFT, OBX                          |
| Account          | PID                                    |
| RelatedPerson    | PID                                    |
| Practitioner     | EVN, PD1, PV1, PV2, OBX                |
| Location         | EVN, PV1, PV2                          |
| EpisodeOfCare    | PV1                                    |
| Encounter        | PV1, PV2                               |
| PractitionerRole | OBX                                    |
| Observation      | OBX                                    |

**Extensions**:

| FHIR Extension                    | Segment Mapped |
|-----------------------------------|----------------|
| Patient- PatientExtension         | PID, PD1, PV1  |
| Encounter- EncounterExtension     | PV1, PV2       |
| Observation- ObservationExtension | OBX            |

### ADT_A13

**Usage:** The A13 event is sent when an A03 (discharge/end visit) event is cancelled, either because of erroneous entry of the A03 event or because of a decision not to discharge or end the visit of the patient after all.

**Mapped segment list**: MSH, EVN, PID, PD1, PV1, PV2, SFT, NK1, DB1, OBX, AL1, DG1, PR1, GT1, IN1, IN2, RF1, ACC, ARV, AUT, PDA

**FHIR resource types and their mapped segments:**

| FHIR Resource      | Segment Mapped                                                                           |
|--------------------|------------------------------------------------------------------------------------------|
| MessageHeader      | MSH, SFT                                                                                 |
| Patient            | PID, PD1, DB1, NK1, GT1, IN1, IN2, ARV, PDA                                              |
| Provenance         | MSH, EVN                                                                                 |
| Organization       | MSH, EVN, PID, PD1, PV1, PV2, DB1, OBX, NK1, DG1, PR1, GT1, IN1, IN2, RF1, ACC, AUT, PDA |
| Device             | MSH, SFT, OBX                                                                            |
| Account            | PID, GT1                                                                                 |
| RelatedPerson      | PID, NK1, GT1, IN1, IN2                                                                  |
| Practitioner       | EVN, PD1, PV1, PV2, OBX, DG1, PR1, RF1, ACC                                              |
| Location           | EVN, PV1, PV2, PR1, ACC, PDA                                                             |
| EpisodeOfCare      | PV1, DG1                                                                                 |
| Encounter          | PV1, PV2, DG1, ARV, PDA                                                                  |
| Procedure          | PR1, PDA                                                                                 |
| PractitionerRole   | OBX                                                                                      |
| Observation        | OBX                                                                                      |
| AllergyIntolerance | AL1                                                                                      |
| Condition          | DG1, PDA                                                                                 |
| ServiceRequest     | RF1                                                                                      |
| Coverage           | IN1, IN2                                                                                 |
| ClaimResponse      | AUT                                                                                      |

**Extensions**:

| FHIR Extension                    | Segment Mapped     |
|-----------------------------------|--------------------|
| Patient- PatientExtension         | PID, PD1, PV1, DB1 |
| Encounter- EncounterExtension     | PV1, PV2           |
| RelatedPerson- RelatedPerson      | NK1                |
| Observation- ObservationExtension | OBX, ACC           |
| Condition- ConditionExtension     | DG1                |

### ADT_A14

**Usage:** An A14 event notifies other systems of a planned admission, when there is a reservation or when patient admission is to occur imminently. The A14 event is similar to a pre-admit, but without the implication that an account should be opened for the purposes of tests prior to admission. It is used when advanced notification of an admit is required in order to prepare for the patient's arrival.

**Mapped segment list**: MSH, EVN, PID, PD1, PV1, PV2, SFT, NK1, DB1, OBX, AL1, DG1, PR1, GT1, IN1, IN2, RF1, ACC, ARV, AUT

**FHIR resource types and their mapped segments:**

| FHIR Resource      | Segment Mapped                                                                      |
|--------------------|-------------------------------------------------------------------------------------|
| MessageHeader      | MSH, SFT                                                                            |
| Patient            | PID, PD1, DB1, NK1, GT1, IN1, IN2, ARV                                              |
| Provenance         | MSH, EVN                                                                            |
| Organization       | MSH, EVN, PID, PD1, PV1, PV2, DB1, OBX, NK1, DG1, PR1, GT1, IN1, IN2, RF1, ACC, AUT |
| Device             | MSH, SFT, OBX                                                                       |
| Account            | PID, GT1                                                                            |
| RelatedPerson      | PID, NK1, GT1, IN1, IN2                                                             |
| Practitioner       | EVN, PD1, PV1, PV2, OBX, DG1, PR1, RF1, ACC                                         |
| Location           | EVN, PV1, PV2, PR1, ACC                                                             |
| EpisodeOfCare      | PV1, DG1                                                                            |
| Encounter          | PV1, PV2, DG1, ARV                                                                  |
| Procedure          | PR1                                                                                 |
| PractitionerRole   | OBX                                                                                 |
| Observation        | OBX                                                                                 |
| AllergyIntolerance | AL1                                                                                 |
| Condition          | DG1                                                                                 |
| ServiceRequest     | RF1                                                                                 |
| Coverage           | IN1, IN2                                                                            |
| ClaimResponse      | AUT                                                                                 |

**Extensions**:

| FHIR Extension                      | Segment Mapped       |
|-------------------------------------|----------------------|
| Patient-   PatientExtension         | PID, PD1,   PV1, DB1 |
| Encounter-   EncounterExtension     | PV1, PV2             |
| RelatedPerson-   RelatedPerson      | NK1                  |
| Observation-   ObservationExtension | OBX, ACC             |
| Condition-   ConditionExtension     | DG1                  |

### ADT_A15

**Usage:** An A15 event notifies other systems of a plan to transfer a patient to a new location when the patient has not yet left the old location. It is used when advanced notification of a transfer is required in order to prepare for the patient's location change. For example, this transaction could be sent so that staff will be on hand to move the patient or so that dietary services can route the next meal to the new location.

**Mapped segment list**: MSH, EVN, PID, PD1, PV1, PV2, SFT, DB1, OBX, ARV

**FHIR resource types and their mapped segments:**

| FHIR Resource    | Segment Mapped                           |
|------------------|------------------------------------------|
| MessageHeader    | MSH, SFT                                 |
| Patient          | PID, PD1, DB1,   ARV                     |
| Provenance       | MSH, EVN                                 |
| Organization     | MSH, EVN,   PID, PD1, PV1, PV2, DB1, OBX |
| Device           | MSH, SFT, OBX                            |
| Account          | PID                                      |
| RelatedPerson    | PID                                      |
| Practitioner     | EVN, PD1,   PV1, PV2, OBX                |
| Location         | EVN, PV1, PV2                            |
| EpisodeOfCare    | PV1                                      |
| Encounter        | PV1, PV2, ARV                            |
| PractitionerRole | OBX                                      |
| Observation      | OBX                                      |

**Extensions**:

| FHIR Extension                      | Segment Mapped       |
|-------------------------------------|----------------------|
| Patient-   PatientExtension         | PID, PD1,   PV1, DB1 |
| Encounter-   EncounterExtension     | PV1, PV2             |
| Observation-   ObservationExtension | OBX                  |

### ADT_A16

**Usage:** An A16 event notifies other systems of a plan to discharge a patient when the patient has not yet left the healthcare facility. It is used when advanced notification of a discharge is required in order to prepare for the patient's change in location. For example, it is used to notify the pharmacy of the possible need for discharge drugs or to notify psychotherapy of the possible need for post-discharge appointments or to notify the extended care or home health system that the patient will be discharged and that the new extended care and home health admission assessment can be scheduled.

**Mapped segment list**: MSH, EVN, PID, PD1, PV1, PV2, SFT, NK1, DB1, OBX, AL1, DG1, PR1, GT1, IN1, IN2, RF1, ACC, ARV, AUT

**FHIR resource types and their mapped segments:**

| FHIR Resource      | Segment Mapped                                                                        |
|--------------------|---------------------------------------------------------------------------------------|
| MessageHeader      | MSH, SFT                                                                              |
| Patient            | PID, PD1,   DB1, NK1, GT1, IN1, IN2, ARV                                              |
| Provenance         | MSH, EVN                                                                              |
| Organization       | MSH, EVN,   PID, PD1, PV1, PV2, DB1, OBX, NK1, DG1, PR1, GT1, IN1, IN2, RF1, ACC, AUT |
| Device             | MSH, SFT, OBX                                                                         |
| Account            | PID, GT1                                                                              |
| RelatedPerson      | PID, NK1,   GT1, IN1, IN2                                                             |
| Practitioner       | EVN, PD1,   PV1, PV2, OBX, DG1, PR1, RF1, ACC                                         |
| Location           | EVN, PV1,   PV2, PR1, ACC                                                             |
| EpisodeOfCare      | PV1, DG1                                                                              |
| Encounter          | PV1, PV2, DG1,   ARV                                                                  |
| Procedure          | PR1                                                                                   |
| PractitionerRole   | OBX                                                                                   |
| Observation        | OBX                                                                                   |
| AllergyIntolerance | AL1                                                                                   |
| Condition          | DG1                                                                                   |
| ServiceRequest     | RF1                                                                                   |
| Coverage           | IN1, IN2                                                                              |
| ClaimResponse      | AUT                                                                                   |

**Extensions**:

| FHIR Extension                      | Segment Mapped       |
|-------------------------------------|----------------------|
| Patient-   PatientExtension         | PID, PD1,   PV1, DB1 |
| Encounter-   EncounterExtension     | PV1, PV2             |
| RelatedPerson-   RelatedPerson      | NK1                  |
| Observation-   ObservationExtension | OBX, ACC             |
| Condition-   ConditionExtension     | DG1                  |

### ADT_A25

**Usage:** The A25 event is sent when an A16 (pending discharge) event is cancelled, either because of erroneous entry of the A16 event or because of a decision not to discharge the patient after all. The "Encounter.status" here, is based on the ADT_A25 trigger event i.e., Cancel pending discharge, hence, we are updating it to “in-progress”.

**Mapped segment list**: MSH, EVN, PID, PD1, PV1, PV2, SFT, DB1, OBX

**FHIR resource types and their mapped segments:**

| FHIR   Resource  | Segment   Mapped                         |
|------------------|------------------------------------------|
| MessageHeader    | MSH, SFT                                 |
| Patient          | PID, PD1, DB1                            |
| Provenance       | MSH, EVN                                 |
| Organization     | MSH, EVN, PID,   PD1, PV1, PV2, DB1, OBX |
| Device           | MSH, SFT, OBX                            |
| Account          | PID                                      |
| RelatedPerson    | PID                                      |
| Practitioner     | EVN, PD1, PV1,   PV2, OBX                |
| Location         | EVN, PV1, PV2                            |
| EpisodeOfCare    | PV1                                      |
| Encounter        | PV1, PV2                                 |
| PractitionerRole | OBX                                      |
| Observation      | OBX                                      |

**Extensions**:

| FHIR   Extension                    | Segment   Mapped     |
|-------------------------------------|----------------------|
| Patient-   PatientExtension         | PID, PD1, PV1,   DB1 |
| Encounter-   EncounterExtension     | PV1, PV2             |
| Observation-   ObservationExtension | OBX                  |

### ADT_A26

**Usage:** The A26 event is sent when an A15 (pending transfer) event is cancelled, either because of erroneous entry of the A15 event or because of a decision not to transfer the patient after all. The "Encounter.status" here, is based on the ADT_A26 trigger event i.e., Cancel pending transfer, hence we are updating it to “in-progress”.

**Mapped segment list**: MSH, EVN, PID, PD1, PV1, PV2, SFT, DB1, OBX

**FHIR resource types and their mapped segments:**

| FHIR   Resource  | Segment   Mapped                         |
|------------------|------------------------------------------|
| MessageHeader    | MSH, SFT                                 |
| Patient          | PID, PD1, DB1                            |
| Provenance       | MSH, EVN                                 |
| Organization     | MSH, EVN, PID,   PD1, PV1, PV2, DB1, OBX |
| Device           | MSH, SFT, OBX                            |
| Account          | PID                                      |
| RelatedPerson    | PID                                      |
| Practitioner     | EVN, PD1, PV1,   PV2, OBX                |
| Location         | EVN, PV1, PV2                            |
| EpisodeOfCare    | PV1                                      |
| Encounter        | PV1, PV2                                 |
| PractitionerRole | OBX                                      |
| Observation      | OBX                                      |

**Extensions**:

| FHIR   Extension                    | Segment   Mapped     |
|-------------------------------------|----------------------|
| Patient-   PatientExtension         | PID, PD1, PV1,   DB1 |
| Encounter-   EncounterExtension     | PV1, PV2             |
| Observation-   ObservationExtension | OBX                  |

### ADT_A27

**Usage:** The A27 event is sent when an A14 (pending admit) event is cancelled, either because of erroneous entry of the A14 event or because of a decision not to admit the patient after all. The "Encounter.status" here, is based on the ADT_A27 trigger event i.e., Cancel pending admit, hence we are updating it to “cancelled”.

**Mapped segment list**: MSH, SFT, EVN, PID, PD1, PV1, PV2, DB1, OBX

**FHIR resource types and their mapped segments:**

| FHIR Resource    | Segment Mapped                           |
|------------------|------------------------------------------|
| MessageHeader    | MSH, SFT                                 |
| Patient          | PID, PD1, DB1                            |
| Provenance       | MSH, EVN                                 |
| Organization     | MSH, PID, PD1,   PV1, PV2, EVN, DB1, OBX |
| Device           | MSH, SFT, OBX                            |
| Account          | PID                                      |
| RelatedPerson    | PID                                      |
| Practitioner     | PD1, PV1, PV2,   OBX, EVN                |
| Location         | PV1, PV2, EVN                            |
| EpisodeOfCare    | PV1                                      |
| Encounter        | PV1, PV2                                 |
| PractitionerRole | OBX                                      |
| Observation      | OBX                                      |

**Extensions**:

| FHIR   Extension                    | Segment   Mapped     |
|-------------------------------------|----------------------|
| Patient-   PatientExtension         | PID, PD1, PV1,   DB1 |
| Encounter-   EncounterExtension     | PV1, PV2             |
| Observation-   ObservationExtension | OBX                  |

### ADT_A28

**Usage:** The ADT_A28 event allows sites with multiple systems and respective master patient databases to communicate activity related to a person regardless of whether that person is currently a patient on each system. Each system has an interest in the database activity of the others to maintain data integrity across an institution. Though they are defined within the ADT message set, these messages differ in that they are not patient-specific. The A28 event can be used to send everything that is known about a person.

**Mapped segment list**: MSH, SFT, EVN, PID, PD1, NK1, PV1, PV2, DB1, OBX, AL1, DG1, PR1, GT1, IN1, IN2, RF1, ACC, ARV

**FHIR resource types and their mapped segments:**

| FHIR Resource      | Segment Mapped                                                                   |
|--------------------|----------------------------------------------------------------------------------|
| MessageHeader      | MSH, SFT                                                                         |
| Patient            | PID, PD1,   GT1, NK1, IN1, IN2, DB1, ARV                                         |
| Provenance         | MSH, EVN                                                                         |
| Organization       | MSH, PID,   GT1, PD1, PV1, PV2, EVN, PR1, NK1, DB1, OBX, DG1, RF1, IN1, IN2, ACC |
| Device             | MSH, SFT, OBX                                                                    |
| Account            | PID, GT1                                                                         |
| RelatedPerson      | NK1, GT1,   PID, IN1, IN2                                                        |
| Practitioner       | PD1, PV1,   PV2, PR1, OBX, DG1, RF1, ACC                                         |
| Location           | PV1, PV2,   EVN, PR1, ACC                                                        |
| EpisodeOfCare      | PV1, DG1                                                                         |
| Encounter          | PV1, PV2, DG1,   ARV                                                             |
| Procedure          | PR1                                                                              |
| PractitionerRole   | OBX                                                                              |
| Observation        | OBX                                                                              |
| AllergyIntolerance | AL1                                                                              |
| Condition          | DG1                                                                              |
| ServiceRequest     | RF1                                                                              |
| Coverage           | IN1, IN2                                                                         |

**Extensions**:

| FHIR Extension                      | Segment Mapped       |
|-------------------------------------|----------------------|
| Patient- PatientExtension           | PID, PD1,   PV1, DB1 |
| Encounter-   EncounterExtension     | PV1, PV2             |
| RelatedPerson-   RelatedPerson      | NK1                  |
| Observation-   ObservationExtension | OBX, ACC             |
| Condition-   ConditionExtension     | DG1                  |

### ADT_A29

**Usage:** An A29 event can be used to delete all demographic information related to a given person. The “request.method” here, is based on the ADT_A29 trigger event i.e., Delete person information, hence we are updating it to “DELETE”. This will delete all the demographic information from the FHIR Server. Note: FHIR resources will only have the DELETE method and will delete resource if same ID exists on server.

**Mapped segment list**: MSH, SFT, EVN, PID, PD1, PV1, PV2, DB1, OBX

**FHIR resource types and their mapped segments:**

| FHIR Resource    | Segment Mapped                           |
|------------------|------------------------------------------|
| MessageHeader    | MSH, SFT                                 |
| Patient          | PID, PD1 ,   DB1                         |
| Provenance       | MSH, EVN                                 |
| Organization     | MSH, PID,   PD1, PV1, PV2, EVN, DB1, OBX |
| Device           | MSH, SFT, OBX                            |
| Account          | PID                                      |
| RelatedPerson    | PID                                      |
| Practitioner     | PD1, PV1,   PV2, OBX, EVN                |
| Location         | PV1, PV2, EVN                            |
| EpisodeOfCare    | PV1                                      |
| Encounter        | PV1, PV2                                 |
| PractitionerRole | OBX                                      |
| Observation      | OBX                                      |

**Extensions**:

| FHIR   Extension                  | Segment   Mapped     |
|-----------------------------------|----------------------|
| Patient-   PatientExtension       | PID, PD1, PV1,   DB1 |
| Encounter-   EncounterExtension   | PV1, PV2             |
| Observation- ObservationExtension | OBX                  |

### ADT_A31

**Usage:** An A31 event can be used to update person information on an MPI.

**Mapped segment list**: MSH, SFT, EVN, PID, PD1, NK1, PV1, PV2, DB1, OBX, AL1, DG1, PR1, GT1, IN1, IN2, RF1, ACC, AUT

**FHIR resource types and their mapped segments:**

| FHIR Resource      | Segment Mapped                                                                        |
|--------------------|---------------------------------------------------------------------------------------|
| MessageHeader      | MSH, SFT                                                                              |
| Patient            | PID, PD1,   GT1, NK1, IN1, IN2, DB1, ARV                                              |
| Provenance         | MSH, EVN                                                                              |
| Organization       | MSH, PID,   GT1, PD1, PV1, PV2, EVN, PR1, NK1, DB1, OBX, DG1, RF1, IN1, IN2, ACC, AUT |
| Device             | MSH, SFT, OBX                                                                         |
| Account            | PID, GT1                                                                              |
| RelatedPerson      | NK1, GT1,   PID, IN1, IN2                                                             |
| Practitioner       | PD1, PV1,   PV2, PR1, OBX, DG1, RF1, ACC                                              |
| Location           | PV1, PV2,   EVN, PR1, ACC                                                             |
| EpisodeOfCare      | PV1, DG1                                                                              |
| Encounter          | PV1, PV2, DG1,   ARV                                                                  |
| Procedure          | PR1                                                                                   |
| PractitionerRole   | OBX                                                                                   |
| Observation        | OBX                                                                                   |
| AllergyIntolerance | AL1                                                                                   |
| Condition          | DG1                                                                                   |
| ServiceRequest     | RF1                                                                                   |
| Coverage           | IN1, IN2                                                                              |
| ClaimResponse      | AUT                                                                                   |

**Extensions**:

| FHIR Extension                      | Segment Mapped       |
|-------------------------------------|----------------------|
| Patient-   PatientExtension         | PID, PD1,   PV1, DB1 |
| Encounter-   EncounterExtension     | PV1, PV2             |
| RelatedPerson-   RelatedPerson      | NK1                  |
| Observation-   ObservationExtension | OBX, ACC             |
| Condition-   ConditionExtension     | DG1                  |

### ADT_A40

**Usage:** An A40 event is used to signal a merge of records for a patient that was incorrectly filed under two different identifiers. The "incorrect source identifier" identified in the MRG segment (MRG-1 - Prior Patient Identifier List) is to be merged with the required "correct target identifier" of the same "identifier type code" component identified in the PID segment (PID-3 - Patient Identifier List). The "incorrect source identifier" would then logically never be referenced in future transactions. It is noted that some systems may still physically keep this "incorrect identifier" for audit trail purposes or other reasons associated with database index implementation requirements.

**Mapped segment list**: MSH, EVN, PID, SFT, MRG

**FHIR resource types and their mapped segments:**

|     FHIR   Resource    |     Segment   Mapped    |
|------------------------|-------------------------|
|     MessageHeader      |     MSH, SFT            |
|     Provenance         |     MSH, EVN            |
|     Organization       |     MSH, EVN, MRG       |
|     Device             |     MSH, SFT            |
|     Practitioner       |     EVN                 |
|     Location           |     EVN                 |
|     Linkage            |     MRG                 |

**Extensions**: N/A

### ADT_A41

**Usage:** An A41 event is used to signal a merge of records for an account that was incorrectly filed under two different account numbers. The "incorrect source patient account number" identified in the MRG segment (MRG-3 - Prior Patient Account Number) is to be merged with the "correct target patient account number" identified in the PID segment (PID-18 - Patient Account Number). The "incorrect source patient account number" would then logically never be referenced in future transactions. It is noted that some systems may still physically keep this "incorrect identifier" for audit trail purposes or other reasons associated with database index implementation requirements.

**Mapped segment list**: MSH, EVN, PID, SFT, MRG

**FHIR resource types and their mapped segments:**

|     FHIR   Resource    |     Segment   Mapped    |
|------------------------|-------------------------|
|     MessageHeader      |     MSH, SFT            |
|     Provenance         |     MSH, EVN            |
|     Organization       |     MSH, EVN, MRG       |
|     Device             |     MSH, SFT            |
|     Practitioner       |     EVN                 |
|     Location           |     EVN                 |
|     Linkage            |     MRG                 |

**Extensions**: N/A

### ADT_A45

**Usage:** An A45 event is used to signal a move of records identified by the MRG-5 - Prior Visit Number or the MRG-6 - Prior Alternate Visit ID from the "incorrect source account identifier" identified in the MRG segment (MRG-3 - Prior Patient Account Number) to the "correct target account identifier" identified in the PID segment (PID-18 - Patient Account Number).

**Mapped segment list**: MSH, SFT, EVN, PID, PV1, MRG

**FHIR resource types and their mapped segments:**

|     FHIR Resource    |     Segment Mapped          |
|----------------------|-----------------------------|
|     MessageHeader    |     MSH, SFT                |
|     Patient          |     PID, ARV                |
|     Provenance       |     MSH, EVN                |
|     Organization     |     MSH, PID,   PV1, EVN    |
|     Device           |     MSH, SFT                |
|     Account          |     PID                     |
|     RelatedPerson    |     PID                     |
|     Practitioner     |     PV1                     |
|     Location         |     PV1, EVN                |
|     EpisodeOfCare    |     PV1                     |
|     Encounter        |     PV1, ARV                |

**Extensions**:

|     FHIR Extension                     |     Segment Mapped    |
|----------------------------------------|-----------------------|
|     Patient-   PatientExtension        |     PID, PD1, PV1     |
|     Encounter-   EncounterExtension    |     PV1               |

### ADT_A47

**Usage:** An A47 event is used to signal a change of an incorrectly assigned PID-3 - Patient Identifier List value. The "incorrect source identifier" value is stored in the MRG segment.

**Mapped segment list**: MSH, SFT, EVN, PID, PD1

**FHIR resource types and their mapped segments:**

|     FHIR Resource     |     Segment Mapped     |
|-----------------------|------------------------|
|     MessageHeader     |     MSH, SFT           |
|     Provenance        |     MSH, EVN           |
|     Organization      |     MSH, EVN, MRG      |
|     Device            |     MSH, SFT           |
|     Practitioner      |     EVN                |
|     Location          |     EVN                |
|     Linkage           |     MRG                |

**Extensions**: N/A

### ADT_A60

**Usage:** This trigger event is used when person/patient allergy information has changed. It is used in conjunction with a new allergy segment, the IAM - Patient Allergy Information Segment-Unique Identifier.

**Mapped segment list**: MSH, SFT, EVN, PID, PV1, PV2, IAM, NTE, IAR, ARV

**FHIR resource types and their mapped segments:**

|     FHIR Resource         |     Segment Mapped                         |
|---------------------------|--------------------------------------------|
|     MessageHeader         |     MSH, SFT                               |
|     Patient               |     PID, ARV                               |
|     Provenance            |     MSH, EVN                               |
|     Organization          |     MSH, PID,   PV1, PV2, EVN, NTE, IAM    |
|     Device                |     MSH, SFT                               |
|     Account               |     PID                                    |
|     RelatedPerson         |     PID, IAM                               |
|     Practitioner          |     PV1, PV2, NTE,   IAM                   |
|     Location              |     PV1, PV2, EVN                          |
|     EpisodeOfCare         |     PV1                                    |
|     Encounter             |     PV1, ARV                               |
|     AllergyIntolerance    |     NTE, IAR, IAM                          |

**Extensions**:

|     FHIR Extension                                       |     Segment Mapped    |
|----------------------------------------------------------|-----------------------|
|     Patient-   PatientExtension                          |     PID, PD1, PV1     |
|     Encounter-   EncounterExtension                      |     PV1, PV2          |
|     AllergyIntolerance-   AllergyIntoleranceExtension    |     IAM               |

## ORU Message Types

### ORU_R01

**Usage:** The ORU message is for transmitting laboratory results to other systems. The OUL message is designed to accommodate the laboratory processes of laboratory automation systems.

**Mapped segment list**: MSH, SFT, PID, PD1, NK1, OBX, PV1, PV2, OBX, OBR, NTE, TQ1, CTD, ARV

**FHIR resource types and their mapped segments:**

|     FHIR Resource       |     Segment Mapped                                             |
|-------------------------|----------------------------------------------------------------|
|     MessageHeader       |     MSH, SFT                                                   |
|     Patient             |     PID, PD1, NK1,   PV1, CTD, ARV                             |
|     Provenance          |     MSH                                                        |
|     Organization        |     MSH, PID, PD1,   NK1, OBX, PV1, PV2, ORC, OBR, NTE, SPM    |
|     Device              |     MSH, SFT, OBX                                              |
|     Account             |     PID                                                        |
|     RelatedPerson       |     PID, NK1                                                   |
|     Practitioner        |     PD1, OBX, PV1,   PV2, ORC, OBR, NTE                        |
|     PractitionerRole    |     OBX, ORC, OBR                                              |
|     Location            |     PV1, PV2, OBR                                              |
|     EpisodeOfCare       |     PV1                                                        |
|     Encounter           |     PV1, PV2, ARV                                              |
|     ServiceRequest      |     ORC, OBR, TQ1                                              |
|     Observation         |     OBX, NTE                                                   |
|     DiagnosticReport    |     OBX, OBR                                                   |
|     Specimen            |     OBR, SPM                                                   |

**Extensions**:

| FHIR Extension                          | Segment Mapped |
|-----------------------------------------|----------------|
| Patient-   PatientExtension             | PID, PD1       |
| Encounter-   EncounterExtension         | PV1, PV2       |
| Observation-ObservationExtension        | OBX            |
| RelatedPerson-   RelatedPersonExtension | NK1            |
| Specimen- SpecimenExtension             | SPM            |

## BAR Message Types

### BAR_P01

**Usage:** This message is sent from a filler application to notify other applications that a new appointment has been booked. The information provided in the SCH segment and the other detail segments as appropriate describe the appointment that has been booked by the filler application.

**Mapped segment list**: MSH, SFT, EVN, PRT, ROL, DB1, AL1, DG1, GT1, IN1, IN2, ACC, PID, PD1, PV1, PV2, DG1

**FHIR resource types and their mapped segments:**

|     FHIR Resource         |     Segment Mapped                                                                 |
|---------------------------|------------------------------------------------------------------------------------|
|     MessageHeader         |     MSH, SFT                                                                       |
|     Patient               |     PID, PD1,   PRT, DB1, GT1, IN1, IN2                                            |
|     Provenance            |     MSH, EVN                                                                       |
|     Organization          |     MSH, PID, PD1,   PV1, PV2, DG1, EVN, PRT, ROL, DB1, DG1, GT1, IN1, IN2, ACC    |
|     Device                |     MSH, OBX,   SFT, PRT                                                           |
|     Account               |     PID, EVN, GT1                                                                  |
|     RelatedPerson         |     PID, GT1,   IN1, IN2                                                           |
|     Practitioner          |     NTE, PD1, PV1,   PV2, OBX, DG1, EVN, PRT, ROL, DG1, ACC                        |
|     PractitionerRole      |     PRT, ROL                                                                       |
|     Location              |     PV1, PV2,   EVN, PRT, ROL, ACC                                                 |
|     EpisodeOfCare         |     PV1, DG1                                                                       |
|     Encounter             |     PV1, PV2, DG1                                                                  |
|     Observation           |     ACC                                                                            |
|     AllergyIntolerance    |     AL1                                                                            |
|     Condition             |     DG1                                                                            |
|     Coverage              |     IN1, IN2                                                                       |

**Extensions**:

|     FHIR Extension                      |     Segment Mapped          |
|-----------------------------------------|-----------------------------|
|     Patient-   PatientExtension         |     PID, PD1, PV1,   DB1    |
|     Encounter-   EncounterExtension     |     PV1, PV2                |
|     Observation-ObservationExtension    |     ACC                     |
|     ConditionExtension                  |     DG1                     |

### BAR_P02

**Usage:** Generally, the elimination of all billing/accounts receivable records will be an internal function controlled, for example, by the patient accounting or financial system. However, on occasion, there will be a need to correct an account, or a series of accounts, that may require that a notice of account deletion be sent from another sub-system and processed, for example, by the patient accounting or financial system. Although a series of accounts may be purged within this one event, we recommend that only one PID segment be sent per event.

**Mapped segment list**: MSH, SFT, EVN, PID, PV1, PD1, DB1

**FHIR resource types and their mapped segments:**

|     FHIR Resource    |     Segment Mapped                    |
|----------------------|---------------------------------------|
|     MessageHeader    |     MSH, SFT                          |
|     Patient          |     PID, PD1, DB1                     |
|     Provenance       |     MSH, EVN                          |
|     Organization     |     MSH, PID,   PV1, EVN, PD1, DB1    |
|     Device           |     MSH, SFT                          |
|     Account          |     PID                               |
|     RelatedPerson    |     PID                               |
|     Practitioner     |     PV1, PD1                          |
|     Location         |     PV1, EVN                          |
|     EpisodeOfCare    |     PV1                               |
|     Encounter        |     PV1                               |

**Extensions**:

|     FHIR Extension                     |     Segment Mapped               |
|----------------------------------------|----------------------------------|
|     Patient-   PatientExtension        |     PID, PD1,   PV1, PD1, DB1    |
|     Encounter-   EncounterExtension    |     PV1                          |

### BAR_P12

**Usage:** The P12 event is used to communicate diagnosis and/or procedures in update mode. The newly created fields in DG1 and PR1, i.e., identifiers and action codes, must be populated to indicate which change should be applied. When other patient or visit related fields change, use the A08 (update patient information) event.

**Mapped segment list**: MSH, SFT, EVN, PID, PV1, DG1, PR1, PRT, ROL, OBX

**FHIR resource types and their mapped segments:**

|     FHIR Resource       |     Segment Mapped                              |
|-------------------------|-------------------------------------------------|
|     MessageHeader       |     MSH, SFT                                    |
|     Patient             |     PID                                         |
|     Provenance          |     MSH, EVN                                    |
|     Organization        |     MSH, PID,   PV1, EVN, DG1, PRT, ROL, OBX    |
|     Device              |     MSH, SFT, OBX                               |
|     Account             |     PID                                         |
|     RelatedPerson       |     PID                                         |
|     Practitioner        |     PV1, DG1,   PRT, ROL, OBX                   |
|     PractitionerRole    |     ROL, OBX                                    |
|     Location            |     PV1, EVN,   PRT, ROL                        |
|     EpisodeOfCare       |     PV1, DG1                                    |
|     Encounter           |     PV1, DG1                                    |
|     Condition           |     DG1                                         |
|     DiagnosticReport    |     OBX                                         |
|     Observation         |     OBX                                         |

**Extensions**:

|     FHIR Extension                       |     Segment Mapped          |
|------------------------------------------|-----------------------------|
|     Patient-   PatientExtension          |     PID, PD1,   PV1, DB1    |
|     Encounter-   EncounterExtension      |     PV1                     |
|     Observation- ObservationExtension    |     OBX                     |

## DFT Message Types

### DFT_P03

**Usage:** The Detail Financial Transaction (DFT) message is used to describe a financial transaction transmitted between systems, that is, to the billing system for ancillary charges, ADT to billing system for patient deposits, etc.

**Mapped segment list**: MSH, SFT, EVN, PID, PD1, PRT, ROL, PV1, PV2, DB1, ORC, TQ1, OBR, NTE, OBX, FT1, PR1, DG1, GT1, IN1, IN2, ACC

**FHIR resource types and their mapped segments:**

|     FHIR Resource         |     Segment Mapped                                                                                                 |
|---------------------------|--------------------------------------------------------------------------------------------------------------------|
|     MessageHeader         |     MSH, SFT                                                                                                       |
|     Patient               |     PID, PD1,   PRT, DB1, GT1, IN1, IN2                                                                            |
|     Provenance            |     MSH, EVN                                                                                                       |
|     Organization          |     MSH, PID, PD1,   PV1, PV2, OBX, DG1, EVN, PRT, ROL, DB1, DG1, GT1, IN1, IN2, ACC, ORC, OBR,   NTE, FT1, PR1    |
|     Device                |     MSH, OBX,   SFT, PRT, FT1                                                                                      |
|     Account               |     PID, EVN, GT1                                                                                                  |
|     RelatedPerson         |     PID, GT1,   IN1, IN2                                                                                           |
|     Practitioner          |     NTE, PD1, PV1,   PV2, OBX, DG1, EVN, PRT, ROL, DG1, ACC, ORC, OBR, NTE, FT1, PR1                               |
|     PractitionerRole      |     OBX, PRT, ROL,   ORC, OBR                                                                                      |
|     Location              |     PV1, PV2,   EVN, PRT, ROL, ACC, OBR, PR1                                                                       |
|     EpisodeOfCare         |     PV1, DG1                                                                                                       |
|     Encounter             |     PV1, PV2, DG1                                                                                                  |
|     Observation           |     OBX, ACC, NTE                                                                                                  |
|     AllergyIntolerance    |     AL1                                                                                                            |
|     Condition             |     DG1                                                                                                            |
|     Coverage              |     IN1, IN2                                                                                                       |
|     ServiceRequest        |     ORC, TQ1, OBR                                                                                                  |
|     DiagnosticReport      |     OBR, OBX                                                                                                       |
|     ChargeItem            |     NTE, FT1                                                                                                       |
|     Procedure             |     PR1                                                                                                            |

**Extensions**:

|     FHIR Extension                      |     Segment Mapped          |
|-----------------------------------------|-----------------------------|
|     Patient-   PatientExtension         |     PID, PD1, PV1,   DB1    |
|     Encounter-   EncounterExtension     |     PV1, PV2                |
|     Observation-ObservationExtension    |     OBX, ACC                |
|     ConditionExtension                  |     DG1                     |

### DFT_P11

**Usage:** The Detail Financial Transaction (DFT) - Expanded message is used to describe a financial transaction transmitted between systems, that is, to the billing system for ancillary charges, ADT to billing system for patient deposits, etc.

**Mapped segment list**: MSH, SFT, EVN, PID, PD1, PRT, ROL, PV1, PV2, DB1, ORC, TQ1, OBR, NTE, OBX, FT1, PR1, DG1, GT1, IN1, IN2, ACC

**FHIR resource types and their mapped segments:**

|     FHIR Resource       |     Segment Mapped                                                                                                 |
|-------------------------|--------------------------------------------------------------------------------------------------------------------|
|     MessageHeader       |     MSH, SFT                                                                                                       |
|     Patient             |     PID, PD1,   PRT, DB1, GT1, IN1, IN2                                                                            |
|     Provenance          |     MSH, EVN                                                                                                       |
|     Organization        |     MSH, PID,   PD1, PV1, PV2, OBX, DG1, EVN, PRT, ROL, DB1, DG1, GT1, IN1, IN2, ACC, ORC,   OBR, NTE, FT1, PR1    |
|     Device              |     MSH, OBX,   SFT, PRT, FT1                                                                                      |
|     Account             |     PID, EVN, GT1                                                                                                  |
|     RelatedPerson       |     PID, GT1,   IN1, IN2                                                                                           |
|     Practitioner        |     NTE, PD1,   PV1, PV2, OBX, DG1, EVN, PRT, ROL, DG1, ACC, ORC, OBR, NTE, FT1, PR1                               |
|     PractitionerRole    |     OBX, PRT,   ROL, ORC, OBR                                                                                      |
|     Location            |     PV1, PV2,   EVN, PRT, ROL, ACC, OBR, PR1                                                                       |
|     EpisodeOfCare       |     PV1, DG1                                                                                                       |
|     Encounter           |     PV1, PV2, DG1                                                                                                  |
|     Observation         |     OBX, ACC, NTE                                                                                                  |
|     Condition           |     DG1                                                                                                            |
|     Coverage            |     IN1, IN2                                                                                                       |
|     ServiceRequest      |     ORC, TQ1, OBR                                                                                                  |
|     DiagnosticReport    |     OBR, OBX                                                                                                       |
|     ChargeItem          |     NTE, FT1                                                                                                       |
|     Procedure           |     PR1                                                                                                            |

**Extensions**:

|     FHIR Extension                      |     Segment Mapped          |
|-----------------------------------------|-----------------------------|
|     Patient-   PatientExtension         |     PID, PD1,   PV1, DB1    |
|     Encounter-   EncounterExtension     |     PV1, PV2                |
|     Observation-ObservationExtension    |     OBX, ACC                |
|     ConditionExtension                  |     DG1                     |

## OMG Message Types

### OMG_O19

**Usage:** The function of this message is to initiate the transmission of information about a general clinical order that uses the OBR segment. OMG messages can originate also with a placer, filler, or an interested third party. The trigger event for this message is any change to a general clinical order. Such changes include submission of new orders, cancellations, updates, patient and non-patient-specific orders, etc.

**Mapped segment list**: MSH, SFT, PID, PD1, PRT, NK1, ARV, PV1, PV2, IN1, IN2, GT1, AL1, ORC, TQ1, OBR, NTE, DG1, OBX, SPM, SAC, FT1, BLG

**FHIR resource types and their mapped segments:**

|     FHIR Resource         |     Segment Mapped                                                                                  |
|---------------------------|-----------------------------------------------------------------------------------------------------|
|     MessageHeader         |     MSH, SFT                                                                                        |
|     Patient               |     PID, PD1,   PRT, GT1, IN1, IN2, NK1, ARV                                                        |
|     Provenance            |     MSH                                                                                             |
|     Organization          |     MSH, PID,   PD1, PV1, PV2, OBX, DG1, PRT, GT1, IN1, IN2, ORC, OBR, NTE, FT1, NK1, SPM,   BLG    |
|     Device                |     MSH, OBX,   SFT, PRT, FT1                                                                       |
|     Account               |     PID, GT1, BLG                                                                                   |
|     RelatedPerson         |     PID, GT1,   IN1, IN2, NK1                                                                       |
|     Practitioner          |     NTE, PD1,   PV1, PV2, OBX, DG1, PRT, ORC, OBR, NTE, FT1, SPM                                    |
|     PractitionerRole      |     OBX, PRT,   ORC, OBR                                                                            |
|     Location              |     PV1, PV2,   PRT, OBR                                                                            |
|     EpisodeOfCare         |     PV1, DG1                                                                                        |
|     Encounter             |     PV1, PV2, DG1                                                                                   |
|     Observation           |     OBX, NTE                                                                                        |
|     AllergyIntolerance    |     AL1                                                                                             |
|     Condition             |     DG1                                                                                             |
|     Coverage              |     IN1, IN2                                                                                        |
|     ServiceRequest        |     ORC, TQ1, OBR,   NTE                                                                            |
|     DiagnosticReport      |     OBR, OBX                                                                                        |
|     ChargeItem            |     FT1                                                                                             |
|     Specimen              |     SPM, SAC                                                                                        |

**Extensions**:

|     FHIR Extension                      |     Segment Mapped    |
|-----------------------------------------|-----------------------|
|     Patient-   PatientExtension         |     PID, PD1, PV1     |
|     Encounter-   EncounterExtension     |     PV1, PV2          |
|     Observation-ObservationExtension    |     OBX               |
|     ConditionExtension                  |     DG1               |
|     Specimen/SpecimenExtension          |     SPM               |

## SIU Message Types

### SIU_S12

**Usage:** This message is sent from a filler application to notify other applications that a new appointment has been booked. The information provided in the SCH segment and the other detail segments as appropriate describe the appointment that has been booked by the filler application.

**Mapped segment list**: MSH, SCH, TQ1, NTE, PID, PD1, PV1, PV2, OBX, DG1, AIS, AIG, AIL, AIP

**FHIR resource types and their mapped segments:**

|     FHIR Resource       |     Segment Mapped                                        |
|-------------------------|-----------------------------------------------------------|
|     MessageHeader       |     MSH                                                   |
|     Patient             |     PID, PD1                                              |
|     Provenance          |     MSH                                                   |
|     Organization        |     MSH, SCH, NTE,   PID, PD1, PV1, PV2, OBX, DG1, AIP    |
|     Device              |     MSH, OBX                                              |
|     Account             |     PID                                                   |
|     RelatedPerson       |     PID                                                   |
|     Practitioner        |     SCH, NTE, PD1,   PV1, PV2, OBX, DG1, AIP              |
|     PractitionerRole    |     SCH, OBX                                              |
|     Location            |     SCH, NTE, PV1,   PV2, AIL                             |
|     EpisodeOfCare       |     PV1                                                   |
|     Encounter           |     PV1, PV2                                              |
|     ServiceRequest      |     NTE, PID, AIS,   AIG, AIL, AIP, TQ1                   |
|     Observation         |     OBX                                                   |

**Extensions**:

|     FHIR Extension                      |     Segment Mapped    |
|-----------------------------------------|-----------------------|
|     Patient-   PatientExtension         |     PID, PD1, PV1     |
|     Encounter-   EncounterExtension     |     PV1, PV2          |
|     Observation-ObservationExtension    |     OBX               |

### SIU_S13

**Usage:** This message is sent from a filler application to notify other applications that an existing appointment has been rescheduled. The information in the SCH segment and the other detail segments as appropriate describe the new date(s) and time(s) to which the previously booked appointment has been moved. Additionally, it describes the unchanged information in the previously booked appointment.

**Mapped segment list**: MSH, SCH, TQ1, NTE, PID, PD1, PV1, PV2, OBX, DG1, AIS, AIG, AIL, AIP

**FHIR resource types and their mapped segments:**

|     FHIR Resource         |     Segment Mapped                                        |
|---------------------------|-----------------------------------------------------------|
|     MessageHeader         |     MSH                                                   |
|     Patient               |     PID, PD1                                              |
|     Provenance            |     MSH                                                   |
|     Organization          |     MSH, SCH,   NTE, PID, PD1, PV1, PV2, OBX, DG1, AIP    |
|     Device                |     MSH, NTE,   OBX, AIG                                  |
|     Account               |     PID                                                   |
|     RelatedPerson         |     PID                                                   |
|     Practitioner          |     SCH, NTE,   PD1, PV1, PV2, OBX, DG1, AIP              |
|     PractitionerRole      |     SCH, OBX                                              |
|     Location              |     SCH, NTE,   PV1, PV2, AIL                             |
|     EpisodeOfCare         |     PV1, DG1                                              |
|     Encounter             |     PV1, PV2, DG1                                         |
|     Appointment           |     SCH, TQ1,   NTE, PID, AIS, AIG, AIL, AIP              |
|     ServiceRequest        |     NTE, PID,   AIS, AIG, AIL, AIP, TQ1                   |
|     Observation           |     OBX                                                   |
|     Condition             |     DG1                                                   |

**Extensions**:

|     FHIR Extension                     |     Segment Mapped    |
|----------------------------------------|-----------------------|
|     Patient-   PatientExtension        |     PID, PD1, PV1     |
|     Encounter-   EncounterExtension    |     PV1, PV2          |
|     Condition-ConditionExtension       |     DG1               |

### SIU_S14

**Usage:** This message notifies other applications that an existing appointment has been modified on the filler application. This trigger event should only be used for appointments that have not been completed, or for parent appointments whose children have not been completed.

**Mapped segment list**: MSH, SCH, TQ1, NTE, PID, PD1, PV1, PV2, OBX, DG1, AIS, AIG, AIL, AIP

**FHIR resource types and their mapped segments:**

|     FHIR Resource         |     Segment Mapped                                        |
|---------------------------|-----------------------------------------------------------|
|     MessageHeader         |     MSH                                                   |
|     Patient               |     PID, PD1                                              |
|     Provenance            |     MSH                                                   |
|     Organization          |     MSH, SCH,   NTE, PID, PD1, PV1, PV2, OBX, DG1, AIP    |
|     Device                |     MSH, NTE,   OBX, AIG                                  |
|     Account               |     PID                                                   |
|     RelatedPerson         |     PID                                                   |
|     Practitioner          |     SCH, NTE,   PD1, PV1, PV2, OBX, DG1, AIP              |
|     PractitionerRole      |     SCH, OBX                                              |
|     Location              |     SCH, NTE,   PV1, PV2, AIL                             |
|     EpisodeOfCare         |     PV1, DG1                                              |
|     Encounter             |     PV1, PV2, DG1                                         |
|     Appointment           |     SCH, TQ1,   NTE, PID, AIS, AIG, AIL, AIP              |
|     ServiceRequest        |     NTE, PID,   AIS, AIG, AIL, AIP, TQ1                   |
|     Observation           |     OBX                                                   |
|     Condition             |     DG1                                                   |

**Extensions**:

|     FHIR Extension                     |     Segment Mapped    |
|----------------------------------------|-----------------------|
|     Patient-   PatientExtension        |     PID, PD1, PV1     |
|     Encounter-   EncounterExtension    |     PV1, PV2          |
|     Condition-ConditionExtension       |     DG1               |

### SIU_S15

**Usage:** A notification of appointment cancellation is sent by the filler application to other applications when an existing appointment has been canceled. A cancel event is used to stop a valid appointment from taking place. For example, if a patient scheduled for an exam cancels his/her appointment, then the appointment is canceled on the filler application.

**Mapped segment list**: MSH, SCH, TQ1, NTE, PID, PD1, PV1, PV2, OBX, DG1, AIS, AIG, AIL, AIP

**FHIR resource types and their mapped segments:**

|     FHIR Resource         |     Segment Mapped                                        |
|---------------------------|-----------------------------------------------------------|
|     MessageHeader         |     MSH                                                   |
|     Patient               |     PID, PD1                                              |
|     Provenance            |     MSH                                                   |
|     Organization          |     MSH, SCH,   NTE, PID, PD1, PV1, PV2, OBX, DG1, AIP    |
|     Device                |     MSH, NTE,   OBX, AIG                                  |
|     Account               |     PID                                                   |
|     RelatedPerson         |     PID                                                   |
|     Practitioner          |     SCH, NTE,   PD1, PV1, PV2, OBX, DG1, AIP              |
|     PractitionerRole      |     SCH, OBX                                              |
|     Location              |     SCH, NTE,   PV1, PV2, AIL                             |
|     EpisodeOfCare         |     PV1, DG1                                              |
|     Encounter             |     PV1, PV2, DG1                                         |
|     Appointment           |     SCH, TQ1,   NTE, PID, AIS, AIG, AIL, AIP              |
|     ServiceRequest        |     NTE, PID,   AIS, AIG, AIL, AIP, TQ1                   |
|     Observation           |     OBX                                                   |
|     Condition             |     DG1                                                   |

**Extensions**:

|     FHIR Extension                     |     Segment Mapped    |
|----------------------------------------|-----------------------|
|     Patient-   PatientExtension        |     PID, PD1, PV1     |
|     Encounter-   EncounterExtension    |     PV1, PV2          |
|     Condition-ConditionExtension       |     DG1               |

### SIU_S16

**Usage:** A notification of appointment discontinuation is sent by the filler application to notify other applications that an appointment in progress has been stopped, or that the remaining occurrences of a parent appointment will not occur. If none of the child appointments of a parent appointment have taken place, then a cancel trigger event should be sent instead.

**Mapped segment list**: MSH, SCH, TQ1, NTE, PID, PD1, PV1, PV2, OBX, DG1, AIS, AIG, AIL, AIP

**FHIR resource types and their mapped segments:**

|     FHIR Resource         |     Segment Mapped                                        |
|---------------------------|-----------------------------------------------------------|
|     MessageHeader         |     MSH                                                   |
|     Patient               |     PID, PD1                                              |
|     Provenance            |     MSH                                                   |
|     Organization          |     MSH, SCH,   NTE, PID, PD1, PV1, PV2, OBX, DG1, AIP    |
|     Device                |     MSH, NTE,   OBX, AIG                                  |
|     Account               |     PID                                                   |
|     RelatedPerson         |     PID                                                   |
|     Practitioner          |     SCH, NTE,   PD1, PV1, PV2, OBX, DG1, AIP              |
|     PractitionerRole      |     SCH, OBX                                              |
|     Location              |     SCH, NTE,   PV1, PV2, AIL                             |
|     EpisodeOfCare         |     PV1, DG1                                              |
|     Encounter             |     PV1, PV2, DG1                                         |
|     Appointment           |     SCH, TQ1,   NTE, PID, AIS, AIG, AIL, AIP              |
|     ServiceRequest        |     NTE, PID,   AIS, AIG, AIL, AIP, TQ1                   |
|     Observation           |     OBX                                                   |
|     Condition             |     DG1                                                   |

**Extensions**:

|     FHIR Extension                     |     Segment Mapped    |
|----------------------------------------|-----------------------|
|     Patient-   PatientExtension        |     PID, PD1, PV1     |
|     Encounter-   EncounterExtension    |     PV1, PV2          |
|     Condition-ConditionExtension       |     DG1               |

### SIU_S17

**Usage:** A notification of appointment deletion is sent by filler application to other applications when an appointment that had been entered in error has been removed from the system. A delete trigger event should only be used when an appointment has been erroneously scheduled. It must be removed from the schedule so that it does not affect any statistical processing. A delete trigger event differs from a cancel trigger event in that a delete acts to remove an error, whereas a cancel acts to prevent a valid request from occurring. This trigger event should not be used for any appointment that has already begun, or that has already been completed. Likewise, it should not be used for any parent appointment if any child appointments have either begun or been completed.

**Mapped segment list**: MSH, SCH, TQ1, NTE, PID, PD1, PV1, PV2, OBX, DG1, AIS, AIG, AIL, AIP

**FHIR resource types and their mapped segments:**

|     FHIR Resource         |     Segment Mapped                                        |
|---------------------------|-----------------------------------------------------------|
|     MessageHeader         |     MSH                                                   |
|     Patient               |     PID, PD1                                              |
|     Provenance            |     MSH                                                   |
|     Organization          |     MSH, SCH,   NTE, PID, PD1, PV1, PV2, OBX, DG1, AIP    |
|     Device                |     MSH, NTE,   OBX, AIG                                  |
|     Account               |     PID                                                   |
|     RelatedPerson         |     PID                                                   |
|     Practitioner          |     SCH, NTE,   PD1, PV1, PV2, OBX, DG1, AIP              |
|     PractitionerRole      |     SCH, OBX                                              |
|     Location              |     SCH, NTE,   PV1, PV2, AIL                             |
|     EpisodeOfCare         |     PV1, DG1                                              |
|     Encounter             |     PV1, PV2, DG1                                         |
|     Appointment           |     SCH, TQ1,   NTE, PID, AIS, AIG, AIL, AIP              |
|     ServiceRequest        |     NTE, PID,   AIS, AIG, AIL, AIP, TQ1                   |
|     Observation           |     OBX                                                   |
|     Condition             |     DG1                                                   |

**Extensions**:

|     FHIR Extension                     |     Segment Mapped    |
|----------------------------------------|-----------------------|
|     Patient-   PatientExtension        |     PID, PD1, PV1     |
|     Encounter-   EncounterExtension    |     PV1, PV2          |
|     Condition-ConditionExtension       |     DG1               |

### SIU_S26

**Usage:** A notification that a patient did not show up for an appointment. For example, if a patient was scheduled for a clinic visit, and never arrived for that appointment, this trigger event can be used to set a status on the appointment record for statistical purposes, as well as to free resources assigned to the appointment (or any other application-level actions that must be taken in the event a patient does not appear for an appointment).

**Mapped segment list**: MSH, SCH, TQ1, NTE, PID, PD1, PV1, PV2, OBX, DG1, AIS, AIG, AIL, AIP

**FHIR resource types and their mapped segments:**

|     FHIR Resource         |     Segment Mapped                                        |
|---------------------------|-----------------------------------------------------------|
|     MessageHeader         |     MSH                                                   |
|     Patient               |     PID, PD1                                              |
|     Provenance            |     MSH                                                   |
|     Organization          |     MSH, SCH,   NTE, PID, PD1, PV1, PV2, OBX, DG1, AIP    |
|     Device                |     MSH, NTE,   OBX, AIG                                  |
|     Account               |     PID                                                   |
|     RelatedPerson         |     PID                                                   |
|     Practitioner          |     SCH, NTE,   PD1, PV1, PV2, OBX, DG1, AIP              |
|     PractitionerRole      |     SCH, OBX                                              |
|     Location              |     SCH, NTE,   PV1, PV2, AIL                             |
|     EpisodeOfCare         |     PV1, DG1                                              |
|     Encounter             |     PV1, PV2, DG1                                         |
|     Appointment           |     SCH, TQ1,   NTE, PID, AIS, AIG, AIL, AIP              |
|     ServiceRequest        |     NTE, PID,   AIS, AIG, AIL, AIP, TQ1                   |
|     Observation           |     OBX                                                   |
|     Condition             |     DG1                                                   |

**Extensions**:

|     FHIR Extension                     |     Segment Mapped    |
|----------------------------------------|-----------------------|
|     Patient-   PatientExtension        |     PID, PD1, PV1     |
|     Encounter-   EncounterExtension    |     PV1, PV2          |
|     Condition-ConditionExtension       |     DG1               |

## MDM Message Types

### MDM_T01

**Usage:** This is a notification of the creation of a document without the accompanying content. There are multiple approaches by which systems become aware of documents:

* Scenario A: A document is dictated, and chart tracking system is notified that it has been dictated and is awaiting transcription.
* Scenario B: Dictation is transcribed, and chart tracking system is notified that the document exists and requires authentication.
* Scenario C: A provider orders a series of three X-rays. The radiologist dictates a single document which covers all three orders. Multiple placer numbers are used to identify each of these orders.

**Mapped segment list**: MSH, SFT, EVN, PID, PV1, TXA, ORC, OBR, NTE, TQ1, CON, ORC

**FHIR resource types and their mapped segments:**

|     FHIR Resource        |     Segment Mapped                         |
|--------------------------|--------------------------------------------|
|     MessageHeader        |     MSH, SFT                               |
|     Patient              |     PID, CON                               |
|     Provenance           |     MSH, EVN                               |
|     Organization         |     MSH, EVN, PID,   PV1, TXA, ORC, NTE    |
|     Device               |     MSH, SFT                               |
|     Account              |     PID                                    |
|     RelatedPerson        |     PID, CON                               |
|     Practitioner         |     EVN, PV1, TXA,   ORC, OBR, NTE, CON    |
|     PractitionerRole     |     TXA, ORC, OBR                          |
|     Location             |     EVN, PV1, OBR                          |
|     EpisodeOfCare        |     PV1                                    |
|     Encounter            |     PV1                                    |
|     ServiceRequest       |     ORC, OBR,   NTE, TQ1                   |
|     DocumentReference    |     TXA                                    |
|     Specimen             |     OBR                                    |
|     Consent              |     CON                                    |

**Extensions**:

|     FHIR Extension                     |     Segment Mapped    |
|----------------------------------------|-----------------------|
|     Patient- PatientExtension          |     PID, PV1          |
|     Encounter-   EncounterExtension    |     PV1               |

### MDM_T02

**Usage:** This is a notification of the creation of a document with the accompanying content.

* Scenario A: Dictation is transcribed, and the chart tracking system is notified that the document exists and requires authentication. The content of the document is transmitted along with the notification.
* Scenario B: A provider orders a series of three X-rays. The radiologist's dictation is transcribed in a single document, which covers all three orders. Multiple placer numbers are used to identify each of the orders within the single document message. The notification and document content are transmitted.

**Mapped segment list**: MSH, SFT, EVN, PID, PV1, TXA, ORC, OBR, NTE, TQ1, OBX, CON

**FHIR resource types and their mapped segments:**

|     FHIR Resource        |     Segment Mapped                              |
|--------------------------|-------------------------------------------------|
|     MessageHeader        |     MSH, SFT                                    |
|     Patient              |     PID, CON                                    |
|     Provenance           |     MSH, EVN                                    |
|     Organization         |     MSH, EVN,   PID, PV1, TXA, ORC, NTE, OBX    |
|     Device               |     MSH, SFT, OBX                               |
|     Account              |     PID                                         |
|     RelatedPerson        |     PID, CON                                    |
|     Practitioner         |     EVN, PV1,   TXA, ORC, OBR, NTE, OBX         |
|     PractitionerRole     |     TXA, ORC, OBR,   OBX                        |
|     Location             |     EVN, PV1, OBR                               |
|     EpisodeOfCare        |     PV1                                         |
|     Encounter            |     PV1                                         |
|     ServiceRequest       |     ORC, OBR,   NTE, TQ1                        |
|     Observation          |     OBX                                         |
|     DocumentReference    |     TXA                                         |
|     DiagnosticReport     |     OBX                                         |
|     Specimen             |     OBR                                         |
|     Consent              |     CON                                         |

**Extensions**:

|     FHIR Extension                      |     Segment Mapped    |
|-----------------------------------------|-----------------------|
|     Patient-   PatientExtension         |     PID, PV1          |
|     Encounter-   EncounterExtension     |     PV1               |
|     Observation-ObservationExtension    |     OBX               |

### MDM_T05

**Usage:** This is a notification of an addendum to a document without the accompanying content. Scenario: Author dictates additional information as an addendum to a previously transcribed document. A new document is transcribed. This addendum has its own new unique document ID that is linked to the original document via the parent ID. Addendum document notification is transmitted. This creates a composite document.

**Mapped segment list**: MSH, SFT, EVN, PID, PV1, TXA, ORC, OBR, NTE, TQ1, CON, ORC

**FHIR resource types and their mapped segments:**

|     FHIR Resource        |     Segment Mapped                         |
|--------------------------|--------------------------------------------|
|     MessageHeader        |     MSH, SFT                               |
|     Patient              |     PID, CON                               |
|     Provenance           |     MSH, EVN                               |
|     Organization         |     MSH, EVN, PID,   PV1, TXA, ORC, NTE    |
|     Device               |     MSH, SFT                               |
|     Account              |     PID                                    |
|     RelatedPerson        |     PID, CON                               |
|     Practitioner         |     EVN, PV1, TXA,   ORC, OBR, NTE, CON    |
|     PractitionerRole     |     TXA, ORC, OBR                          |
|     Location             |     EVN, PV1, OBR                          |
|     EpisodeOfCare        |     PV1                                    |
|     Encounter            |     PV1                                    |
|     ServiceRequest       |     ORC, OBR,   NTE, TQ1                   |
|     DocumentReference    |     TXA                                    |
|     Specimen             |     OBR                                    |
|     Consent              |     CON                                    |

**Extensions**:

|     FHIR Extension                     |     Segment Mapped    |
|----------------------------------------|-----------------------|
|     Patient-   PatientExtension        |     PID, PV1          |
|     Encounter-   EncounterExtension    |     PV1               |

### MDM_T06

**Usage:** This is a notification of an addendum to a document with the accompanying content. Scenario: Author dictates additional information as an addendum to a previously transcribed document. A new document is transcribed. This addendum has its own new unique document ID that is linked to the original document via the parent ID. Addendum document notification is transmitted, along with the document content. This creates a composite document.

**Mapped segment list**: MSH, SFT, EVN, PID, PV1, TXA, ORC, OBR, NTE, TQ1, OBX, CON

**FHIR resource types and their mapped segments:**

|     FHIR Resource        |     Segment Mapped                              |
|--------------------------|-------------------------------------------------|
|     MessageHeader        |     MSH, SFT                                    |
|     Patient              |     PID, CON                                    |
|     Provenance           |     MSH, EVN                                    |
|     Organization         |     MSH, EVN,   PID, PV1, TXA, ORC, NTE, OBX    |
|     Device               |     MSH, SFT, OBX                               |
|     Account              |     PID                                         |
|     RelatedPerson        |     PID, CON                                    |
|     Practitioner         |     EVN, PV1,   TXA, ORC, OBR, NTE, OBX         |
|     PractitionerRole     |     TXA, ORC, OBR,   OBX                        |
|     Location             |     EVN, PV1, OBR                               |
|     EpisodeOfCare        |     PV1                                         |
|     Encounter            |     PV1                                         |
|     ServiceRequest       |     ORC, OBR,   NTE, TQ1                        |
|     Observation          |     OBX                                         |
|     DocumentReference    |     TXA                                         |
|     DiagnosticReport     |     OBX                                         |
|     Specimen             |     OBR                                         |
|     Consent              |     CON                                         |

**Extensions**:

|     FHIR Extension                      |     Segment Mapped    |
|-----------------------------------------|-----------------------|
|     Patient-   PatientExtension         |     PID, PV1          |
|     Encounter-   EncounterExtension     |     PV1               |
|     Observation-ObservationExtension    |     OBX               |

### MDM_T09

**Usage:** This is a notification of replacement to a document without the accompanying content. Scenario: Errors discovered in a document are corrected. The original document is replaced with the revised document. The replacement document has its own new unique document ID that is linked to the original document via the parent ID. The availability status of the original document is changed to "Obsolete", but the original document should be retained in the system for historical reference. Document replacement notification is sent.

**Mapped segment list**: MSH, SFT, EVN, PID, PV1, TXA, ORC, OBR, NTE, TQ1, CON, ORC

**FHIR resource types and their mapped segments:**

|     FHIR Resource        |     Segment Mapped                         |
|--------------------------|--------------------------------------------|
|     MessageHeader        |     MSH, SFT                               |
|     Patient              |     PID, CON                               |
|     Provenance           |     MSH, EVN                               |
|     Organization         |     MSH, EVN, PID,   PV1, TXA, ORC, NTE    |
|     Device               |     MSH, SFT                               |
|     Account              |     PID                                    |
|     RelatedPerson        |     PID, CON                               |
|     Practitioner         |     EVN, PV1, TXA,   ORC, OBR, NTE, CON    |
|     PractitionerRole     |     TXA, ORC, OBR                          |
|     Location             |     EVN, PV1, OBR                          |
|     EpisodeOfCare        |     PV1                                    |
|     Encounter            |     PV1                                    |
|     ServiceRequest       |     ORC, OBR,   NTE, TQ1                   |
|     DocumentReference    |     TXA                                    |
|     Specimen             |     OBR                                    |
|     Consent              |     CON                                    |

**Extensions**:

|     FHIR Extension                     |     Segment Mapped    |
|----------------------------------------|-----------------------|
|     Patient-   PatientExtension        |     PID, PV1          |
|     Encounter-   EncounterExtension    |     PV1               |

### MDM_T10
 
**Usage:** This is an event about Medical Records/Information Management. Scenario: Errors discovered in a document are corrected. The original document is replaced with the revised document. The replacement document has its own new unique document ID that is linked to the original document via the parent ID. The availability status of the original document is changed to "Obsolete" but the original document should be retained in the system for historical reference. Document replacement notification and document content are sent.
 
**Mapped segment list**: MSH, SFT, EVN, PID, PV1, TXA, ORC, OBR, NTE, TQ1, OBX, CON

**FHIR resource types and their mapped segments:**

|     FHIR Resource        |     Segment Mapped                              |
|--------------------------|-------------------------------------------------|
|     MessageHeader        |     MSH, SFT                                    |
|     Patient              |     PID, CON                                    |
|     Provenance           |     MSH, EVN                                    |
|     Organization         |     MSH, EVN,   PID, PV1, TXA, ORC, NTE, OBX    |
|     Device               |     MSH, SFT, OBX                               |
|     Account              |     PID                                         |
|     RelatedPerson        |     PID, CON                                    |
|     Practitioner         |     EVN, PV1,   TXA, ORC, OBR, NTE, OBX         |
|     PractitionerRole     |     TXA, ORC, OBR,   OBX                        |
|     Location             |     EVN, PV1, OBR                               |
|     EpisodeOfCare        |     PV1                                         |
|     Encounter            |     PV1                                         |
|     ServiceRequest       |     ORC, OBR,   NTE, TQ1                        |
|     Observation          |     OBX                                         |
|     DocumentReference    |     TXA                                         |
|     DiagnosticReport     |     OBX                                         |
|     Specimen             |     OBR                                         |
|     Consent              |     CON                                         |

**Extensions**:

|     FHIR Extension                      |     Segment Mapped    |
|-----------------------------------------|-----------------------|
|     Patient-   PatientExtension         |     PID, PV1          |
|     Encounter-   EncounterExtension     |     PV1               |
|     Observation-ObservationExtension    |     OBX               |

## ORM Message Types

### ORM_O01

**Usage:** This is a General Order Message. The function of this message is to initiate the transmission of information about an order. This includes placing new orders, cancellation of existing orders, discontinuation, holding, etc. ORM messages can originate also with a placer, filler, or an interested third party.

**Mapped segment list**: MSH, PID, PD1, PV1, PV2, IN1, IN2, GT1, AL1, ORC, OBR, NTE, CTD, DG1, OBX, ODS, RQ1, RXO

**FHIR resource types and their mapped segments:**

|     FHIR Resource         |     Segment Mapped                                                                      |
|---------------------------|-----------------------------------------------------------------------------------------|
|     MessageHeader         |     MSH                                                                                 |
|     Patient               |     PID, PD1, IN1,   IN2, GT1, CTD                                                      |
|     Provenance            |     MSH                                                                                 |
|     Organization          |     MSH, PID, PD1,   PV1, PV2, IN1, IN2, GT1, ORC, OBR, NTE, DG1, OBX, RQ1, RQD, RXI    |
|     Device                |     MSH, OBX                                                                            |
|     Account               |     PID, IN2                                                                            |
|     RelatedPerson         |     PID, IN1, IN2,   IN2                                                                |
|     Practitioner          |     PD1, PV1, PV2,   ORC, OBR, NTE, DG1, OBX, RXO                                       |
|     PractitionerRole      |     ORC, OBR, OBX,   RXO                                                                |
|     Location              |     PV1, PV2                                                                            |
|     EpisodeOfCare         |     PV1                                                                                 |
|     Encounter             |     PV1, PV2, DG1                                                                       |
|     ServiceRequest        |     ORC, OBR, NTE                                                                       |
|     Specimen              |     OBR                                                                                 |
|     Coverage              |     IN1, IN2                                                                            |
|     AllergyIntolerance    |     AL1                                                                                 |
|     Condition             |     DG1                                                                                 |
|     DiagnosticReport      |     OBX                                                                                 |
|     Observation           |     OBX                                                                                 |
|     NutritionOrder        |     ODS                                                                                 |
|     SupplyRequest         |     RQ1, RQD                                                                            |
|     MedicationRequest     |     RXO                                                                                 |
|     Medication            |     RXO                                                                                 |

**Extensions**:

|     FHIR Extension                     |     Segment Mapped    |
|----------------------------------------|-----------------------|
|     Patient-   PatientExtension        |     PID, PD1, PV1     |
|     Encounter-   EncounterExtension    |     PV1, PV2          |
|     Condition-   ConditionExtension    |     DG1               |

## OML Message Types 

### OML_O21

**Usage:** This event is about Laboratory order. The OML-O21 message structure may be used for the communication of laboratory and other order messages and must be used for lab automation messages where it is required that the Specimen/Container information is within the ORC/OBR segment group. The trigger event for this message is any change to a laboratory order. Such changes include submission of new orders, cancellations, updates, etc. OML messages can originate also with a placer, filler, or an interested third party.

**Mapped segment list**: MSH, SFT, PID, PD1, PV1, PV2, IN1, IN2, GT1, AL1, ORC, TQ1, OBR, NTE, CTD, DG1, OBX, ARV, SAC, SPM

**FHIR resource types and their mapped segments:**

|     FHIR Resource         |     Segment Mapped                                                                 |
|---------------------------|------------------------------------------------------------------------------------|
|     MessageHeader         |     MSH, SFT                                                                       |
|     Patient               |     PID, PD1, NK1,   IN1, IN2, GT1, CTD, ARV                                       |
|     Provenance            |     MSH                                                                            |
|     Organization          |     MSH, PID, PD1,   NK1, PV1, PV2, IN1, IN2, GT1, ORC, OBR, NTE, DG1, OBX, SPM    |
|     Device                |     MSH, SFT, OBX                                                                  |
|     Account               |     PID, GT1                                                                       |
|     RelatedPerson         |     PID, NK1, IN1,   IN2, GT1                                                      |
|     Practitioner          |     PD1, PV1, PV2,   ORC, OBR, NTE, DG1, OBX                                       |
|     PractitionerRole      |     ORC, OBR, OBX                                                                  |
|     Location              |     PV1, PV2                                                                       |
|     EpisodeOfCare         |     PV1, DG1                                                                       |
|     Encounter             |     PV1, PV2, DG1,   ARV                                                           |
|     ServiceRequest        |     ORC, TQ1, OBR                                                                  |
|     Specimen              |     OBR, SPM, SAC                                                                  |
|     Coverage              |     IN1, IN2                                                                       |
|     AllergyIntolerance    |     AL1                                                                            |
|     Condition             |     DG1                                                                            |
|     DiagnosticReport      |     OBX                                                                            |
|     Observation           |     OBX, NTE                                                                       |

**Extensions**:

|     FHIR Extension                                   |     Segment Mapped    |
|------------------------------------------------------|-----------------------|
|     Patient-   PatientExtension                      |     PID, PD1, PV1     |
|     Encounter-   EncounterExtension                  |     PV1, PV2          |
|     Condition-   ConditionExtension                  |     DG1               |
|     RelatedPerson-   RelatedPersonExtension          |     NK1               |
|     Specimen-   SpecimenExtension                    |     SPM, SAC          |

## OUL Message Types

### OUL_R22

**Usage:** This message was designed to accommodate specimen-oriented testing. It should be applicable to container-less testing (e.g., elephant on a table) and laboratory automation systems requiring container.

**Mapped segment list**: MSH, SFT, PID, PD1, PV1, PV2, NK1, ORC, TQ1, OBR, NTE, OBX, ARV, SAC, SPM, PRT, INV, TXA, CTI

**FHIR resource types and their mapped segments:**

|     FHIR Resource        |     Segment Mapped                                                       |
|--------------------------|--------------------------------------------------------------------------|
|     MessageHeader        |     MSH, SFT                                                             |
|     Patient              |     PID, PD1, NK1,   ARV                                                 |
|     Provenance           |     MSH                                                                  |
|     Organization         |     MSH, PID, PD1,   NK1, PV1, PV2, ORC, OBR, NTE, OBX, SPM, PRT, TXA    |
|     Device               |     MSH, SFT,   OBX, PRT                                                 |
|     Account              |     PID                                                                  |
|     RelatedPerson        |     PID, NK1                                                             |
|     Practitioner         |     PD1, PV1, PV2,   ORC, OBR, NTE, OBX, PRT, TXA                        |
|     PractitionerRole     |     ORC, OBR,   OBX, PRT, TXA                                            |
|     Location             |     PV1, PV2, PRT                                                        |
|     EpisodeOfCare        |     PV1                                                                  |
|     Encounter            |     PV1, PV2, ARV                                                        |
|     ServiceRequest       |     ORC, TQ1,   OBR, NTE                                                 |
|     Specimen             |     OBR, SPM, SAC                                                        |
|     DiagnosticReport     |     OBX                                                                  |
|     Observation          |     OBX, NTE                                                             |
|     Substance            |     INV                                                                  |
|     DocumentReference    |     TXA                                                                  |
|     ResearchStudy        |     CTI                                                                  |

**Extensions**:

| FHIR Extension                          | Segment Mapped |
|-----------------------------------------|----------------|
| Patient-   PatientExtension             | PID, PD1, PV1  |
| Encounter-   EncounterExtension         | PV1, PV2       |
| RelatedPerson-   RelatedPersonExtension | NK1            |
| Specimen- SpecimenExtension             | SPM, SAC       |

### OUL_R23

**Usage:** This message was designed to accommodate specimen-oriented testing. It should be applicable to, for example, laboratory automation systems requiring container.

**Mapped segment list**: MSH, SFT, PID, PD1, PV1, PV2, NK1, ORC, TQ1, OBR, NTE, OBX, ARV, SAC, SPM, PRT, INV, TXA, CTI

**FHIR resource types and their mapped segments:**

|     FHIR Resource        |     Segment Mapped                                                       |
|--------------------------|--------------------------------------------------------------------------|
|     MessageHeader        |     MSH, SFT                                                             |
|     Patient              |     PID, PD1, NK1,   ARV                                                 |
|     Provenance           |     MSH                                                                  |
|     Organization         |     MSH, PID, PD1,   NK1, PV1, PV2, ORC, OBR, NTE, OBX, SPM, PRT, TXA    |
|     Device               |     MSH, SFT,   OBX, PRT                                                 |
|     Account              |     PID                                                                  |
|     RelatedPerson        |     PID, NK1                                                             |
|     Practitioner         |     PD1, PV1, PV2,   ORC, OBR, NTE, OBX, PRT, TXA                        |
|     PractitionerRole     |     ORC, OBR,   OBX, PRT, TXA                                            |
|     Location             |     PV1, PV2, PRT                                                        |
|     EpisodeOfCare        |     PV1                                                                  |
|     Encounter            |     PV1, PV2, ARV                                                        |
|     ServiceRequest       |     ORC, TQ1,   OBR, NTE                                                 |
|     Specimen             |     OBR, SPM, SAC                                                        |
|     DiagnosticReport     |     OBX                                                                  |
|     Observation          |     OBX, NTE                                                             |
|     Substance            |     INV                                                                  |
|     DocumentReference    |     TXA                                                                  |
|     ResearchStudy        |     CTI                                                                  |

**Extensions**:

|     FHIR Extension                                   |     Segment Mapped    |
|------------------------------------------------------|-----------------------|
|     Patient- PatientExtension                        |     PID, PD1, PV1     |
|     Encounter-   EncounterExtension                  |     PV1, PV2          |
|     RelatedPerson-   RelatedPersonExtension          |     NK1               |
|     Specimen-   SpecimenExtension                    |     SPM, SAC          |

### OUL_R24

**Usage:** This message was designed to accommodate multi-specimen-oriented testing. It should be applicable to, e.g., laboratory automation systems requiring container.

**Mapped segment list**: MSH, SFT, PID, PD1, PV1, PV2, NK1, ORC, TQ1, OBR, NTE, OBX, ARV, SAC, SPM, PRT, INV, TXA, CTI

**FHIR resource types and their mapped segments:**

|     FHIR Resource        |     Segment Mapped                                                       |
|--------------------------|--------------------------------------------------------------------------|
|     MessageHeader        |     MSH, SFT                                                             |
|     Patient              |     PID, PD1, NK1,   ARV                                                 |
|     Provenance           |     MSH                                                                  |
|     Organization         |     MSH, PID, PD1,   NK1, PV1, PV2, ORC, OBR, NTE, OBX, SPM, PRT, TXA    |
|     Device               |     MSH, SFT,   OBX, PRT                                                 |
|     Account              |     PID                                                                  |
|     RelatedPerson        |     PID, NK1                                                             |
|     Practitioner         |     PD1, PV1, PV2,   ORC, OBR, NTE, OBX, PRT, TXA                        |
|     PractitionerRole     |     ORC, OBR,   OBX, PRT, TXA                                            |
|     Location             |     PV1, PV2, PRT                                                        |
|     EpisodeOfCare        |     PV1                                                                  |
|     Encounter            |     PV1, PV2, ARV                                                        |
|     ServiceRequest       |     ORC, TQ1,   OBR, NTE                                                 |
|     Specimen             |     OBR, SPM, SAC                                                        |
|     DiagnosticReport     |     OBX                                                                  |
|     Observation          |     OBX, NTE                                                             |
|     Substance            |     INV                                                                  |
|     DocumentReference    |     TXA                                                                  |
|     ResearchStudy        |     CTI                                                                  |

**Extensions**:

|     FHIR Extension                                   |     Segment Mapped    |
|------------------------------------------------------|-----------------------|
|     Patient-   PatientExtension                      |     PID, PD1, PV1     |
|     Encounter-   EncounterExtension                  |     PV1, PV2          |
|     RelatedPerson-   RelatedPersonExtension          |     NK1               |
|     Specimen-   SpecimenExtension                    |     SPM, SAC          |

## RDS Message Types

### RDS_O13

**Usage:** The RDS message may be created by the pharmacy/treatment application for each instance of dispensing a drug or treatment to fill an existing order or orders. In the most common case, the RDS messages would be routed to a Nursing application or to some clinical application, which needs the data about drugs dispensed or treatments given. As a site-specific variant, the original order segments (RXO, RXE and their associated RXR/RXCs) may be sent optionally (for comparison).

**Mapped segment list**: MSH, SFT, PID, PD1, PV1, PV2, ORC, TQ1, NTE, OBX, ARV, PRT, AL1, RXO, RXR, RXC, RXE, RXD, CDO

**FHIR resource types and their mapped segments:**


|     FHIR Resource         |     Segment Mapped                                             |
|---------------------------|----------------------------------------------------------------|
|     MessageHeader         |     MSH, SFT                                                   |
|     Patient               |     PID, PD1, ARV                                              |
|     Provenance            |     MSH                                                        |
|     Organization          |     MSH, PID, PD1,   PV1, PV2, ORC, NTE, OBX, PRT, RXO, RXE    |
|     Device                |     MSH, SFT,   OBX, PRT                                       |
|     Account               |     PID                                                        |
|     RelatedPerson         |     PID                                                        |
|     Practitioner          |     PD1, PV1, PV2,   ORC, NTE, OBX, PRT, RXO, RXE              |
|     PractitionerRole      |     ORC, OBX,   PRT, RXO, RXE                                  |
|     Location              |     PV1, PV2,   PRT, RXD                                       |
|     EpisodeOfCare         |     PV1                                                        |
|     Encounter             |     PV1, PV2, ARV                                              |
|     Specimen              |     OBR                                                        |
|     AllergyIntolerance    |     AL1                                                        |
|     DiagnosticReport      |     OBX                                                        |
|     Observation           |     OBX, NTE                                                   |
|     MedicationRequest     |     ORC, TQ1,   NTE, RXO, RXR, RXE                             |
|     MedicationDispense    |     ORC, NTE,   RXR, RXE, RXD, CDO                             |
|     Medication            |     RXO, RXC,   RXE, RXD                                       |

**Extensions**:

|     FHIR Extension                               |     Segment Mapped    |
|--------------------------------------------------|-----------------------|
|     Patient-   PatientExtension                  |     PID, PD1, PV1     |
|     Encounter-   EncounterExtension              |     PV1, PV2          |
|     Observation-   ObservationExtension          |     OBX               |

## RDE Message Types

### RDE_O11

**Usage:** This message communicates the pharmacy or treatment application's encoding of the pharmacy/treatment order, OMP, message. It may be sent as an unsolicited message to report on either a single order or multiple pharmacy/treatment orders for a patient.

**Mapped segment list**: MSH, SFT, PID, PD1, PV1, PV2, ORC, TQ1, NTE, OBX, ARV, PRT, AL1, RXO, RXR, RXC, RXE, CDO

**FHIR resource types and their mapped segments:**

|     FHIR Resource         |     Segment Mapped                                             |
|---------------------------|----------------------------------------------------------------|
|     MessageHeader         |     MSH, SFT                                                   |
|     Patient               |     PID, PD1, ARV                                              |
|     Provenance            |     MSH                                                        |
|     Organization          |     MSH, PID, PD1,   PV1, PV2, ORC, NTE, OBX, PRT, RXO, RXE    |
|     Device                |     MSH, SFT,   OBX, PRT                                       |
|     Account               |     PID                                                        |
|     RelatedPerson         |     PID                                                        |
|     Practitioner          |     PD1, PV1, PV2,   ORC, NTE, OBX, PRT, RXO, RXE              |
|     PractitionerRole      |     ORC, OBX,   PRT, RXO, RXE                                  |
|     Location              |     PV1, PV2, PRT                                              |
|     EpisodeOfCare         |     PV1                                                        |
|     Encounter             |     PV1, PV2, ARV                                              |
|     Specimen              |     OBR                                                        |
|     AllergyIntolerance    |     AL1                                                        |
|     DiagnosticReport      |     OBX                                                        |
|     Observation           |     OBX, NTE                                                   |
|     MedicationRequest     |     ORC, TQ1,   NTE, RXO, RXR, RXE, CDO                        |
|     Medication            |     RXO, RXC, RXE                                              |

**Extensions**:

|     FHIR Extension                               |     Segment Mapped    |
|--------------------------------------------------|-----------------------|
|     Patient-   PatientExtension                  |     PID, PD1, PV1     |
|     Encounter-   EncounterExtension              |     PV1, PV2          |
|     Observation-   ObservationExtension          |     OBX               |

### RDE_O25

**Usage:** The RDE/RRE is used to communicate a refill authorization request originating with the pharmacy. This message replicates the standard RDE message with a different trigger event code to indicate the specific use case of a refill authorization request.

**Mapped segment list**: MSH, SFT, PID, PD1, PV1, PV2, ORC, TQ1, NTE, OBX, ARV, PRT, AL1, RXO, RXR, RXC, RXE, CDO, GT1, IN1, IN2

**FHIR resource types and their mapped segments:**


|     FHIR Resource         |     Segment Mapped                                                            |
|---------------------------|-------------------------------------------------------------------------------|
|     MessageHeader         |     MSH, SFT                                                                  |
|     Patient               |     PID, PD1,   ARV, GT1, IN2                                                 |
|     Provenance            |     MSH                                                                       |
|     Organization          |     MSH, PID, PD1,   PV1, PV2, ORC, NTE, OBX, PRT, RXO, RXE, GT1, IN1, IN2    |
|     Device                |     MSH, SFT,   OBX, PRT                                                      |
|     Account               |     PID, GT1                                                                  |
|     RelatedPerson         |     PID, GT1, IN2                                                             |
|     Practitioner          |     PD1, PV1, PV2,   ORC, NTE, OBX, PRT, RXO, RXE                             |
|     PractitionerRole      |     ORC, OBX,   PRT, RXO, RXE                                                 |
|     Location              |     PV1, PV2, PRT                                                             |
|     EpisodeOfCare         |     PV1                                                                       |
|     Encounter             |     PV1, PV2, ARV                                                             |
|     Specimen              |     OBR                                                                       |
|     Coverage              |     IN1, IN2                                                                  |
|     AllergyIntolerance    |     AL1                                                                       |
|     DiagnosticReport      |     OBX                                                                       |
|     Observation           |     OBX, NTE                                                                  |
|     MedicationRequest     |     ORC, TQ1,   NTE, RXO, RXR, RXE, CDO                                       |
|     Medication            |     RXO, RXC, RXE                                                             |

**Extensions**:

|     FHIR Extension                               |     Segment Mapped    |
|--------------------------------------------------|-----------------------|
|     Patient-   PatientExtension                  |     PID, PD1, PV1     |
|     Encounter-   EncounterExtension              |     PV1, PV2          |
|     Observation-   ObservationExtension          |     OBX               |

## VXU Message Types

### VXU_V04

**Usage:** When a provider wishes to update the patient's vaccination record being held in a registry, he will transmit an unsolicited update of the record (a V04 trigger event). An unsolicited update will follow this format. The three-letter code in the leftmost column indicates the segment that is included; the column on the right specifies the chapter in which that segment is fully defined.

**Mapped segment list**: MSH, SFT, PID, PD1, PV1, PV2, ORC, TQ1, NTE, OBX, ARV, PRT, RXR, GT1, IN1, IN2, RXA, NK1

**FHIR resource types and their mapped segments:**

|     FHIR Resource       |     Segment Mapped                                                            |
|-------------------------|-------------------------------------------------------------------------------|
|     MessageHeader       |     MSH, SFT                                                                  |
|     Patient             |     PID, PD1,   ARV, GT1, IN2, NK1                                            |
|     Provenance          |     MSH                                                                       |
|     Organization        |     MSH, PID, PD1,   PV1, PV2, ORC, NTE, OBX, PRT, GT1, IN1, IN2, RXA, NK1    |
|     Device              |     MSH, SFT,   OBX, PRT                                                      |
|     Account             |     PID, GT1                                                                  |
|     RelatedPerson       |     PID, GT1,   IN2, NK1                                                      |
|     Practitioner        |     PD1, PV1, PV2,   ORC, NTE, OBX, PRT, RXA                                  |
|     PractitionerRole    |     ORC, OBX, PRT                                                             |
|     Location            |     PV1, PV2,   PRT, RXA                                                      |
|     EpisodeOfCare       |     PV1                                                                       |
|     Encounter           |     PV1, PV2, ARV                                                             |
|     ServiceRequest      |     ORC, TQ1                                                                  |
|     Specimen            |     OBR                                                                       |
|     Coverage            |     IN1, IN2                                                                  |
|     DiagnosticReport    |     OBX                                                                       |
|     Observation         |     OBX, NTE                                                                  |
|     Immunization        |     ORC, NTE,   RXR, RXA                                                      |

**Extensions**:

|     FHIR Extension                               |     Segment Mapped    |
|--------------------------------------------------|-----------------------|
|     Patient-   PatientExtension                  |     PID, PD1, PV1     |
|     Encounter-   EncounterExtension              |     PV1, PV2          |
|     Observation-   ObservationExtension          |     OBX               |

## REF Message Types

### REF_I12

**Usage:** This event triggers a message to be sent from one healthcare provider to another regarding a specific patient. The referral message may contain patient demographic information, specific medical procedures to be performed (accompanied by previously obtained authorizations) and relevant clinical information pertinent to the patient's case.

**Mapped segment list**: MSH, SFT, RF1, AUT, PRD, PID, NK1, GT1, IN1, IN2, ACC, DG1, AL1, PR1, OBR, NTE, OBX, PV1, PV2, PRT

**FHIR resource types and their mapped segments:**

|     FHIR Resource         |     Segment Mapped                                                                                  |
|---------------------------|-----------------------------------------------------------------------------------------------------|
|     MessageHeader         |     MSH, SFT                                                                                        |
|     Patient               |     PID, GT1,   IN2, NK1                                                                            |
|     Provenance            |     MSH                                                                                             |
|     Organization          |     MSH, PID,   PV1, PV2, NTE, OBX, PRT, GT1, IN1, IN2, NK1, RF1, AUT, PRD, ACC, DG1, PR1,   OBR    |
|     Device                |     MSH, SFT,   OBX, PRT                                                                            |
|     Account               |     PID, GT1                                                                                        |
|     RelatedPerson         |     PID, GT1,   IN2, NK1                                                                            |
|     Practitioner          |     PV1, PV2,   NTE, OBX, PRT, RF1, PRD, ACC, DG1, PR1, OBR                                         |
|     PractitionerRole      |     OBX, PRT, PRD,   OBR                                                                            |
|     Location              |     PV1, PV2,   PRT, ACC, PR1, OBR                                                                  |
|     EpisodeOfCare         |     PV1, DG1                                                                                        |
|     Encounter             |     PV1, PV2, DG1                                                                                   |
|     ServiceRequest        |     RF1, PRD, OBR,   NTE                                                                            |
|     Specimen              |     OBR                                                                                             |
|     Coverage              |     IN1, IN2                                                                                        |
|     DiagnosticReport      |     OBX, OBR                                                                                        |
|     Observation           |     OBX, NTE, ACC                                                                                   |
|     Immunization          |                                                                                                     |
|     ClaimResponse         |     AUT                                                                                             |
|     Condition             |     ACC, DG1                                                                                        |
|     AllergyIntolerance    |     AL1                                                                                             |
|     Procedure             |     PR1                                                                                             |

**Extensions**:

|     FHIR Extension                               |     Segment Mapped    |
|--------------------------------------------------|-----------------------|
|     Patient-   PatientExtension                  |     PID, PV1          |
|     Encounter- EncounterExtension                |     PV1, PV2          |
|     Observation-   ObservationExtension          |     OBX, ACC          |
|     Condition- ConditionExtension                |     DG1               |

### REF_I14

**Usage:** This event triggers a message to be sent from one healthcare provider to another canceling a referral. A previous referral may have been made in error, or perhaps the cancellation has come from the patient.

**Mapped segment list**: MSH, SFT, RF1, AUT, PRD, PID, NK1, GT1, IN1, IN2, ACC, DG1, AL1, PR1, OBR, NTE, OBX, PV1, PV2, PRT

**FHIR resource types and their mapped segments:**

|     FHIR Resource         |     Segment Mapped                                                                                  |
|---------------------------|-----------------------------------------------------------------------------------------------------|
|     MessageHeader         |     MSH, SFT                                                                                        |
|     Patient               |     PID, GT1,   IN2, NK1                                                                            |
|     Provenance            |     MSH                                                                                             |
|     Organization          |     MSH, PID,   PV1, PV2, NTE, OBX, PRT, GT1, IN1, IN2, NK1, RF1, AUT, PRD, ACC, DG1, PR1,   OBR    |
|     Device                |     MSH, SFT,   OBX, PRT                                                                            |
|     Account               |     PID, GT1                                                                                        |
|     RelatedPerson         |     PID, GT1,   IN2, NK1                                                                            |
|     Practitioner          |     PV1, PV2,   NTE, OBX, PRT, RF1, PRD, ACC, DG1, PR1, OBR                                         |
|     PractitionerRole      |     OBX, PRT,   PRD, OBR                                                                            |
|     Location              |     PV1, PV2,   PRT, ACC, PR1, OBR                                                                  |
|     EpisodeOfCare         |     PV1, DG1                                                                                        |
|     Encounter             |     PV1, PV2, DG1                                                                                   |
|     ServiceRequest        |     RF1, PRD,   OBR, NTE                                                                            |
|     Specimen              |     OBR                                                                                             |
|     Coverage              |     IN1, IN2                                                                                        |
|     DiagnosticReport      |     OBX, OBR                                                                                        |
|     Observation           |     OBX, NTE, ACC                                                                                   |
|     ClaimResponse         |     AUT                                                                                             |
|     Condition             |     ACC, DG1                                                                                        |
|     AllergyIntolerance    |     AL1                                                                                             |
|     Procedure             |     PR1                                                                                             |

**Extensions**:

|     FHIR Extension                               |     Segment Mapped    |
|--------------------------------------------------|-----------------------|
|     Patient-   PatientExtension                  |     PID, PV1          |
|     Encounter- EncounterExtension                |     PV1, PV2          |
|     Observation-   ObservationExtension          |     OBX, ACC          |
|     Condition- ConditionExtension                |     DG1               |
