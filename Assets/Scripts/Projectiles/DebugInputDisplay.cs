using UnityEngine;
using UnityEngine.XR;
using TMPro;

/// <summary>
/// VR 컨트롤러 입력 실시간 표시 — 어떤 입력이 잡히는지 화면에 보여줌.
/// 카메라에 따라가서 항상 시야에 보임.
/// </summary>
public class DebugInputDisplay : MonoBehaviour
{
    public TMP_Text displayText;

    [Header("카메라 따라가기 (항상 보이게)")]
    public bool followCamera = true;
    public Vector3 cameraOffset = new Vector3(-0.5f, 0.2f, 1.2f);

    void LateUpdate()
    {
        // 카메라 좌측 상단에 항상 보이게
        if (followCamera)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 targetPos = cam.transform.position
                    + cam.transform.right * cameraOffset.x
                    + cam.transform.up * cameraOffset.y
                    + cam.transform.forward * cameraOffset.z;
                transform.position = targetPos;
                transform.rotation = cam.transform.rotation;
            }
        }

        if (displayText == null) return;

        string s = "<color=#ffd23b>=== INPUT DEBUG ===</color>\n";

        // OVRInput 시도
        s += "<color=#7ad>--- OVRInput ---</color>\n";
        try
        {
            float rT = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
            float lT = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
            bool rA = OVRInput.Get(OVRInput.Button.One);
            bool rB = OVRInput.Get(OVRInput.Button.Two);
            bool lX = OVRInput.Get(OVRInput.Button.Three);
            bool lY = OVRInput.Get(OVRInput.Button.Four);
            s += $"R Trigger: {rT:F2}  L Trigger: {lT:F2}\n";
            s += $"A: {rA}  B: {rB}  X: {lX}  Y: {lY}\n";
        }
        catch (System.Exception e)
        {
            s += $"<color=red>OVRInput 실패: {e.Message}</color>\n";
        }

        // UnityEngine.XR
        s += "<color=#7ad>--- UnityEngine.XR ---</color>\n";
        var right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        var left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        s += $"R Valid: {right.isValid} ({right.name})\n";
        s += $"L Valid: {left.isValid} ({left.name})\n";

        if (right.isValid)
        {
            float t = 0f;
            bool a = false, b = false;
            right.TryGetFeatureValue(CommonUsages.trigger, out t);
            right.TryGetFeatureValue(CommonUsages.primaryButton, out a);
            right.TryGetFeatureValue(CommonUsages.secondaryButton, out b);
            s += $"R Trigger: {t:F2}  A: {a}  B: {b}\n";
        }
        if (left.isValid)
        {
            float t = 0f;
            bool x = false, y = false;
            left.TryGetFeatureValue(CommonUsages.trigger, out t);
            left.TryGetFeatureValue(CommonUsages.primaryButton, out x);
            left.TryGetFeatureValue(CommonUsages.secondaryButton, out y);
            s += $"L Trigger: {t:F2}  X: {x}  Y: {y}\n";
        }

        s += "\n<color=#7ad>--- VRInputHelper 결과 ---</color>\n";
        s += $"Trigger: <color=#46e26a>{VRInputHelper.IsTriggerPressed()}</color>\n";
        s += $"A: <color=#46e26a>{VRInputHelper.IsAButtonPressed()}</color>\n";
        s += $"B: <color=#46e26a>{VRInputHelper.IsBButtonPressed()}</color>\n";
        s += $"X: <color=#46e26a>{VRInputHelper.IsXButtonPressed()}</color>\n";
        s += $"Y: <color=#46e26a>{VRInputHelper.IsYButtonPressed()}</color>\n";

        displayText.text = s;
    }
}
