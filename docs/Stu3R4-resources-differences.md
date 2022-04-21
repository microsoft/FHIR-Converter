# Resources Differences for STU3 to R4
## Renamed Resources
|STU3|R4|
|--|--|
|BodySite|BodyStructure|
|EligibilityRequest|CoverageEligibilityRequest|
|EligibilityResponse|CoverageEligibilityResponse|
|Sequence|MolecularSequence|

## STU3 Resources Removed from R4
- DataElement 
- DeviceComponent 
- ExpansionProfile
- ImagingManifest
- ProcedureRequest
- ProcessRequest
- ProcessResponse
- ReferralRequest
- ServiceDefinition

## Constraints with STU3 to R4 templates
Note the following constraints in the default templates: 
- Code System and Terminology URLs are copied as is 
- Extension fields are copied as is
- Cannot guarantee FHIR R4 cardinality constraints

## New Resources Added to R4
- BiologicallyDerivedProduct
- CatalogEntry
- ChargeItemDefinition
- DeviceDefinition
- EffectEvidenceSynthesis
- EventDefinition
- Evidence
- EvidenceVariable
- ExampleScenario
- ImmunizationEvaluation
- InsurancePlan
- Invoice
- MedicationKnowledge
- MedicinalProduct
- MedicinalProductAuthorization
- MedicinalProductContraindication
- MedicinalProductIndication	
- MedicinalProductIngredient	
- MedicinalProductInteraction	
- MedicinalProductManufactured	
- MedicinalProductPackaged	
- MedicinalProductPharmaceutical	
- MedicinalProductUndesirableEffect
- ObservationDefinition
- OrganizationAffiliation
- ResearchDefinition	
- ResearchElementDefinition
- RiskEvidenceSynthesis
- ServiceRequest
- SpecimenDefinition
- SubstanceNucleicAcid	
- SubstancePolymer	
- SubstanceProtein	
- SubstanceReferenceInformation	
- SubstanceSourceMaterial	
- SubstanceSpecification
- TerminologyCapabilities
- VerificationResult
