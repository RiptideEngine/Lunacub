using Silk.NET.WebGPU;

namespace Caxivitual.Lunacub.Examples.GasterBlaster;

public sealed unsafe class RenderingDevice : IDisposable {
    private bool _disposed;
    
    public Adapter* Adapter { get; private set; }
    public Device* Device { get; private set; }
    public Queue* Queue { get; private set; }

    private readonly WebGPU _webgpu;

    internal RenderingDevice(WebGPU webgpu, Instance* instance, Surface* surface) {
        using ManualResetEventSlim _queryLock = new();

        using (var callback = PfnRequestAdapterCallback.From(RequestAdapterCallback)) {
            webgpu.InstanceRequestAdapter(instance, new RequestAdapterOptions {
                CompatibleSurface = surface,
                PowerPreference = PowerPreference.HighPerformance,
                ForceFallbackAdapter = false,
            }, callback, null);

            _queryLock.Wait();
        }

        nuint numFeatures = webgpu.AdapterEnumerateFeatures(Adapter, null);
        Span<FeatureName> features = numFeatures > 32 ? new FeatureName[numFeatures] : stackalloc FeatureName[(int)numFeatures];
        webgpu.AdapterEnumerateFeatures(Adapter, features);

        fixed (FeatureName* pFeatures = features) {
            using var callback = PfnRequestDeviceCallback.From(RequestDeviceCallback);

            webgpu.AdapterRequestDevice(Adapter, new DeviceDescriptor {
                RequiredFeatures = pFeatures,
                RequiredFeatureCount = (nuint)features.Length,
            }, callback, null);

            _queryLock.Wait();
        }

        Queue = webgpu.DeviceGetQueue(Device);

        _webgpu = webgpu;
        
        void RequestAdapterCallback(RequestAdapterStatus status, Adapter* adapter, byte* message, void* userData) {
            if (status == RequestAdapterStatus.Success) {
                Adapter = adapter;
            }

            _queryLock.Set();
        }
        void RequestDeviceCallback(RequestDeviceStatus status, Device* device, byte* message, void* userData) {
            if (status == RequestDeviceStatus.Success) {
                Device = device;
            }

            _queryLock.Set();
        }
    }

    private void Dispose(bool disposing) {
        if (Interlocked.Exchange(ref _disposed, true)) return;

        if (Queue != null) { _webgpu.QueueRelease(Queue); Queue = null; }
        if (Device != null) { _webgpu.DeviceRelease(Device); Device = null; }
        if (Adapter != null) { _webgpu.AdapterRelease(Adapter); Adapter = null; }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~RenderingDevice() {
        Dispose(false);
    }
}