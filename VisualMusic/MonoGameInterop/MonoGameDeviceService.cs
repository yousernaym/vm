using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading;

#pragma warning disable 67

namespace VisualMusic.MonoGameInterop
{
    class GraphicsDeviceService : IGraphicsDeviceService
    {
        static GraphicsDeviceService s_instance;
        static int s_referenceCount;

        readonly PresentationParameters _parameters;
        GraphicsDevice _graphicsDevice;

        GraphicsDeviceService(IntPtr windowHandle, int width, int height)
        {
            _parameters = new PresentationParameters
            {
                BackBufferWidth = Math.Max(width, 1),
                BackBufferHeight = Math.Max(height, 1),
                BackBufferFormat = SurfaceFormat.Color,
                DepthStencilFormat = DepthFormat.Depth24Stencil8,
                DeviceWindowHandle = windowHandle,
                PresentationInterval = PresentInterval.One,
                IsFullScreen = false,
                MultiSampleCount = 8,
            };

            _graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, _parameters);
        }

        public static GraphicsDeviceService AddRef(IntPtr windowHandle, int width, int height)
        {
            if (Interlocked.Increment(ref s_referenceCount) == 1)
                s_instance = new GraphicsDeviceService(windowHandle, width, height);

            return s_instance;
        }

        public void Release(bool disposing)
        {
            if (Interlocked.Decrement(ref s_referenceCount) != 0)
                return;

            if (disposing)
            {
                DeviceDisposing?.Invoke(this, EventArgs.Empty);
                _graphicsDevice.Dispose();
            }

            _graphicsDevice = null;
            s_instance = null;
        }

        public void ResetDevice(int width, int height)
        {
            DeviceResetting?.Invoke(this, EventArgs.Empty);
            _parameters.BackBufferWidth = width;
            _parameters.BackBufferHeight = height;
            _graphicsDevice.Reset(_parameters);
            DeviceReset?.Invoke(this, EventArgs.Empty);
        }

        public GraphicsDevice GraphicsDevice => _graphicsDevice;

        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;
    }

    public class ServiceContainer : IServiceProvider
    {
        readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void AddService<T>(T service) => _services.Add(typeof(T), service);

        public object GetService(Type serviceType)
        {
            _services.TryGetValue(serviceType, out var service);
            return service;
        }
    }
}
