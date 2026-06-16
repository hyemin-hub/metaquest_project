using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// VR 컨트롤러 입력 헬퍼 — OVRInput + UnityEngine.XR 동시 체크로 robust.
/// 어느 한쪽이 실패해도 다른 쪽이 작동하면 OK.
/// </summary>
public static class VRInputHelper
{
    // ===== Trigger (검지) =====
    public static bool IsTriggerPressed(float threshold = 0.5f)
    {
        // 1. OVRInput 시도
        try
        {
            float rT = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
            float lT = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
            if (rT > threshold || lT > threshold) return true;
        }
        catch { }

        // 2. UnityEngine.XR 시도
        var right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        var left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        float rTrigger = 0f, lTrigger = 0f;
        bool rTriggerBtn = false, lTriggerBtn = false;

        if (right.isValid)
        {
            right.TryGetFeatureValue(CommonUsages.trigger, out rTrigger);
            right.TryGetFeatureValue(CommonUsages.triggerButton, out rTriggerBtn);
        }
        if (left.isValid)
        {
            left.TryGetFeatureValue(CommonUsages.trigger, out lTrigger);
            left.TryGetFeatureValue(CommonUsages.triggerButton, out lTriggerBtn);
        }

        return rTrigger > threshold || lTrigger > threshold || rTriggerBtn || lTriggerBtn;
    }

    // ===== A 버튼 (오른손 primary) =====
    public static bool IsAButtonPressed()
    {
        try
        {
            if (OVRInput.Get(OVRInput.Button.One)) return true;
        }
        catch { }

        var right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        bool pressed = false;
        if (right.isValid) right.TryGetFeatureValue(CommonUsages.primaryButton, out pressed);
        return pressed;
    }

    // ===== B 버튼 (오른손 secondary) =====
    public static bool IsBButtonPressed()
    {
        try
        {
            if (OVRInput.Get(OVRInput.Button.Two)) return true;
        }
        catch { }

        var right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        bool pressed = false;
        if (right.isValid) right.TryGetFeatureValue(CommonUsages.secondaryButton, out pressed);
        return pressed;
    }

    // ===== X 버튼 (왼손 primary) =====
    public static bool IsXButtonPressed()
    {
        try
        {
            if (OVRInput.Get(OVRInput.Button.Three)) return true;
        }
        catch { }

        var left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        bool pressed = false;
        if (left.isValid) left.TryGetFeatureValue(CommonUsages.primaryButton, out pressed);
        return pressed;
    }

    // ===== Y 버튼 (왼손 secondary) =====
    public static bool IsYButtonPressed()
    {
        try
        {
            if (OVRInput.Get(OVRInput.Button.Four)) return true;
        }
        catch { }

        var left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        bool pressed = false;
        if (left.isValid) left.TryGetFeatureValue(CommonUsages.secondaryButton, out pressed);
        return pressed;
    }

    // ===== Grip (옆면) =====
    public static bool IsGripPressed(bool rightHand = true, float threshold = 0.5f)
    {
        try
        {
            float g = rightHand
                ? OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger)
                : OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger);
            if (g > threshold) return true;
        }
        catch { }

        var device = InputDevices.GetDeviceAtXRNode(rightHand ? XRNode.RightHand : XRNode.LeftHand);
        float grip = 0f;
        if (device.isValid) device.TryGetFeatureValue(CommonUsages.grip, out grip);
        return grip > threshold;
    }
}
