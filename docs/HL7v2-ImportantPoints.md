# HL7v2 to FHIR: Important Points

1.	If the System URL is not present in the HL7 V2 message, then the FHIR converter tool will use a dummy URL in the corresponding FHIR resource. This URL needs to be replaced by actual URL by the end users while consuming the FHIR resource.
2.	FHIR converter tool will not use any code system which is a licensed one e.g., SNOMED. If the end users want the codes, then they need to purchase the subscription and use the codes. 
3.	We have not mapped some v2 fields to FHIR attribute where date/time manipulation is required. Since the mapping needs date/time manipulation which is not possible in current FHIR converter implementation as explained through some use cases below. This will be taken care of in future versions with the help of Date Filters.

    * AIS Segment to ServiceRequest resource mapping looks like this: <br>
ServiceRequest.occurrencePeriod.end = AIS-4 [DTM- Start Date/Time] + AIS-7 [NM- Duration] <br>
Here, the number of minutes in AIS-7 needs to be added to the time in AIS-4 to get the endpoint of the occurrencePeriod (other elements of the date/time (hours, date, etc) may also need to be changed depending on the length of the duration)<br>
e.g : <br>
Start Date/Time + Duration (15m)<br>
Start Date/Time + Duration (15h)

    * PR1 Segment to Procedure resource mapping looks like this:<br>
performedPeriod.end = PR1-5 [DTM- performedDateTime] + PR1-7 [NM-Procedure Minutes] <br>
Here, the number of minutes in PR1-7 needs to be added to the time in PR1-5 to get the endpoint of the performedPeriod (Other elements of the date/time (hours, date, etc) may also need to be changed depending on the length of the procedure)<br>
e.g : <br>
performedDateTime + Duration (15m)<br>
performedDateTime + Duration (15h)

4.	We have handled v2 message type segments grouping in the current FHIR converter tool. The filters available in the code i.e., get_related_segment_list & get_parent_segment have been used but it is not giving the expected grouping result when grouping structure don’t have the segments as explained in below example:

    * **Example 1: related to ‘get_parent_segment’ filter**<br>
If we have below structure message, <br>
OBR|1|||24725-4^CT Head^LN||202110201200+0215
SPM|1|2012545^2012999999&IA PHIMS Stage&2.16.840.1.114222.4.3.3.5.1.2&ISO 
OBX|1|RP|1063-7^Serum or Plasma^XYZ^^^^2.33^^result1<br>
Then after execution of below line, <br>
{% assign checkParentOBR = hl7v2Data | get_parent_segment: 'OBX', {{index_value}}, 'OBR' -%} <br>
**Actual Output:** If we don't have any OBX below OBR, and we executed the above code then in checkParentOBR will get the OBR segment which is above SPM segment. <br>
**Expected Output:** If we don't have any OBX below OBR, then in checkParentOBR will not have any value in it.	

    * **Example 2: related to ‘get_related_segment_list’ filter** <br>
If we have below structure message,<br>
SCH|12345|36996||||Add|EMERGENCY|Normal|70|Min|2^^2
TQ1|||||||202010041126+0215|202010081126+0215|A
NTE|1||Appointment specific instructions
RGS|1||
AIS|1||111^Hemogram^L|
NTE|1||Fasting for Blood sugar before appointment<br>
Then after execution of below line,<br> 
{% assign nteSegmentLists = hl7v2Data | get_related_segment_list: schSegment, 'NTE' -%}<br>
**Actual Output:** If we don't have any NTE below SCH, and we executed the above code then in nteSegmentLists we are getting the NTE segment which is below AIS segment. <br>
**Expected Output:** If we don't have any NTE below SCH, then in nteSegmentLists will not have any value in it.

5.	To generate the FHIR resources ID we are passing the segment fields/subfields or even the whole segment as a parameter.<br>
If two field values are same and we have considered those field for generating the particular 		resource ID then FHIR converter tool will generate the same id and it will overwrite/ merge the 	content of first resource with the second one.<br>
    * **Example 1:**<br>
In ORU-R01 message type we have mapped the OBR segment to the ServiceRequest resource and 	generating its id using the OBR.2.1 & OBR.2.2 field if the value present in this fields in the input v2 	message.<br>
OBR|1|845439^GHH OE|986^IA PHIMS Stage^2.16.840.1.114222.4.3.3.5.1.2^ISO|||| <br>
OBR|2|845439^GHH OE||24725-4^CT Head^LN||202110201200+0215|202110201200+0215 <br>
Here, OBR segment may repeat in v2 message so if we have same values in first OBR.2.1, 		OBR.2.2 and in second OBR.2.1, OBR.2.2 as given in above example then it will overwrite the first 	ServiceRequest resource content with the Second ServiceRequest resource.
    * **Example 2:** <br> 
MSH-4 Sending Facility:  GHHSFacility^2.16.840.1.122848.1.30^ISO <br>
MSH-5 Receiving Application: GHHSFacility^2.16.840.1.122848.1.30^ISO <br>
In FHIR converter code we are generating the organization resource from MSH-4 and MSH-5, so if we 	passed the same values in both the fields it will generate the same resource ID and it will overwrite 	the content of the first resource generated from MSH-4 with the resource generated from MSH-5.

6.	**ADT_A29 delete:**
We are deleting the FHIR resources (e.g., Patient, Encounter, Account etc) in a bundle which is generated from ADT A29 message. We have changed the request method as ‘DELETE’ in template for A29.  It will delete the existing FHIR resources from FHIR server if the ID for FHIR resources matched. However, it will not delete the resources (e.g., Observation, Medication etc.) which are not created from ADT_A29 and which were created as part of other Message Types like ADT_A01/ ADT_A28
7.	**Referential Integrity:**
The FHIR server implementation does not support ‘Referential Integrity’. Hence the downstream applications will not get any error while deleting resources even if there are dependent resources which still exist. 
8.	**Bundle Type as ‘Batch’ instead of ‘Transaction’:**
We have updated the Bundle type to ‘Batch’ for all the message templates as FHIR server currently does not support ‘transaction’ and added the bundle ID generation from MSH segment.
9.	**Two Provenance resources:**
We are creating two provenance resources. One from MSH and the other from EVN. So, values from both the segments would be captured in provenance based on what is getting passed to the Provenance Liquid template. <br>
This is done to capture the following:<br>
    - Capture message creation activity in provenance using MSH Segment <br>
    - Capture real world event in provenance using EVN segment Empty spaces in HL7<br>	
10.	**Empty spaces in the input HL7 message:**
If HL7 message contains spaces, it will lead to creating empty resource value or resources without ID. Currently this is not handled while conversion takes place. There should be postprocessing added to remove any such blank values or resources.
11.	**IN1 mapping to subscriberID:**
It is safe not to map subscriber id to Coverage.identifier. FHIR suggest using member id and subscriber id combination to use as identifier but not alone subscriber id. Hence IN1.49 is now only mapped to subscriberID attribute in Coverage FHIR resource.
12.	**ORM_O01 discontinued after V2.6:** ORM_O01 will be based on HL7 version 2.6 since it is not available in the later versions.
13.	**OUL_R24 OBX- Observation/result grouping related issue:**
In OUL-R24 Message we have SPECIMEN_OBSERVATION group under SPECIMEN group which is optional and after SPECIMEN group we have RESULT group. So, from v2 side will receive input like below:<br>
OBR||| ----> ORDER<br> 
SPM||| ----> SPECIMEN<br> 
OBX||| ----> SPECIMEN_OBSERVATION <br>
OBX||| ----> RESULT <br>
If we have two OBX after SPM segment one is related to SPECIMEN_OBSERVATION group and second is related to RESULT group, then currently we don't have any filter or way to find/ distinguish between these two OBX which one is related to which group. <br>
So, in current OUL_R24.liquid placeholder template we are assuming that from v2 side we will get SAC segment in CONTAINER-SPECIMEN group. Using that we are handling this grouping.
14.	**PRIOR_RESULT group related issue in OMG_019 and OML_O21:** In OMG_O19 & OML_O21 trigger events we have Prior_Result group under order group. In the prior result group, we only have ORC as a required segment due to this we are unable to distinguish whether ORC is part of the next order group or prior result group.<br>
As a solution for this issue, we are considering PV1 segment as a required segment in prior 	result group and prior result group should be there in last order group only.
15.	**ChargeItem to ServiceRequest referencing issue in OMG_O19 and OML_021:** not added ChargeItem[1].supportingInformation.reference = ServiceRequest[1].id reference as we cannot reference it to a correct ServiceRequest instance due to prior result grouping issue.
