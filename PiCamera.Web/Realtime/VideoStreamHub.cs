using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PiCamera.Web.Realtime
{
    public class VideoStreamHub : Hub<IVideoStreamClient>
    {
        public async Task GetStatus()
        {
            await Clients.All.ReceiveStatus(CameraControl.Current.IsRunning);
        }
    }
}
