namespace Microsoft.HockeyApp.Services.Device
{
    /// <summary>
    /// The chassis type according to https://technet.microsoft.com/en-us/library/ee156537.aspx
    /// </summary>
    internal enum ChassisType
    {
        Other = 1,
        Unknown = 2,
        Desktop = 3,
        LowProfileDesktop = 4,
        PizzaBox = 5,
        MiniTower = 6,
        Tower = 7,
        Portable = 8,
        Laptop = 9,
        Notebook = 10,
        HandHeld = 11,
        DockingStation = 12,
        AllInOne = 13,
        SubNotebook = 14,
        SpaceSaving = 15,
        LunchBox = 16,
        MainSystemChassis = 17,
        ExpansionChassis = 18,
        SubChassis = 19,
        BusExpansionChassis = 12,
        PeripheralChassis = 21,
        StorageChassis = 22,
        RackMountChassis = 23,
        SealedCasePC = 24
    }
}
