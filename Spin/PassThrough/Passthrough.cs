using UnityEngine;
using Wave.Essence;
using Wave.Native;

namespace PassThrough
{
    public class Passthrough : MonoBehaviour
    {
        [SerializeField] private PassthroughHelper passthroughHelper;

        // Update is called once per frame
        void Update()
        {
            if (ButtonFacade.YButtonPressed)
            {
                passthroughHelper.ShowPassthroughUnderlay(!Interop.WVR_IsPassthroughOverlayVisible());
            }
        }

        private void Start()
        {
            passthroughHelper.ShowPassthroughUnderlay(true);
        }

        private static class ButtonFacade
        {
            public static bool YButtonPressed =>
                WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Left, WVR_InputId.WVR_InputId_Alias1_Y);
        }
    }
}