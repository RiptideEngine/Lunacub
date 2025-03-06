namespace Caxivitual.Lunacub.Building;

public readonly struct BuildingResult {
    public IReadOnlyDictionary<ResourceID, BuildingReport>? Reports { get; }
    public Exception? Exception { get; }
    
    [MemberNotNullWhen(false, nameof(Exception))]
    [MemberNotNullWhen(true, nameof(Reports))]
    public bool IsSuccess => Exception == null;

    internal BuildingResult(IReadOnlyDictionary<ResourceID, BuildingReport>? reports, Exception? excecption) {
        Reports = reports;
        Exception = excecption;
    }
}