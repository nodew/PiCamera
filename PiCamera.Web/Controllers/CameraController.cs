using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PiCamera.Web.Controllers
{
    public class CameraController : ControllerBase
    {
        [HttpPost]
        public void Start()
        {
            CameraControl.Current.Start();
        }

        [HttpPost]
        public void Stop()
        {
            CameraControl.Current.Stop();
        }

        [HttpGet]
        public bool Status()
        {
            return CameraControl.Current.IsRunning;
        }
    }
}
