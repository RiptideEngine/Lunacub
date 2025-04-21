using Microsoft.Extensions.Logging;

namespace Caxivitual.Lunacub.Importing;

public static class Logging {
    public static EventId BeginImportEvent => new(1, "BeginImport");
    public static EventId ImportExceptionOccuredEvent => new(2, "ImportExceptionOccured");
    public static EventId DependencyImportExceptionOccuredEvent => new(3, "DependencyImportExceptionOccured");
    public static EventId ImportCancelEvent => new(4, "ImportCancel");
}