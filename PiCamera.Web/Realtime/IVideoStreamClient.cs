using PiCamera.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PiCamera.Web.Realtime
{
    public interface IVideoStreamClient
    {
        Task ReceiveStatus(bool status);
        Task ReceiveFragment(VideoFragment videoFragment);
    }
}
