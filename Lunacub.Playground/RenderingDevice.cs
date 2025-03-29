using Microsoft.Extensions.Logging;
using Silk.NET.WebGPU;

namespace Lunacub.Playground;

public sealed unsafe class RenderingDevice : IDisposable {
    private bool _disposed;
    
    public Adapter* Adapter { get; private set; }
    public Device* Device { get; private set; }
    public Queue* Queue { get; private set; }

    private readonly WebGPU _webgpu;

    internal RenderingDevice(WebGPU webgpu, Instance* instance, Surface* surface) {
        Application.Logger.LogInformation("Initializing WebGPU Devices...");
        
        using ManualResetEventSlim _queryLock = new();

        using (var callback = PfnRequestAdapterCallback.From(RequestAdapterCallback)) {
            webgpu.InstanceRequestAdapter(instance, new RequestAdapterOptions {
                CompatibleSurface = surface,
                PowerPreference = PowerPreference.Undefined,
                ForceFallbackAdapter = false,
            }, callback, null);

            _queryLock.Wait();

            if (Adapter == null) {
                throw new("Failed to query suitable adapter.");
            }
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
            
            if (Adapter == null) {
                throw new("Failed to query suitable device.");
            }
        }
        
        Queue = webgpu.DeviceGetQueue(Device);
        
        _webgpu = webgpu;
        
        void RequestAdapterCallback(RequestAdapterStatus status, Adapter* adapter, byte* message, void* userData) {
            if (status == RequestAdapterStatus.Success) {
                Adapter = adapter;
            } else {
                Application.Logger.LogError("Failed to query WebGPU adapter. Status: {status}.", status);
            }
            
            _queryLock.Set();
        }
        void RequestDeviceCallback(RequestDeviceStatus status, Device* device, byte* message, void* userData) {
            if (status == RequestDeviceStatus.Success) {
                Device = device;
            } else {
                Application.Logger.LogError("Failed to query WebGPU device. Status: {status}.", status);
            }
            
            _queryLock.Set();
        }
    }

    private void Dispose(bool disposing) {
        if (_disposed) return;

        _disposed = true;
        
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