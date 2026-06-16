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

    [Header("카메라 자식으로 부착")]
    public bool attachToCamera = true;
    public Vector3 localOffset = new Vector3(-0.25f, 0.15f, 0.6f);
    public float localScale = 0.0015f;

    private bool attached = false;

    void Update()
    {
        // 카메라에 자식으로 부착 (매 프레임 확인 — 한번 부착되면 자동 따라감)
        if (attachToCamera && !attached)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                transform.SetParent(cam.transform);
                transform.localPosition = localOffset;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one * localScale;
                attached = true;
                Debug.Log($"[DebugInputDisplay] 카메라({cam.name})에 부착됨");
            }
        }
    }

    void LateUpdate()
    {
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
