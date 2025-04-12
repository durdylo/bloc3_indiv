using System;

namespace CameraService.Messages
{
    public class CameraStatusChangedMessage
    {
        public string CameraCode { get; set; }
        public bool EstAfficher { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}