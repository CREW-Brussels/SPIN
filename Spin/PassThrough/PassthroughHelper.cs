using System;
using UnityEngine;
using Wave.Native;

namespace PassThrough
{
    [Serializable]
    public class PassthroughHelper
    {
        [SerializeField] private Camera hmd;

        public void ShowPassthroughUnderlay(bool show)
        {
            if (show)
            {
                hmd.clearFlags = CameraClearFlags.SolidColor;
                hmd.backgroundColor = new Color(.15f, .25f, .15f, 0);
            }
            else
            {
                hmd.clearFlags = CameraClearFlags.Skybox;
            }

            Interop.WVR_ShowPassthroughUnderlay(show);
        }
    }
}