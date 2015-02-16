using System;
using System.Xml.Serialization;

namespace MTechServices.Models.Entity
{
    public class GlobalData {
        [XmlElement(IsNullable = true)]
        public SecurityDetails User { get; set; }
        //[XmlElement(IsNullable = true)]
        //public StationInfo Station { get; set; }
        [XmlElement(IsNullable = true)]
        public WorkCenterSettings WcSettings { get; set; }
        [XmlElement(IsNullable = true)]
        public GlobalSettings GlobalSettings { get; set; }
    }

    # region work center settings

    public class WorkCenterSettings {
        [XmlElement(IsNullable = true)]
        public WorkCenterDataSecurity DataSecurity { get; set; }
        [XmlElement(IsNullable = true)]
        public WorkCenterSystemSetup SystemSetup { get; set; }
        [XmlElement(IsNullable = true)]
        public WorkCenterProgramSecurity ProgramSecurity { get; set; }
        [XmlElement(IsNullable = true)]
        public Facility RoomFacility { get; set; }
    }

    public class WorkCenterDataSecurity {
        [XmlElement(IsNullable = true)]
        public WorkOrderDataSecurity WorkOrder { get; set; }
        [XmlElement(IsNullable = true)]
        public EquipmentOrderDataSecurity EquipmentOrder { get; set; }
        [XmlElement(IsNullable = true)]
        public ProcedureDataSecurity Procedure { get; set; }
        [XmlElement(IsNullable = true)]
        public ScheduleTableDataSecurity ScheduleTable { get; set; }
    }

    public sealed class WorkOrderDataSecurity {
        [XmlElement(IsNullable = true)]
        public byte? EditWorkOrder { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ReopenWorkOrder { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ReprintWorkOrder { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? VoidOpenWorkOrder { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ChangeTemplates { get; set; }
    }

    public sealed class EquipmentOrderDataSecurity {
        [XmlElement(IsNullable = true)]
        public byte? EditEquipmentOrder { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ReopenEquipmentOrder { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? VoidEquipmentOrder { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? GenerateBilling { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditPatientAndPatientAccount { get; set; }
    }

    public sealed class ProcedureDataSecurity {
        [XmlElement(IsNullable = true)]
        public byte? EditPmProcedure { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? CreatePmProcedure { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ChangePmProcedureNumber { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ReviewPmProcedures { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? MergePmProcedures { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? DeletePmProcedures { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditMedtesterCheckList { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ImportMedTesterResults { get; set; }
    }

    public sealed class ScheduleTableDataSecurity {
        [XmlElement(IsNullable = true)]
        public byte? EditScheduleTables { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ReviewScheduleTables { get; set; }
    }

    public class WorkCenterSystemSetup {
        [XmlElement(IsNullable = true)]
        public WorkCenterWorkOrderSetups WorkOrder { get; set; }
        [XmlElement(IsNullable = true)]
        public WorkCenterAssetSetups Asset { get; set; }
        [XmlElement(IsNullable = true)]
        public WorkCenterSchedulerSetups Scheduler { get; set; }
        [XmlElement(IsNullable = true)]
        public WorkCenterDepreciationSetups Depreciation { get; set; }
        [XmlElement(IsNullable = true)]
        public WorkCenterPostingSystemSetups PostingSystem { get; set; }
        [XmlElement(IsNullable = true)]
        public WorkCenterFieldLabelSetups FieldLabels { get; set; }
        [XmlElement(IsNullable = true)]
        public WorkCenterMobileAssetSetups MobileAsset { get; set; }
        [XmlElement(IsNullable = true)]
        public WorkCenterMobileBillingSetups MobileBilling { get; set; }
        [XmlElement(IsNullable = true)]
        public WorkCenterMedTesterSetups MedTester { get; set; }
    }

    public sealed class WorkCenterWorkOrderSetups {
        [XmlElement(IsNullable = true)]
        public Option DisplayWorkerName { get; set; }
        [XmlElement(IsNullable = true)]
        public Option PrintControlNotesOnWo { get; set; }
        [XmlElement(IsNullable = true)]
        public Option AllowChangesToPartPrices { get; set; }
        [XmlElement(IsNullable = true)]
        public Option MatchPmPartsPmKitLevels { get; set; }
        [XmlElement(IsNullable = true)]
        public Option UseOWSCode { get; set; }
        [XmlElement(IsNullable = true)]
        public Option UseIssueStartCompleteTimes { get; set; }
        [XmlElement(IsNullable = true)]
        public Option UseDueDateTimeFields { get; set; }
        [XmlElement(IsNullable = true)]
        public Option UseShortForm { get; set; }
        [XmlElement(IsNullable = true)]
        public long DefaultWoUrgency { get; set; }
        // w.o continued -- 7/8 Completed
        [XmlElement(IsNullable = true)]
        public Option AllowCopyforPmLevelField { get; set; }
        [XmlElement(IsNullable = true)]
        public Option AssignTimeToSafetyWOs { get; set; }
        [XmlElement(IsNullable = true)]
        public decimal DefaultWoSafetyTime { get; set; }
        [XmlElement(IsNullable = true)]
        public Option LockDcSafetyTime { get; set; }
        [XmlElement(IsNullable = true)]
        public ActionForOpenSafetyWO DefaultActionForOpenSafetyWO { get; set; }
        [XmlElement(IsNullable = true)]
        public Option AppendTimeEntryToNotes { get; set; }
        [XmlElement(IsNullable = true)]
        public Option AllowQuickClose { get; set; }
        [XmlElement(IsNullable = true)]
        public Option AllowNegativeTotalCharges { get; set; }

        // w.o printing -- completed
        [XmlElement(IsNullable = true)]
        public byte? MaxWoCopiesWithoutPwd { get; set; }
        [XmlElement(IsNullable = true)]
        public Option AutoPrintNonSafetyWOs { get; set; }
        [XmlElement(IsNullable = true)]
        public Option AutoPrintSafetyWOs { get; set; }
        [XmlElement(IsNullable = true)]
        public Option ClearHourlyRateOnOpenWo { get; set; }
        [XmlElement(IsNullable = true)]
        public Option ClearHourlyRateOnClosedWo { get; set; }
        [XmlElement(IsNullable = true)]
        public Option ClearTotalTimeOnNonSafetyWo { get; set; }
        [XmlElement(IsNullable = true)]
        public Option ClearTotalTimeOnSafetyWo { get; set; }

        // w.o Time -- completed
        [XmlElement(IsNullable = true)]
        public SystemCode DefaultTask { get; set; }
        [XmlElement(IsNullable = true)]
        public SystemCode DefaultTimeType { get; set; }
    }

    public sealed class WorkCenterAssetSetups {
        [XmlElement(IsNullable = true)]
        public short? DefaultWarranty { get; set; }
        [XmlElement(IsNullable = true)]
        public Option AutoFillIncomingDate { get; set; }
        [XmlElement(IsNullable = true)]
        public Option AllowEqpLevelSchdTables { get; set; }
        [XmlElement(IsNullable = true)]
        public Option AllowWrkrAssignForSchedule { get; set; }
    }

    public sealed class WorkCenterSchedulerSetups {
        [XmlElement(IsNullable = true)]
        public ScheduleOperatingMode OperatingMode { get; set; }
        [XmlElement(IsNullable = true)]
        public ScheduleReportingMode ReportingMode { get; set; }
        [XmlElement(IsNullable = true)]
        public DateTime EarliestIssueDate { get; set; }
        [XmlElement(IsNullable = true)]
        public Option SetIssueDateToFirstOfReport { get; set; }
        [XmlElement(IsNullable = true)]
        public Option IncludeWoCompletionFields { get; set; }
        [XmlElement(IsNullable = true)]
        public Option EnableSchedToAssignPartKits { get; set; }
        [XmlElement(IsNullable = true)]
        public PmProcedureFormat UseForProcedures { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ScheduleWindow { get; set; }
    }

    public sealed class WorkCenterDepreciationSetups {
        [XmlElement(IsNullable = true)]
        public decimal DefaultLifeCode { get; set; }
        [XmlElement(IsNullable = true)]
        public Option SubtractLifetoDateCost { get; set; }
        [XmlElement(IsNullable = true)]
        public DepreciationConventionMode ConventionMode { get; set; }
        [XmlElement(IsNullable = true)]
        public DepreciationRateMode RateMode { get; set; }
    }

    public sealed class WorkCenterPostingSystemSetups {
        [XmlElement(IsNullable = true)]
        public Option DeriveRprtMonthFromPostingDt { get; set; }
        [XmlElement(IsNullable = true)]
        public short? DaysOffsetRptPerEndOfMonth { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? DaysOffsetRptPerLocked { get; set; }
    }

    public sealed class WorkCenterFieldLabelSetups {
        [XmlElement(IsNullable = true)]
        public string AreaFieldLabel { get; set; }
        [XmlElement(IsNullable = true)]
        public string AreaColumnLabel { get; set; }
    }

    public sealed class WorkCenterMobileAssetSetups {
        [XmlElement(IsNullable = true)]
        public Option AutoDiscontinueEO { get; set; }
        [XmlElement(IsNullable = true)]
        public long DeliverAction { get; set; }
        [XmlElement(IsNullable = true)]
        public long MoveAction { get; set; }
        [XmlElement(IsNullable = true)]
        public long DiscontinueAction { get; set; }
        [XmlElement(IsNullable = true)]
        public long PickupAction { get; set; }
        [XmlElement(IsNullable = true)]
        public long UrgencyAction { get; set; }
        [XmlElement(IsNullable = true)]
        public long AutoCloseNonDuarableAction { get; set; }
        [XmlElement(IsNullable = true)]
        public short? DefaultStatusCode { get; set; }
        [XmlElement(IsNullable = true)]
        public string DefaultOwnerAccount { get; set; }
        [XmlElement(IsNullable = true)]
        public string DefaultLocationAccount { get; set; }
        [XmlElement(IsNullable = true)]
        public string DefaultReasonSentOut { get; set; }
        [XmlElement(IsNullable = true)]
        public Option ValidatePatientAccount { get; set; }
    }

    public sealed class WorkCenterMobileBillingSetups {
        [XmlElement(IsNullable = true)]
        public Option UseMobileBillingSystem { get; set; }
        [XmlElement(IsNullable = true)]
        public DateTime BillingStartDate { get; set; }
        [XmlElement(IsNullable = true)]
        public short? PurgeBillingRecordsAfter { get; set; }
        [XmlElement(IsNullable = true)]
        public BillingEndsOn BillingEndDate { get; set; }
        [XmlElement(IsNullable = true)]
        public DateTime BillingEndTime { get; set; }
        [XmlElement(IsNullable = true)]
        public string BillingExportFile { get; set; }
        [XmlElement(IsNullable = true)]
        public string BillingExportFolder { get; set; }
    }

    public sealed class WorkCenterMedTesterSetups {
        [XmlElement(IsNullable = true)]
        public Option MatchMtResults { get; set; }
        [XmlElement(IsNullable = true)]
        public Option CloseWoDuringImport { get; set; }
        [XmlElement(IsNullable = true)]
        public string MtResultCode { get; set; }
        [XmlElement(IsNullable = true)]
        public Option SetSafetyTestPassed { get; set; }
        [XmlElement(IsNullable = true)]
        public MedTesterWorkerType ForWorkerUse { get; set; }
    }

    public class WorkCenterProgramSecurity {
    }

    # endregion

    # region Global settings

    public class GlobalSettings {
        [XmlElement(IsNullable = true)]
        public GlobalDataSecurity DataSecurity { get; set; }
        [XmlElement(IsNullable = true)]
        public GlobalSystemSetup SystemSetup { get; set; }
        [XmlElement(IsNullable = true)]
        public GlobalProgramSecurity ProgramSecurity { get; set; }
    }

    public class GlobalDataSecurity {
        [XmlElement(IsNullable = true)]
        public AccountDataSecurity Account { get; set; }
        [XmlElement(IsNullable = true)]
        public FacilityDataSecurity Facility { get; set; }
        [XmlElement(IsNullable = true)]
        public AssetDataSecurity Asset { get; set; }
        [XmlElement(IsNullable = true)]
        public ModelDataSecurity Model { get; set; }
        [XmlElement(IsNullable = true)]
        public AssetCategoryDataSecurity AssetCategory { get; set; }
        [XmlElement(IsNullable = true)]
        public CodesDataSecurity Codes { get; set; }
        [XmlElement(IsNullable = true)]
        public WorkerDataSecurity Worker { get; set; }
        [XmlElement(IsNullable = true)]
        public AreaRoomDataSecurity AreaRoom { get; set; }
        [XmlElement(IsNullable = true)]
        public HourlyRateDataSecurity HourlyRateTable { get; set; }
        [XmlElement(IsNullable = true)]
        public PartDataSecurity Part { get; set; }
        [XmlElement(IsNullable = true)]
        public SourceDataSecurity Source { get; set; }
    }

    public sealed class AccountDataSecurity {
        [XmlElement(IsNullable = true)]
        public byte? CreateAccount { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditAccount { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ChangeAccountCode { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ReviewAccounts { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? MergeAccounts { get; set; }
    }

    public sealed class FacilityDataSecurity {
        [XmlElement(IsNullable = true)]
        public byte? CreateFacility { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditFacility { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ChangeFacilityName { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ReviewFacilities { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? MergeFacilities { get; set; }
    }

    public sealed class AssetDataSecurity {
        [XmlElement(IsNullable = true)]
        public byte? CreateAsset { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditAsset { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ChangeControlNumber { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ReviewAssets { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? MergeAssets { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditContracts { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? CreateUserDefinedFields { get; set; }
    }

    public sealed class ModelDataSecurity {
        [XmlElement(IsNullable = true)]
        public byte? CreateModel { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditModel { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ChangeModel { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ReviewModels { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? MergeModels { get; set; }
    }

    public sealed class AssetCategoryDataSecurity {
        [XmlElement(IsNullable = true)]
        public byte? CreateAssetCategory { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditAssetCategory { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ChangeAssetCategoryName { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ReviewAssetCategories { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? MergeAssetCategories { get; set; }
    }

    public sealed class CodesDataSecurity {
        [XmlElement(IsNullable = true)]
        public byte? EditRateCategory { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditAssetStatusCode { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditRequestCode { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditResultCode { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditFaultCode { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditSchedulerCode { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditCenterCode { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditActionCode { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditUrgencyCode { get; set; }
    }

    public sealed class WorkerDataSecurity {
        [XmlElement(IsNullable = true)]
        public byte? CreateWorker { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditWorker { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ChangeWorkerCode { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? AccessWorkerPersonalData { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? MergeWorkers { get; set; }
    }

    public sealed class AreaRoomDataSecurity {
        [XmlElement(IsNullable = true)]
        public byte? CreateAndEditAreas { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? CreateAndEditRooms { get; set; }
    }

    public sealed class HourlyRateDataSecurity {
        [XmlElement(IsNullable = true)]
        public byte? EditHourlyRateTables { get; set; }
    }

    public sealed class PartDataSecurity {
        [XmlElement(IsNullable = true)]
        public byte? CreateInventory { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditInventory { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ChangeStockNumber { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ReviewInventory { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? MergeInventories { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditPurchaseRequest { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? CreatePurchaseRequest { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? DeletePurchaseRequest { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? PrintPurchaseRequest { get; set; }
    }

    public sealed class SourceDataSecurity {
        [XmlElement(IsNullable = true)]
        public byte? CreateSource { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditSource { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ChangeSourceCode { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? MergeSources { get; set; }
    }

    public class GlobalSystemSetup {
        [XmlElement(IsNullable = true)]
        public GlobalWorkOrderSetups WorkOrder { get; set; }
        [XmlElement(IsNullable = true)]
        public GlobalAssetSetups Asset { get; set; }
        [XmlElement(IsNullable = true)]
        public GlobalPartSetups Part { get; set; }
        [XmlElement(IsNullable = true)]
        public GlobalMobileAssetSetups MobileAsset { get; set; }
        [XmlElement(IsNullable = true)]
        public GlobalMobileBillingSetups MobileBilling { get; set; }
    }

    public class GlobalWorkOrderSetups {
        [XmlElement(IsNullable = true)]
        public Option ViewWOsAcrossWorkCenters { get; set; }
    }

    public class GlobalAssetSetups {
        [XmlElement(IsNullable = true)]
        public WarrantyCalculationDate WarrantyCalculationDate { get; set; }
        [XmlElement(IsNullable = true)]
        public Option SupportAssetLocatorRoom { get; set; }
        [XmlElement(IsNullable = true)]
        public Option ShareDevCatAcrossWrkCenters { get; set; }
        [XmlElement(IsNullable = true)]
        public Option AllowRoomSelectionFromList { get; set; }
    }

    public class GlobalPartSetups {
        [XmlElement(IsNullable = true)]
        public VendorAddress VendorAddrTypeOnPurReqForm { get; set; }
        [XmlElement(IsNullable = true)]
        public Option DisplayStockNumOnPurReqForm { get; set; }
        [XmlElement(IsNullable = true)]
        public MultiLinePurchaseRequestLayout MultiLinePurReqForm { get; set; }
        [XmlElement(IsNullable = true)]
        public Option DefaultStockType { get; set; }
        [XmlElement(IsNullable = true)]
        public float? InternalAccPriceMarkup { get; set; }
        [XmlElement(IsNullable = true)]
        public float? AffiliatedAccPriceMarkup { get; set; }
        [XmlElement(IsNullable = true)]
        public float? NonAffiliatedAccPriceMarkuproperty { get; set; }
        [XmlElement(IsNullable = true)]
        public Option UpdatePartCost { get; set; }
        [XmlElement(IsNullable = true)]
        public Option UpdatePartVendor { get; set; }
    }

    public class GlobalMobileAssetSetups {
        [XmlElement(IsNullable = true)]
        public short? MedicalRecordMinLength { get; set; }
        [XmlElement(IsNullable = true)]
        public short? MedicalRecordMaxLength { get; set; }
        [XmlElement(IsNullable = true)]
        public RecordFormat MedicalRecordType { get; set; }
        [XmlElement(IsNullable = true)]
        public short? PatientAccountMinLength { get; set; }
        [XmlElement(IsNullable = true)]
        public short? PatientAccountMaxLength { get; set; }
        [XmlElement(IsNullable = true)]
        public RecordFormat PatientAccountType { get; set; }
        [XmlElement(IsNullable = true)]
        public BarCodeFormat BarCodeFormat { get; set; }
        [XmlElement(IsNullable = true)]
        public PrintMode PrintBarCodeLabel { get; set; }
    }

    public class GlobalMobileBillingSetups {
        [XmlElement(IsNullable = true)]
        public short? ReAdmissionTimePeriod { get; set; }
    }

    public class GlobalProgramSecurity {
        [XmlElement(IsNullable = true)]
        public byte? WorkerExpReportSetup { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ScheduleReporter { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? SecurityPasswords { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? ChangeStation { get; set; }
        [XmlElement(IsNullable = true)]
        public byte? EditStation { get; set; }
        [XmlElement(IsNullable = true)]
        public bool? RestrictAccountAndFacilityListFromArchivedRecords { get; set; }
        [XmlElement(IsNullable = true)]
        public bool? RestrictWorkerKListFromGlobalAndFormerRecords { get; set; }
    }

    #endregion

    # region enums for globaldata

    public enum Option {
        None = 0,
        Yes = 1,
        No = 2,
    }

    public enum ScheduleOperatingMode {
        None = 0,
        LastTestDate = 1,
        ScheduleDate = 2,
    }

    public enum ScheduleReportingMode {
        None = 0,
        Monthly = 1,
        Weekly = 2,
    }

    public enum PmProcedureFormat {
        None = 0,
        TextDocuments = 1,
        ListOfProcedureSteps = 2,
    }

    public enum DepreciationConventionMode {
        None = 0,
        HalfYear = 1,
        TimeLine = 2,
    }

    public enum DepreciationRateMode {
        None = 0,
        StraightLine = 1,
        OneFiftyPercent = 2,
        TwoHundredPercent = 3,
    }

    public enum BillingEndsOn {
        None = 0,
        Today = 1,
        Yesterday = 2,
    }

    public enum MedTesterWorkerType {
        None = 0,
        Operator = 1,
        Worker = 2,
    }

    public enum WarrantyCalculationDate {
        None = 0,
        PurchaseDate = 1,
        IncomingDate = 2,
    }

    public enum VendorAddress {
        None = 0,
        Billing = 1,
        Shipping = 2,
    }

    public enum MultiLinePurchaseRequestLayout {
        None = 0,
        Potrait = 1,
        Landscape = 2,
        Custom = 3,
    }

    public enum RecordFormat {
        None = 0,
        Numeric = 1,
        AlphaNumeric = 2,
    }

    public enum BarCodeFormat {
        None = 0,
        ThreeOfNine = 1,
    }

    public enum PrintMode {
        None = 0,
        Automatic = 1,
        Prompt = 2,
        No = 3,
    }

    public enum ActionForOpenSafetyWO {
        None = 0,
        Void = 1,
        Close = 2,
        EditManually = 3,
    }

    # endregion

    public class SystemSetting {
        [XmlElement]
        public string SettingField { get; set; }
        [XmlElement]
        public string SettingValue { get; set; }
    }
}