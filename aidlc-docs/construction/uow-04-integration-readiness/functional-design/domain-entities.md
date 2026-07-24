# UOW-04 Domain Entities

## IntegrationReadinessRecord
- **Purpose**: Unit-level integration status record for UOW-01..UOW-03.
- **Fields**:
  - `unitId` (`UOW-01|UOW-02|UOW-03`)
  - `status` (`Complete|Incomplete`)
  - `note`

## ContractConsistencyCheck
- **Purpose**: Captures contract validation outcome across required surfaces.
- **Fields**:
  - `checkId`
  - `surface` (`PublicApi|SharedPrimitives|RepositoryInterfaces`)
  - `status` (`Pass|Fail`)
  - `note`

## StoryFileTraceEntry
- **Purpose**: Evidence map from each story to implementation files.
- **Fields**:
  - `storyId` (`US-01..US-12`)
  - `unitId`
  - `filePaths` (non-empty list)
  - `status` (`Covered|Missing`)
  - `note`

## UnresolvedIntegrationItem
- **Purpose**: Tracks unresolved integration findings that can block gate passage.
- **Fields**:
  - `itemId`
  - `category`
  - `impact` (`Blocking`)
  - `description`
  - `status` (`Open|Resolved`)

## RuntimeReadinessRecord
- **Purpose**: Minimal runtime artifact presence validation.
- **Fields**:
  - `dockerComposePresent` (bool)
  - `policyTemplatePresent` (bool)
  - `status` (`Pass|Fail`)
  - `note`

## QualityGateChecklist
- **Purpose**: Consolidated Build-and-Test entry decision.
- **Fields**:
  - `owner`
  - `allUnitsImplemented` (bool)
  - `noBlockingUnresolvedItems` (bool)
  - `errorContractConsistent` (bool)
  - `status` (`Pass|Fail`)
  - `summaryNote`
